using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace Ica.Normal.Editor
{
    public static class SmrUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyBlendShapes(SkinnedMeshRenderer from, SkinnedMeshRenderer to)
        {
            for (int i = 0; i < from.sharedMesh.blendShapeCount; i++)
            {
                to.SetBlendShapeWeight(i, from.GetBlendShapeWeight(i));
            }
        }

        [MenuItem("CONTEXT/SkinnedMeshRenderer/RecalculateNormals", false, 1923)]
        public static void  RecalculateNormals()
        {
            var objs = Selection.gameObjects;

            foreach (var o in objs)
            {
                if (o != null && o.GetComponent<SkinnedMeshRenderer>() != null)
                {
                     o.GetComponent<SkinnedMeshRenderer>().sharedMesh.RecalculateNormals();
 
                }
            }
        }
    }
    }

