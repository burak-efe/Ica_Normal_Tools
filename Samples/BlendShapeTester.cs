using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ica.Normal.Samples
{
    public class BlendShapeTester : MonoBehaviour
    {
        [Range(0,3)]
        public float Frequency = 1f;
        public List<SkinnedMeshRenderer> BuiltInSample;
        public List<SkinnedMeshRenderer> IcaSample;
        public IcaNormalMorphedMeshSolver Solver;


        void Update()
        {
            float sinWave = (0.5f * (1f + Mathf.Sin(2 * Mathf.PI * Frequency * Time.time)) * 100);


            foreach (var skinnedMeshRenderer in BuiltInSample)
            {
                for (int i = 0; i < skinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
                {
                    skinnedMeshRenderer.SetBlendShapeWeight(i, sinWave);
                }
            }

            foreach (var skinnedMeshRenderer in IcaSample)
            {
                for (int i = 0; i < skinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
                {
                    skinnedMeshRenderer.SetBlendShapeWeight(i, sinWave);
                }
            }

            Solver.RecalculateNormals();
        }
    }
}