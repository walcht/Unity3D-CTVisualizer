using System;
using UnityEngine;

namespace UnityCTVisualizer {

    public interface IImporter {
        ColorDepth ColorDepth { get; }
        string DatsetPath { get; }


        /// <summary>
        ///     Imports a single volume brick (i.e., a subset of the 3D volume) from a provided CT dataset.
        /// </summary>
        /// 
        /// <remarks>
        ///     Import will fail if chunk size in bytes exceeds the maximum size for an object allowed
        ///     by .NET (i.e., 2GBs). No size checks are performed in this function.
        /// </remarks>
        /// 
        /// <param name="dataset_path">UVDS root directory path</param>
        /// 
        /// <param name="metadata">UVDS metadata</param>
        /// 
        /// <param name="chunk_id">the id of the chunk to be imported. The file <paramref name="chunk_id"/>.uvds should exist
        /// in the provided resolution level subdirectory of the UVDS path</param>
        /// 
        /// <param name="resolution_lvl">resolution level of the requested chunk. The folder <paramref name="resolution_lvl"/>
        /// should exist as a direct subdirectory of the UVDS path</param>
        /// 
        /// <returns>byte array of the requested chunk. TODO: describe the expected layout</returns>
        /// 
        bool ImportChunk(UInt32 brick_id, int brickSize, Vector3Int dims, out byte[] chunk_data);

        /// <summary>
        ///     Imports a single volume brick (i.e., a subset of the 3D volume) from a provided CT dataset.
        /// </summary>
        /// 
        /// <remarks>
        ///     Import will fail if chunk size in bytes exceeds the maximum size for an object allowed
        ///     by .NET (i.e., 2GBs). No size checks are performed in this function.
        /// </remarks>
        /// 
        /// <param name="dataset_path">UVDS root directory path</param>
        /// 
        /// <param name="metadata">UVDS metadata</param>
        /// 
        /// <param name="chunk_id">the id of the chunk to be imported. The file <paramref name="chunk_id"/>.uvds should exist
        /// in the provided resolution level subdirectory of the UVDS path</param>
        /// 
        /// <param name="resolution_lvl">resolution level of the requested chunk. The folder <paramref name="resolution_lvl"/>
        /// should exist as a direct subdirectory of the UVDS path</param>
        /// 
        /// <returns>byte array of the requested chunk. TODO: describe the expected layout</returns>
        /// 
        bool ImportChunk(UInt32 brick_id, int brickSize, Vector3Int dims, out UInt16[] chunk_data);
        bool IsMetadataImportable { get; }
        Metadata ImportMetadata();
    }

    public class Metadata {
        public string DatasetPath { get; private set; }
        public int OriginalImageWidth { get; private set; }
        public int OriginalImageHeight { get; private set; }
        public int OriginNbrSlices { get; private set; }
        public ColorDepth ColorDepth { get; private set; }
        public Vector3 Scale { get; private set; }
        public Vector3 EulerRotation { get; private set; }

        public Metadata(string dataset_path, int original_img_widht, int original_img_height,
            int original_nbr_slices, ColorDepth color_depth, Vector3 scale, Vector3 euler_rotation) {
            DatasetPath = dataset_path;
            OriginalImageWidth = original_img_widht;
            OriginalImageHeight = original_img_height;
            OriginNbrSlices = original_nbr_slices;
            ColorDepth = color_depth;
            Scale = scale;
            EulerRotation = euler_rotation;
        }
    }

    public enum ColorDepth {
        UINT8, UINT16, UNKNOWN
    }
}
