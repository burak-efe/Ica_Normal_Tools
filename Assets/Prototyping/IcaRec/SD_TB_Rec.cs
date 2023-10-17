// using Ica.Tests.Shared;
// using TB;
// using UnityEngine;
//
// namespace Ica.Normal
// {
//     public class SD_TB_Rec : MonoBehaviour
//     {
//         public int seg1;
//         public int seg2;
//         public float rad;
//
//         public Mesh mesh;
//         private void Start()
//         {
//
//             var o =  MeshCreate.CreateUvSphere(seg1,seg2,rad);
//             mesh = o.GetComponent<MeshFilter>().sharedMesh;
//
//         }
//
//         private void Update()
//         {
//             mesh.RecalculateNormals(180f);
//         }
//     }
// }