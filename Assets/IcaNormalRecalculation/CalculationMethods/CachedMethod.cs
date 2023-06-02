using System.Collections.Generic;
using UnityEngine;

namespace IcaNormal
{
    public static class CachedMethod
    {
        public static void CalculateNormalData(Mesh mesh, float angle, List<MeshDataCache.DuplicateMap> duplicateMap, ref Vector3[] normalOut, ref Vector4[] tangentOut)
        {
            var _normalsList = new List<Vector3>(mesh.vertexCount);
            var _tangentsList = new List<Vector4>(mesh.vertexCount);
            mesh.RecalculateNormals();
            mesh.GetNormals(_normalsList);
            mesh.RecalculateTangents();
            mesh.GetTangents(_tangentsList);

            var mapCount = duplicateMap.Count;
            
            for (int vertPos = 0; vertPos < mapCount; vertPos++)
            {
                Vector3 normalSum = Vector3.zero;
                Vector4 tangentSum = Vector4.zero;
                
                var length = duplicateMap[vertPos].DuplicateIndexes.Length;
                
                for (int i = 0; i < length; i++)
                {
                    normalSum += _normalsList[duplicateMap[vertPos].DuplicateIndexes[i]];
                    tangentSum += _tangentsList[duplicateMap[vertPos].DuplicateIndexes[i]];
                }

                normalSum = normalSum.normalized;
                Vector3 tangXYZ = new Vector3(tangentSum.x, tangentSum.y, tangentSum.z);
                tangXYZ = tangXYZ.normalized;
                tangentSum = new Vector4(tangXYZ.normalized.x, tangXYZ.y, tangXYZ.z, Mathf.Clamp(tangentSum.w, -1f, 1f));
                for (int i = 0; i < length; i++)
                {
                    _normalsList[duplicateMap[vertPos].DuplicateIndexes[i]] = normalSum;
                    _tangentsList[duplicateMap[vertPos].DuplicateIndexes[i]] = tangentSum;
                }
            }
        }
    }
}