using System.Collections.Generic;
using UnityEngine;
using IcaNormal;
using Unity.Collections;
using Unity.Mathematics;

namespace Tests
{
    public static class TestUtils
    {
        public static bool IsNormalsAreSameForSamePosition(Mesh mesh)
        {
            var mda = Mesh.AcquireReadOnlyMeshData(mesh);
            var data = mda[0];
            data.GetVerticesWithNewContainer(out var vertices, Allocator.Temp);
            data.GetNormalsWithNewContainer(out var normals, Allocator.Temp);
            mda.Dispose();

            VertexPositionMapper.GetVertexPosHashMap(vertices, out var vertexPosMap, Allocator.Temp);
            foreach (var pair in vertexPosMap)
            {
                if (pair.Value.Length > 1)
                {
                    float3 normalToCompare = normals[pair.Value.ElementAt(0)];
                    foreach (int vertexIndex in pair.Value)
                    {
                        if (!normals[vertexIndex].Equals(normalToCompare))
                        {
                            Debug.LogError("normals are not same " + normals[vertexIndex] + " and " + normalToCompare);
                            return false;
                        }
                    }
                }
            }

            Debug.Log("All Normals are some for vertices on same positions");
            return true;
        }

        public static bool IsEveryNormalAreUnitVectors(Mesh mesh, float precision)
        {
            var mda = Mesh.AcquireReadOnlyMeshData(mesh);
            var data = mda[0];
            data.GetNormalsWithNewContainer(out var normals, Allocator.Temp);
            mda.Dispose();
            var countOfNonUnit = 0;

            for (int i = 0; i < normals.Length; i++)
            {
                var length = math.length(normals[i]);
                var dif = 1f - length;
                if (math.abs(dif) > precision)
                {
                    countOfNonUnit++;
                    //Debug.LogError("Normal at index " +i+ " not normalized "+ length );
                    //return false;
                }
            }


            if (countOfNonUnit > 0)
            {
                Debug.LogError(countOfNonUnit + " normals are not unit vector with precision of "+ precision +", which is " + (countOfNonUnit * 100) / mesh.vertexCount + " Percent of mesh!");
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