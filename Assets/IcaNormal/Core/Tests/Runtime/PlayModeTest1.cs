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
        // [Test]
        // public void Check_Split_Geometry_Sphere()
        // {
        //     var obj = Ica.Utils.Editor.AssetUtils.FindAndInstantiateAsset("SphereFromTwoHalfGeometryPrefab");
        //     var solver = obj.GetComponent<IcaNormalMorphedMeshSolver>();
        //     solver.RecalculateOnStart = false;
        //     solver.Init();
        //     
        //     //smooth all branch
        //     solver.Angle = 180f;
        //     solver.RecalculateNormals();
        //     Assert.IsTrue(TestUtils.IsNormalsAreSameForSamePosition(solver._meshDataCache.VertexData, solver._meshDataCache.NormalData));
        //     
        //     //angle respected branch
        //     solver.Angle = 175f;
        //     solver.RecalculateNormals();
        //     Assert.IsTrue(TestUtils.IsNormalsAreSameForSamePosition(solver._meshDataCache.VertexData, solver._meshDataCache.NormalData));
        //     
        // }
        //
        //  [Test]
        //  public void Is_All_Normals_are_Normalized()
        //  {
        //      var obj = Ica.Utils.Editor.AssetUtils.FindAndInstantiateAsset("SphereFromTwoHalfGeometryPrefab");
        //      var solver = obj.GetComponent<IcaNormalMorphedMeshSolver>();
        //      solver.Init();
        //      solver.RecalculateNormals();
        //      var mesh = obj.GetComponent<SkinnedMeshRenderer>().sharedMesh;
        //      Assert.IsTrue(TestUtils.IsEveryNormalAreUnitVectors(mesh, 0.000001f));
        //  }
    }
}