using System.Runtime.CompilerServices;
using Unity.Collections;
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


        [MenuItem("CONTEXT/Renderer/RecalculateNormals", false, 1923)]
        public static void RecalculateNormalsOfMeshRenderer()
        {
            var objs = Selection.gameObjects;

            foreach (var o in objs)
            {
                if (o != null && o.GetComponent<Renderer>() != null)
                {
                    var rend = o.GetComponent<Renderer>();

                    if (rend is SkinnedMeshRenderer smr)
                    {
                        smr.sharedMesh.RecalculateNormals();
                    }
                    else if (rend is MeshRenderer)
                    {
                        o.GetComponent<MeshFilter>().sharedMesh.RecalculateNormals();
                    }
                }
            }
        }


        [MenuItem("CONTEXT/Renderer/RecalculateNormalsIca", false, 1924)]
        public static void RecalculateNormalsIca()
        {
            var objs = Selection.gameObjects;

            foreach (var o in objs)
            {
                if (o != null && o.GetComponent<Renderer>() != null)
                {
                    var rend = o.GetComponent<Renderer>();

                    if (rend is SkinnedMeshRenderer smr)
                    {
                        smr.sharedMesh.RecalculateNormalsIca();
                    }
                    else if (rend is MeshRenderer)
                    {
                        o.GetComponent<MeshFilter>().sharedMesh.RecalculateNormalsIca();
                    }
                }
            }
        }
    }
}