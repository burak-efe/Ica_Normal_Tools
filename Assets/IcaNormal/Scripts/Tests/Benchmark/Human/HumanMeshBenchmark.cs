using System.Collections;
using System.Collections.Generic;
using Ica.Normal;

using UnityEngine;
using UnityEngine.Serialization;

public class HumanMeshBenchmark : MonoBehaviour
{
    public Mesh TargetMesh;
    [FormerlySerializedAs("IcaBlendShapeSolver")] [FormerlySerializedAs("IcaSolver")] public IcaNormalMorphedMeshSolver IcaMorphedMeshSolver;

}