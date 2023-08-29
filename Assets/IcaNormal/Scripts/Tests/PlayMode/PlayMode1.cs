using System.Collections;
using System.Collections.Generic;
using Ica.Tests.Shared;
using Ica.Utils;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TestTools;

namespace Ica.Normal.Tests.PlayMode
{
    public class PlayMode1
    {
        [Test]
        public void Check_Split_Geometry_Sphere()
        {
            var obj = Ica.Utils.Editor.AssetUtils.FindAndInstantiateAsset("SphereFromTwoHalfGeometryPrefab");
            var solver = obj.GetComponent<RuntimeNormalSolver>();
            solver.Init();
            solver.RecalculateNormals();
            var mesh = obj.GetComponent<SkinnedMeshRenderer>().sharedMesh;
            var mda = Mesh.AcquireReadOnlyMeshData(mesh);

            var vertices = new NativeList<float3>(1, Allocator.Temp);
            var normals = new NativeList<float3>(1, Allocator.Temp);
            mda[0].GetVerticesDataAsList(ref vertices);
            mda[0].GetNormalsDataAsList(ref normals);
            Assert.IsTrue(vertices.Length == mesh.vertexCount);
            Assert.IsTrue(normals.Length == mesh.vertexCount);
            Assert.IsTrue(TestUtils.IsNormalsAreSameForSamePosition(vertices, normals));
        }

         [Test]
         public void Is_All_Normals_are_Normalized()
         {
             var obj = Ica.Utils.Editor.AssetUtils.FindAndInstantiateAsset("SphereFromTwoHalfGeometryPrefab");
             var solver = obj.GetComponent<RuntimeNormalSolver>();
             solver.Init();
             solver.RecalculateNormals();
             var mesh = obj.GetComponent<SkinnedMeshRenderer>().sharedMesh;
             Assert.IsTrue(TestUtils.IsEveryNormalAreUnitVectors(mesh, 0.000001f));
         }
    }
}