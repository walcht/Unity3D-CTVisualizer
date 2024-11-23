using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityCTVisualizer;
using System;
using System.IO;

public class ChunkedVolumetricDatasetImporter : IImporter {
    public string DatsetPath { get => m_DatasetPath; }
    public ColorDepth ColorDepth { get => m_ColorDepth; }

    private readonly string m_DatasetPath;
    private readonly ColorDepth m_ColorDepth;

    public ChunkedVolumetricDatasetImporter(string dataset_path) {
        m_DatasetPath = dataset_path;
    }

    public bool ImportChunk(UInt32 brick_id, int brick_size, Vector3Int dims, out byte[] chunk_data) {
        var chunk_fp = Path.Combine(m_DatasetPath, $"chunk_{brick_id}.uvds");
        if (!File.Exists(chunk_fp)) {
            chunk_data = null;
            return false;
        }
        chunk_data = File.ReadAllBytes(chunk_fp);
        return true;
    }

    public bool ImportChunk(UInt32 brick_id, int brick_size, Vector3Int dims, out UInt16[] chunk_data) {
        string chunk_fp = Path.Combine(m_DatasetPath, $"chunk_{brick_id}.uvds");
        if (!File.Exists(chunk_fp)) {
            chunk_data = null;
            return false;
        }
        chunk_data = new UInt16[8];
        using (FileStream fs = File.OpenRead(chunk_fp)) {
            int i = 0;
            while (fs.Position < fs.Length) {
                UInt16 val = (UInt16)((fs.ReadByte() << 8) | fs.ReadByte());
                chunk_data[i] = val;
                ++i;
            }
        }
        return true;
    }

    public bool IsMetadataImportable { get => true; }

    public Metadata ImportMetadata() {
        float SCALE_DEFAULT = -1.0f;
        string metadata_fp;
        string[] volume_chunks_fps;

        try {
            metadata_fp = Directory.GetFiles(m_DatasetPath, "metadata.txt")[0];
            volume_chunks_fps = Directory.GetFiles(m_DatasetPath, "*.uvds");
            Array.Sort(volume_chunks_fps);
        } catch {
            throw new FileLoadException("Failed to extract UVDS dataset from provided directory");
        }
        int original_img_width = -1;
        int original_img_height = -1;
        int original_nbr_slices = -1;
        ColorDepth color_depth = ColorDepth.UNKNOWN;
        using (StreamReader sr = new(metadata_fp)) {
            string line;
            Vector3 scale = new(1, 1, 1);
            Vector3 euler_rotation = new(1, 1, 1);
            while ((line = sr.ReadLine()) != null) {
                string[] split = line.Split("=");
                switch (split[0]) {
                    case "originalimagewidth":
                    original_img_width = int.Parse(split[1]);
                    break;
                    case "originalimageheight":
                    original_img_height = int.Parse(split[1]);
                    break;
                    case "originalnbrslices":
                    original_nbr_slices = int.Parse(split[1]);
                    break;
                    case "colordepth":
                    switch (split[1]) {
                        case "8":
                        color_depth = ColorDepth.UINT8;
                        break;
                        case "16":
                        color_depth = ColorDepth.UINT16;
                        break;
                        default:
                        throw new Exception($"unknown color depth value: {split[1]}");
                    }

                    break;
                    case "voxeldimX": {
                        float voxelDimX = float.Parse(split[1]);
                        if (voxelDimX == SCALE_DEFAULT) {

                            scale.y = 1.0f;
                            Debug.LogWarning(
                                "Voxel dimension X has invalid default value."
                                    + "Default scale 1 is used for the volume along the X axis. The dimensions of the "
                                    + "volume no longer reflect its dimensions in reality!"
                            );
                            break;
                        }
                        scale.y = (voxelDimX / 1000.0f) * original_img_width;
                        break;
                    }
                    case "voxeldimY": {
                        float voxelDimY = float.Parse(split[1]);
                        if (voxelDimY == SCALE_DEFAULT) {

                            scale.y = 1.0f;
                            Debug.LogWarning(
                                "Voxel dimension Y has invalid default value."
                                    + "Default scale 1 is used for the volume along the Y axis. The dimensions of the "
                                    + "volume no longer reflect its dimensions in reality!"
                            );
                            break;
                        }
                        scale.y = (voxelDimY / 1000.0f) * original_img_height;
                        break;
                    }
                    case "voxeldimZ": {
                        float voxelDimZ = float.Parse(split[1]);
                        if (voxelDimZ == SCALE_DEFAULT) {
                            scale.z = 1.0f;
                            Debug.LogWarning(
                                "Voxel dimension Z has invalid default value."
                                    + "Default scale 1 is used for the volume along the Z axis. The dimensions of the "
                                    + "volume no longer reflect its dimensions in reality!"
                            );
                            break;
                        }
                        scale.z = (voxelDimZ / 1000.0f) * original_nbr_slices;
                        break;
                    }
                    case "eulerrotX":
                    euler_rotation.x = float.Parse(split[1]);
                    break;
                    case "eulerrotY":
                    euler_rotation.y = float.Parse(split[1]);
                    break;
                    case "eulerrotZ":
                    euler_rotation.z = float.Parse(split[1]);
                    break;
                    default:
                    break;
                }
            }
            return new Metadata(m_DatasetPath, original_img_width, original_img_height, original_nbr_slices,
                color_depth, scale, euler_rotation);
        }
    }

}
