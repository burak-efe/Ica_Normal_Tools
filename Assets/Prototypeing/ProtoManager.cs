using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IcaNormal;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class ProtoManager : MonoBehaviour
{
    public KeyCode KeyCode1;
    public KeyCode Init;
    public RuntimeNormalSolver Solver1;

    private void Update()
    {
        if (Input.GetKey(KeyCode1))
        {
            Solver1.RecalculateNormals();
        }
        if (Input.GetKey(Init))
        {
            Solver1.Init();
        }
    }
}