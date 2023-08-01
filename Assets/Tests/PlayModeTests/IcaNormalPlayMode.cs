using System.Collections;
using System.Collections.Generic;
using IcaNormal;
using NUnit.Framework;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayMode1
{
    // A Test behaves as an ordinary method
    [Test]
    public void Check_Split_Geometry_Sphere()
    {
        // Use the Assert class to test conditions
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


    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    // [UnityTest]
    // public IEnumerator PlayMode1WithEnumeratorPasses()
    // {
    //     // Use the Assert class to test conditions.
    //     // Use yield to skip a frame.
    //     yield return null;
    // }
}