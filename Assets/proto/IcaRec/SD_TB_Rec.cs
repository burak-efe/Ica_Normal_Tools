using Ica.Tests.Shared;
using TB;
using UnityEngine;

namespace Ica.Normal
{
    public class SD_TB_Rec : MonoBehaviour
    {
        public int seg1;
        public int seg2;
        public float rad;

        public Mesh mesh;
        private void Start()
        {
            var a = new GameObject();
            MeshCreate.CreateUvSphere(a,seg1,seg2,rad);
            mesh = a.GetComponent<MeshFilter>().sharedMesh;

        }

        private void Update()
        {
            mesh.RecalculateNormals(180f);
        }
    }
}