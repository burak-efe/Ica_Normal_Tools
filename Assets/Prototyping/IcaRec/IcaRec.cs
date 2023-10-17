// using Ica.Tests.Shared;
// using UnityEngine;
//
// namespace Ica.Normal
// {
//     public class IcaRec : MonoBehaviour
//     {
//         public int seg1;
//         public int seg2;
//         public float rad;
//
//         public Mesh mesh;
//         [Range(0,180)]
//         public float Angle;
//         private void Start()
//         {
//
//             var o = MeshCreate.CreateUvSphere(seg1,seg2,rad);
//             mesh = o.GetComponent<MeshFilter>().sharedMesh;
//
//         }
//
//         private void Update()
//         {
//             mesh.RecalculateNormalsIca(Angle);
//         }
//     }
//     
// }