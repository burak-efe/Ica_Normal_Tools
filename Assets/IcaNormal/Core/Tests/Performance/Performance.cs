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


            SampleGroup group1 = new SampleGroup("Built In", SampleUnit.Millisecond);
            Measure.Method(() => mesh.RecalculateNormals()).SampleGroup(group1).MeasurementCount(10).Run();

            SampleGroup group2 = new SampleGroup("Ica", SampleUnit.Millisecond);
            Measure.Method(() => mesh.RecalculateNormalsIca(150)).SampleGroup(group2).MeasurementCount(20).Run();
            
            SampleGroup group3 = new SampleGroup("SD_TB", SampleUnit.Millisecond);
            Measure.Method(() => mesh.RecalculateNormals(150)).SampleGroup(group3).MeasurementCount(20).Run();
        }
        
        [Test, Performance]
        public void Sphere_90k_SmoothNormal_Uncached()
        {
            var o = new GameObject();
            MeshCreate.CreateUvSphere(o, 300, 300, 5f);
            var mesh = o.GetComponent<MeshFilter>().sharedMesh;


            SampleGroup group1 = new SampleGroup("Built In", SampleUnit.Millisecond);
            Measure.Method(() => mesh.RecalculateNormals()).SampleGroup(group1).MeasurementCount(10).Run();

            SampleGroup group2 = new SampleGroup("Ica", SampleUnit.Millisecond);
            Measure.Method(() => mesh.RecalculateNormalsIca(180)).SampleGroup(group2).MeasurementCount(20).Run();
            
            SampleGroup group3 = new SampleGroup("SD_TB", SampleUnit.Millisecond);
            Measure.Method(() => mesh.RecalculateNormals(180)).SampleGroup(group3).MeasurementCount(20).Run();
        }
    }
}