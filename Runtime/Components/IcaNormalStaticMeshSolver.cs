using System.Collections.Generic;
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
            TargetMesh.RecalculateNormalsIca(Angle);
           var mda = Mesh.AcquireReadOnlyMeshData(TargetMesh);


        }
    }
}