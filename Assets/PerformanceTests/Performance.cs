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
            var o = MeshCreate.CreateUvSphere(300, 300, 5f);
            var mesh = o.GetComponent<MeshFilter>().sharedMesh;


            SampleGroup group1 = new SampleGroup("Built In", SampleUnit.Millisecond);
            Measure.Method(() => mesh.RecalculateNormals()).SampleGroup(group1).MeasurementCount(10).Run();

            SampleGroup group2 = new SampleGroup("Ica Uncached Angled", SampleUnit.Millisecond);
            Measure.Method(() => mesh.RecalculateNormalsIca(179)).SampleGroup(group2).MeasurementCount(10).Run();

            SampleGroup group4 = new SampleGroup("Ica Uncached Smooth", SampleUnit.Millisecond);
            Measure.Method(() => mesh.RecalculateNormalsIca(180)).SampleGroup(group4).MeasurementCount(10).Run();

            SampleGroup group3 = new SampleGroup("SD_TB", SampleUnit.Millisecond);
            Measure.Method(() => mesh.RecalculateNormals(179)).SampleGroup(group3).MeasurementCount(10).Run();
        }


        [Test, Performance]
        public void Sphere_90k_Normal_Cached()
        {
            var o = MeshCreate.CreateUvSphere(300, 300, 5f);
            var mesh = o.GetComponent<MeshFilter>().sharedMesh;
            var mdc = new MeshDataCache();
            mdc.Init(new List<Mesh>() { mesh }, false);

            SampleGroup group2 = new SampleGroup("Built In", SampleUnit.Millisecond);
            Measure.Method(() => mesh.RecalculateNormals()).SampleGroup(group2).MeasurementCount(10).Run();

            SampleGroup group0 = new SampleGroup("Ica Cached Smooth", SampleUnit.Millisecond);
            Measure.Method(() => mdc.RecalculateNormals(180f, false)).SampleGroup(group0).MeasurementCount(10).Run();

            SampleGroup group1 = new SampleGroup("Ica Cached Angled", SampleUnit.Millisecond);
            Measure.Method(() => mdc.RecalculateNormals(179f, false)).SampleGroup(group1).MeasurementCount(10).Run();

            mdc.Dispose();
        }

        [Test, Performance]
        public void Sphere_90k_NormalAndTangent_Cached()
        {
            var o = MeshCreate.CreateUvSphere(300, 300, 5f);
            var mesh = o.GetComponent<MeshFilter>().sharedMesh;
            var mdc = new MeshDataCache();
            mdc.Init(new List<Mesh>() { mesh }, true);

            SampleGroup group1 = new SampleGroup("Built In", SampleUnit.Millisecond);
            Measure.Method(() =>
            {
                mesh.RecalculateNormals();
                mesh.RecalculateTangents();
            }).SampleGroup(group1).MeasurementCount(10).Run();

            SampleGroup group2 = new SampleGroup("Ica Cached Smooth", SampleUnit.Millisecond);
            Measure.Method(() => mdc.RecalculateNormals(180, true)).SampleGroup(group2).MeasurementCount(10).Run();

            SampleGroup group3 = new SampleGroup("Ica Cached Angled", SampleUnit.Millisecond);
            Measure.Method(() => mdc.RecalculateNormals(179, true)).SampleGroup(group3).MeasurementCount(10).Run();

            mdc.Dispose();
        }
    }
}