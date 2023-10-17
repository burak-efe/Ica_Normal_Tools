using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Ica.Normal
{
    public static class ExtensionMethods
    {
        public static void RecalculateNormalsIca(this Mesh mesh, float angle = 180f)
        {
            var mda = Mesh.AcquireReadOnlyMeshData(mesh);
            angle = math.clamp(angle, 0, 180);
            UncachedMethod.UncachedNormalRecalculate(mda[0], out var outNormals, Allocator.TempJob, angle);
            mesh.SetNormals(outNormals.AsArray().Reinterpret<Vector3>());
            outNormals.Dispose();
            mda.Dispose();
        }
    }
}