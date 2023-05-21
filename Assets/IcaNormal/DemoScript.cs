using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.BuiltIn.ShaderGraph;
using UnityEngine;
using IcaNormal;
using UnityEngine.Serialization;

public class DemoScript : MonoBehaviour
{
    public KeyCode UnityBuiltIn = KeyCode.F1;
    public KeyCode CachedMethod = KeyCode.F2;
    public KeyCode FullMethod = KeyCode.F3;

    public Mesh TargetMesh;
    public IcaRuntimeNormalSolver icaRuntimeNormalSolver;
    void Start()
    {
        
    }


    void Update()
    {
        if (Input.GetKeyDown(UnityBuiltIn))
        {
            TargetMesh.RecalculateNormals();
            TargetMesh.RecalculateTangents();
        }
        if (Input.GetKeyDown(CachedMethod))
        {
            icaRuntimeNormalSolver.RecalculateNormals();
        }
        if (Input.GetKeyDown(FullMethod))
        {
            TargetMesh.RecalculateNormalsSmooth(120f);
            
        }
    }
}
