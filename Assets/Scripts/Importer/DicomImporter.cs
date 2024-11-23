using System;
using UnityEngine;

namespace UnityCTVisualizer {
    public class DicomImporter : IImporter {
        public string DatsetPath { get => m_DatasetPath; }
        public ColorDepth ColorDepth { get => m_ColorDepth; }

        private readonly string m_DatasetPath;
        private readonly ColorDepth m_ColorDepth;

        public DicomImporter(string dataset_path, string[] _) {
            m_DatasetPath = dataset_path;
        }

        public Metadata ImportMetadata() {
            throw new NotImplementedException();
        }

        public bool ImportChunk(UInt32 brick_id, int brick_size, Vector3Int dims, out byte[] data) {
            data = null;
            return true;
        }

        public bool ImportChunk(UInt32 brick_id, int brick_size, Vector3Int dims, out UInt16[] data) {
            data = null;
            return true;
        }

        public bool IsMetadataImportable { get => true; }
    }
}
