using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Ica.Normal
{
    public static class ExtensionMethods
    {
        public static void RecalculateNormalsIca(this Mesh mesh, float angle = 180f, bool alsoTangents = false)
        {
            var cache = new MeshDataCache();
            cache.Init(new List<Mesh>() { mesh }, alsoTangents);
            cache.RecalculateNormals(angle, alsoTangents);
            mesh.SetNormals(cache.NormalData.AsArray().Reinterpret<Vector3>());
            if (alsoTangents)
            {
                mesh.SetTangents(cache.TangentData.AsArray().Reinterpret<Vector4>());
            }
            cache.Dispose();
        }
    }
}
