using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityCTVisualizer {
    public class ImageSequenceImporter : IImporter {
        public string DatsetPath { get => m_DatasetPath; }
        public ColorDepth ColorDepth { get => m_ColorDepth; }

        private readonly string m_DatasetPath;
        private readonly string[] m_Filepaths;
        private readonly ColorDepth m_ColorDepth;

        public ImageSequenceImporter(string dataset_path, string[] fps) {
            m_DatasetPath = dataset_path;
            m_Filepaths = fps;
            int depth = (new OpenCvSharp.Mat(m_Filepaths[0], OpenCvSharp.ImreadModes.Grayscale)).Depth();
            for (int i = 1; i < m_Filepaths.Length; ++i) {
                var mat = new OpenCvSharp.Mat(m_Filepaths[i], OpenCvSharp.ImreadModes.Grayscale);
                if (mat.Depth() != depth) throw new BadImageFormatException($"image formats in the dataset are not consistent");
            }
            switch (depth) {
                case OpenCvSharp.MatType.CV_8U:
                m_ColorDepth = ColorDepth.UINT8;
                break;
                case OpenCvSharp.MatType.CV_16U:
                m_ColorDepth = ColorDepth.UINT16;
                break;
                default:
                throw new BadImageFormatException($"image color depth: {depth} is not supported");
            }
        }

        public Metadata ImportMetadata() {
            throw new NotImplementedException("this importer does not support importing of metadata. "
                + "Check IsMetadataImportable before calling this.");
        }

        public bool ImportChunk(UInt32 brick_id, int brick_size, Vector3Int dims, out byte[] data) {
            return ImportChunkInternal<byte>(brick_id, brick_size, dims, out data);
        }
        public bool ImportChunk(UInt32 brick_id, int brick_size, Vector3Int dims, out UInt16[] data) {
            return ImportChunkInternal<UInt16>(brick_id, brick_size, dims, out data);
        }

        private bool ImportChunkInternal<T>(UInt32 brick_id, int brick_size, Vector3Int dims, out T[] data) where T : unmanaged {
            Vector3Int nbr_chunks = new(
                Mathf.CeilToInt(dims.x / (float)brick_size),
                Mathf.CeilToInt(dims.y / (float)brick_size),
                Mathf.CeilToInt(dims.z / (float)brick_size)
            );

            int brick_idx = (int)(brick_id & 0x03FFFFFF);
            int resolution_lvl = (int)(brick_id >> 26);
            if (resolution_lvl > 0) {
                data = null;
                return false;
                // throw new NotImplementedException("lower resolution levels are not yet implemented");
            }

            int pad_right = brick_size * nbr_chunks.x - dims.x;
            int pad_top = brick_size * nbr_chunks.y - dims.y;

            int slice_start_idx = (nbr_chunks.z - 1 - brick_idx / (nbr_chunks.x * nbr_chunks.y)) * brick_size;
            int col_start = ((brick_idx % (nbr_chunks.x * nbr_chunks.y)) % nbr_chunks.x) * brick_size;
            int row_start = (nbr_chunks.y - 1 - (brick_idx % (nbr_chunks.x * nbr_chunks.y)) / nbr_chunks.y) * brick_size;

            // in C# this array is initialized with 0s
            data = new T[brick_size * brick_size * brick_size];
            int buf_offset = 0;
            // loop through brick_size slices
            for (int i = 0; i < brick_size; ++i) {
                int slice_idx = slice_start_idx + i; // slice_start_idx - i;
                                                     // check if this is a padding slice (slice filled with 0s)
                if (slice_idx >= dims.z) continue;
                // load current slice - multichannel volumes are not yet supported
                var mat = new OpenCvSharp.Mat(m_Filepaths[slice_idx], OpenCvSharp.ImreadModes.Grayscale);
                // add xy padding (if necessary)
                var padded_mat = mat.CopyMakeBorder(pad_top, 0, 0, pad_right, OpenCvSharp.BorderTypes.Constant, OpenCvSharp.Scalar.All(0));
                padded_mat.SubMat(row_start, row_start + brick_size, col_start, col_start + brick_size).GetArray<T>(out T[] brick_slide_data);
                Assert.IsTrue(brick_slide_data.Length == (brick_size * brick_size));
                Array.Copy(brick_slide_data, 0, data, buf_offset, brick_slide_data.Length);
                buf_offset += brick_size * brick_size;
            }
            return true;
        }

        public bool IsMetadataImportable { get => false; }

    }
}
