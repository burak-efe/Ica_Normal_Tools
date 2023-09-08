using System.Collections;
using System.Collections.Generic;
using Ica.Tests.Shared;
using Ica.Utils;
using Unity.Collections;
using UnityEngine;

namespace Ica.Normal
{
    public class NewBehaviourScript : MonoBehaviour
    {
        public int seg1;
        public int seg2;
        public float rad;
        private void Start()
        {
            var a = new GameObject();
            MeshCreate.CreateUvSphere(a,seg1,seg2,rad);
            
        }
    }
    
}