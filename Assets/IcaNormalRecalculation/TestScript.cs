using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace IcaNormal
{
    public class TestScript : MonoBehaviour
    {
        public int Size;
        // public int[] Keys;
        // public int[] Values;
        private void Start()
        {
            ConcTest();
        }

        private void ConcTest()
        {
            var map = new NativeParallelHashMap<int, int>(Size,Allocator.TempJob);
            var pwriter = map.AsParallelWriter();
            var job = new ConcJob
            {
                Map = pwriter
            };

            var handle = job.ScheduleParallel(Size, Size / 32,default);
            handle.Complete();


            foreach (var kvp in map)
            {
                Debug.Log(kvp.Key + " " + kvp.Value);
            }

            map.Dispose();
        }
        
        [BurstCompile]
        private struct ConcJob : IJobFor
        {
            public NativeParallelHashMap<int, int>.ParallelWriter Map;
            public void Execute(int index)
            {
                Map.TryAdd(index*2,index*4);
            }
        }
    }
}