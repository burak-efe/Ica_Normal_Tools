using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.BuiltIn.ShaderGraph;
using UnityEngine;
using IcaNormal;
using UnityEngine.Serialization;

[DefaultExecutionOrder(-9999)]
public class BenchMarkScript : MonoBehaviour
{
    public KeyCode UnityBuiltIn = KeyCode.F1;
    public KeyCode TB_NormalSolver = KeyCode.F2;
    public KeyCode SDBursted = KeyCode.F3;
    public KeyCode Naive = KeyCode.F4;
    public KeyCode CachedParallel = KeyCode.F5;
    
    public KeyCode TangentOnly = KeyCode.F6;
    public KeyCode BuiltInTangentOnly = KeyCode.F7;

    public Mesh TargetMesh;



    void Update()
    {
        if (Input.GetKey(UnityBuiltIn))
        {
            TargetMesh.RecalculateNormals();
        }
        else if (Input.GetKey(TB_NormalSolver))
        {
            TB.TBNormalSolver.RecalculateNormals(TargetMesh, 120f);
            TB.TBNormalSolver.RecalculateTangents(TargetMesh);
        }
        else if (Input.GetKey(SDBursted))
        {
            TargetMesh.RecalculateNormals();
        }
        else if (Input.GetKey(Naive))
        {
        }
        else if (Input.GetKey(CachedParallel))
        {
           // TargetMesh.reca
        }
        else if (Input.GetKey(TangentOnly))
        {
            //CachedParallelRuntimeNormalSolver.TangentsOnlyTest();
        }
        else if (Input.GetKey(BuiltInTangentOnly))
        {
            TargetMesh.RecalculateTangents();
        }
    }
}