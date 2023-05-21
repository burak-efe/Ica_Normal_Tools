using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.BuiltIn.ShaderGraph;
using UnityEngine;
using IcaNormal;
using UnityEngine.Serialization;

public class BenchMarkScript : MonoBehaviour
{
    public KeyCode UnityBuiltIn = KeyCode.F1;
    
    public KeyCode TB_NormalSolver = KeyCode.F2;
    
    public KeyCode Ica_Cached_WriteToMesh_Method = KeyCode.F3;
    public KeyCode Ica_Cached_WriteToMaterial_Method = KeyCode.F4;
    
    public KeyCode Ica_Bursted_WriteToMesh_Method = KeyCode.F5;
    public KeyCode Ica_Bursted_WriteToMaterial_Method = KeyCode.F6;

    public Mesh BuiltInTargetMesh;
    public Mesh TB_TargetMesh;
    public IcaRuntimeNormalSolver cachedToMesh;
    public IcaRuntimeNormalSolver cachedToMaterial;
    public IcaRuntimeNormalSolver burstedToMesh;
    public IcaRuntimeNormalSolver burstedToMaterial;



    void Update()
    {
        if (Input.GetKey(UnityBuiltIn))
        {
            BuiltInTargetMesh.RecalculateNormals();
            BuiltInTargetMesh.RecalculateTangents();
        }
        
        if (Input.GetKey(TB_NormalSolver))
        {
            TB.TBNormalSolver.RecalculateNormals(TB_TargetMesh,120f);
            TB.TBNormalSolver.RecalculateTangents(TB_TargetMesh);
        }
        
        if (Input.GetKey(Ica_Cached_WriteToMesh_Method))
        {
            cachedToMesh.RecalculateNormals();
        }
        
        if (Input.GetKey(Ica_Cached_WriteToMaterial_Method))
        {
            cachedToMaterial.RecalculateNormals();
            
        }
        
        if (Input.GetKey(Ica_Bursted_WriteToMesh_Method))
        {
            burstedToMesh.RecalculateNormals();
        }
        
        if (Input.GetKey(Ica_Bursted_WriteToMaterial_Method))
        {
            burstedToMaterial.RecalculateNormals();
        }

    }
}
