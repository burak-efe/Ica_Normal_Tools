using System.Collections.Generic;
using Ica.Utils;
using UnityEngine;
using Ica.Normal;
using Unity.Collections;
using Unity.Mathematics;

namespace Ica.Tests.Shared
{
    public static class TestUtils
    {
        public static bool IsNormalsAreSameForSamePosition(NativeList<float3> vertices, NativeList<float3> normals)
        {
            int incorrectCount = 0;
            VertexPositionMapper.GetVertexPosHashMap(vertices.AsArray(), out var vertexPosMap, Allocator.Temp);
            foreach (var pair in vertexPosMap)
            {
                if (pair.Value.Length > 1)
                {
                    float3 normalToCompare = normals[pair.Value.ElementAt(0)];
                    foreach (int vertexIndex in pair.Value)
                    {
                        if (!normals[vertexIndex].Equals(normalToCompare))
                        {
                            incorrectCount++;
                            var dif = normals[vertexIndex] - normalToCompare;
                            var totalDif = math.abs(dif.x) + math.abs(dif.y) + math.abs(dif.z);
                            Debug.Log("total diff is " + totalDif.ToString("N10") );
                        }

                    }
                }
            }

            if (incorrectCount > 0)
            {
                Debug.LogError($"normals are not same for {incorrectCount} vertices, which is {incorrectCount * 100 / vertices.Length} percent  of the all vertices");
                return false;
            }

            return true;
        }

        public static bool IsEveryNormalAreUnitVectors(Mesh mesh, float precision)
        {
            var mda = Mesh.AcquireReadOnlyMeshData(mesh);
            var data = mda[0];
            data.AllocAndGetNormalsDataAsArray(out var normals, Allocator.Temp);
            mda.Dispose();
            var countOfNonUnit = 0;

            for (int i = 0; i < normals.Length; i++)
            {
                var length = math.length(normals[i]);
                var dif = 1f - length;
                if (math.abs(dif) > precision)
                {
                    countOfNonUnit++;
                }
            }

            if (countOfNonUnit > 0)
            {
                Debug.LogError(countOfNonUnit + " normals are not unit vector with precision of " + precision + ", which is " + (countOfNonUnit * 100) / mesh.vertexCount + " Percent of mesh!");
                return false;
            }
            else
            {
                Debug.Log("All Normals are unit vector, with precision of " + precision);
                return true;
            }
        }
    }
}