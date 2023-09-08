using System.Collections;
using System.Collections.Generic;
using Ica.Tests.Shared;
using NUnit.Framework;
using TB;
using Unity.Collections;
using Unity.PerformanceTesting;
using UnityEngine;

namespace Ica.Normal.Tests.Performance
{
    public class Performance
    {
        [Test, Performance]
        public void Sphere_90k_Normal_Uncached()
        {
            var o = new GameObject();
            MeshCreate.CreateUvSphere(o, 300, 300, 5f);
            var mesh = o.GetComponent<MeshFilter>().sharedMesh;


            SampleGroup group1 = new SampleGroup("Vector3Length", SampleUnit.Microsecond);
            Measure.Method(() => mesh.RecalculateNormals()).SampleGroup(group1).MeasurementCount(10).Run();

            SampleGroup group2 = new SampleGroup("Vector3SquaredLength", SampleUnit.Microsecond);
            Measure.Method(() => CachedParallelMethod.CalculateNormalDataUncached(mesh,out var normals,Allocator.TempJob)).SampleGroup(group2).MeasurementCount(10).Run();
        }
    }
}