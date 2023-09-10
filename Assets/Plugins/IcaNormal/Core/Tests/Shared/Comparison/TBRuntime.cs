using UnityEngine;

namespace TB
{
    public class TBRuntime : MonoBehaviour
    {
        public Mesh TargetMesh;
        [Range(0,180)]
        public float Angle;

        private void Start()
        {
            TargetMesh.RecalculateNormals(Angle);
        }
    }
}