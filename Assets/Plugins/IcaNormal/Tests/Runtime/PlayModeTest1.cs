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
    public class PlayModeTest1
    {
        [Test]
        public void CachedSmoothMethod_TwoCubes_NormalsAreSameForSamePosition()
        {
            var cubeTop = MeshCreate.CreateCube(new Vector3(0, 0.5f, 0), new Vector3(1, 1, 1));
            var cubeBottom = MeshCreate.CreateCube(new Vector3(0, -0.5f, 0), new Vector3(1, 1, 1));

            //var mda = Mesh.AcquireReadOnlyMeshData(new List<Mesh>() { cubeTop.GetComponent<MeshFilter>().sharedMesh, cubeBottom.GetComponent<MeshFilter>().sharedMesh, });

            var mdc = new MeshDataCache();
            mdc.InitFromMultipleMesh(new List<Mesh>() { cubeTop.GetComponent<MeshFilter>().sharedMesh, cubeBottom.GetComponent<MeshFilter>().sharedMesh, }, false);
            mdc.RecalculateNormals(180f,false);
            
            Assert.IsTrue(TestUtils.IsNormalsAreSameForSamePosition(mdc.VertexData, mdc.NormalData));
        }


        [Test]
        public void UncachedSmoothMethod_AllNormals_ShouldNormalized()
        {
            var obj = MeshCreate.CreateUvSphere(10, 10, 1);
            var mesh = obj.GetComponent<MeshFilter>().sharedMesh;

            mesh.RecalculateNormalsIca();
            Assert.IsTrue(TestUtils.IsEveryNormalAreUnitVectors(mesh, 0.000001f));
        }

        [Test]
        public void UncachedAngledMethod_AllNormals_ShouldNormalized()
        {
            var obj = MeshCreate.CreateUvSphere(10, 10, 1);
            var mesh = obj.GetComponent<MeshFilter>().sharedMesh;

            mesh.RecalculateNormalsIca(5f);
            Assert.IsTrue(TestUtils.IsEveryNormalAreUnitVectors(mesh, 0.000001f));
        }
    }
}