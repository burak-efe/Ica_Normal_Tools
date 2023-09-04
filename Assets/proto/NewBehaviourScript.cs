using System.Collections;
using System.Collections.Generic;
using Ica.Utils;
using Unity.Collections;
using UnityEngine;

namespace Ica.Normal
{
    public class NewBehaviourScript : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            var toInsert = 5;
            var listModified = new NativeList<int>(1, Allocator.Temp) { 1, 2, 3, 4 };

            listModified.InsertAtBeginning(toInsert);


            for (int i = 0; i < listModified.Length; i++)
            {
                print($"element at {i} is {listModified[i]}");
            }
        }
    }
}