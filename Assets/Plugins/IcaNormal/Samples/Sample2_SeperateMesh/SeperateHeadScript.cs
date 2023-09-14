using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ica.Normal.Samples
{
    public class SeperateHeadScript : MonoBehaviour
    {
        public List<SkinnedMeshRenderer> BuiltInSample;
        public List<SkinnedMeshRenderer> IcaSample;
        public IcaNormalMorphedMeshSolver Solver;
        


        void Update()
        {
            float sinWave = (0.5f * (1f + Mathf.Sin(2 * Mathf.PI * 0.5f * Time.time)) * 100);


            foreach (var skinnedMeshRenderer in BuiltInSample)
            {
                skinnedMeshRenderer.SetBlendShapeWeight(0,sinWave);
            }
            
            foreach (var skinnedMeshRenderer in IcaSample)
            {
                skinnedMeshRenderer.SetBlendShapeWeight(0,sinWave);
            }
            Solver.RecalculateNormals();
        }
    }
}