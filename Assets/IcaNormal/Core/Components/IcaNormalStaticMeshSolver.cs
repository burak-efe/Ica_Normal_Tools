using Unity.Collections;
using UnityEngine;

namespace Ica.Normal
{
    public class IcaNormalStaticMeshSolver : MonoBehaviour
    {
        public Mesh TargetMesh;

        [Range(0f, 180f)]
        public float Angle = 180f;

        private void Start()
        {
            RecalculateNormals();
        }

        [ContextMenu("RecalculateNormals")]
        public void RecalculateNormals()
        {
            Ica.Normal.CachedParallelMethod.CalculateNormalDataUncached(TargetMesh, out var normals, Allocator.TempJob, Angle);
            
            TargetMesh.SetNormals(normals.AsArray().Reinterpret<Vector3>());

            normals.Dispose();
        }
    }
}