using Ica.Normal;
using NUnit.Framework;
using Tests;
using UnityEngine;

namespace Ica.Normal.Tests
{
    public class Tests
    {
        [Test]
        public void Check_Split_Geometry_Sphere()
        {
            var asset = Resources.Load<GameObject>("SphereFromTwoHalfGeometryPrefab");
            var obj = Object.Instantiate(asset);
            var solver = obj.GetComponent<RuntimeNormalSolver>();
            solver.Init();
            solver.RecalculateNormals();
            var mesh = obj.GetComponent<SkinnedMeshRenderer>().sharedMesh;
            Assert.IsTrue(TestUtils.IsNormalsAreSameForSamePosition(mesh));
        }

        [Test]
        public void Is_All_Normals_are_Normalized()
        {
            var asset = Resources.Load<GameObject>("SphereFromTwoHalfGeometryPrefab");
            var obj = Object.Instantiate(asset);
            var solver = obj.GetComponent<RuntimeNormalSolver>();
            solver.Init();
            solver.RecalculateNormals();
            var mesh = obj.GetComponent<SkinnedMeshRenderer>().sharedMesh;
            Assert.IsTrue(TestUtils.IsEveryNormalAreUnitVectors(mesh, 0.000001f));
        }
    }
}