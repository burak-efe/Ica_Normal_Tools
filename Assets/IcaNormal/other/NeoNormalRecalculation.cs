using UnityEngine;


    public class NeoNormalRecalculation : MonoBehaviour
    {
        public void RecalculateNormalsCustom(Mesh mesh)
        {
            var vertices = mesh.vertices;
            var triangles = mesh.triangles;
            var normals = new Vector3[vertices.Length];

            for (int i = 0; i < triangles.Length; i += 3)
            {
                int i1 = triangles[i];
                int i2 = triangles[i + 1];
                int i3 = triangles[i + 2];

                // Calculate the normal of the triangle
                Vector3 p1 = vertices[i2] - vertices[i1];
                Vector3 p2 = vertices[i3] - vertices[i1];
                Vector3 normal = Vector3.Cross(p1, p2);

                float magnitude = normal.magnitude;
                if (magnitude > 0)
                {
                    normal /= magnitude;
                }

                normals[i1] += normal;
                normals[i2] += normal;
                normals[i3] += normal;
            }

            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = normals[i].normalized;
            }

            mesh.normals = normals;
        }

    }
