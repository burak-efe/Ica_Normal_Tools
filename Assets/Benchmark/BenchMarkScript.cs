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
    public KeyCode CachedLite = KeyCode.F4;
    public KeyCode CachedParallel = KeyCode.F5;
    public KeyCode TangentOnly = KeyCode.F6;
    public KeyCode BuiltInTangentOnly = KeyCode.F7;

    public Mesh BuiltInTargetMesh;
    public Mesh TB_TargetMesh;
    public RuntimeNormalSolver SDBurstedRuntimeNormalSolver;
    public RuntimeNormalSolver CachedLiteRuntimeNormalSolver;
    public RuntimeNormalSolver CachedParallelRuntimeNormalSolver;


    void Update()
    {
        if (Input.GetKey(UnityBuiltIn))
        {
            BuiltInTargetMesh.RecalculateNormals();
        }
        else if (Input.GetKey(TB_NormalSolver))
        {
            TB.TBNormalSolver.RecalculateNormals(TB_TargetMesh, 120f);
            TB.TBNormalSolver.RecalculateTangents(TB_TargetMesh);
        }
        else if (Input.GetKey(SDBursted))
        {
            SDBurstedRuntimeNormalSolver.RecalculateNormals();
        }
        else if (Input.GetKey(CachedLite))
        {
            CachedLiteRuntimeNormalSolver.RecalculateNormals();
        }
        else if (Input.GetKey(CachedParallel))
        {
            CachedParallelRuntimeNormalSolver.RecalculateNormals();
        }
        else if (Input.GetKey(TangentOnly))
        {
            CachedParallelRuntimeNormalSolver.TangentsOnlyTest();
        }
        else if (Input.GetKey(BuiltInTangentOnly))
        {
            BuiltInTargetMesh.RecalculateTangents();
        }
    }
}