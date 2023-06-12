using System.Runtime.CompilerServices;
using UnityEngine;

namespace IcaNormal
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
    }
}