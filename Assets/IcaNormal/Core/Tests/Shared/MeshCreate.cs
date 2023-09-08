using UnityEngine;
using UnityEngine.Rendering;

namespace Ica.Tests.Shared
{
    public static class MeshCreate
    {
        
        public static void CreateUvSphere(GameObject gameObject, int numLongitudeSegments = 20, int numLatitudeSegments = 40, float radius = 1f)
        {
            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();

            Mesh mesh = new Mesh();
            mesh.indexFormat = IndexFormat.UInt32;

            // Generate vertices, normals, and UVs
            Vector3[] vertices = new Vector3[(numLongitudeSegments + 1) * (numLatitudeSegments + 1)];
            Vector3[] normals = new Vector3[vertices.Length];
            Vector2[] uv = new Vector2[vertices.Length];

            for (int lat = 0; lat <= numLatitudeSegments; lat++)
            {
                float normalizedLatitude = lat / (float)numLatitudeSegments;
                float theta = normalizedLatitude * Mathf.PI;

                for (int lon = 0; lon <= numLongitudeSegments; lon++)
                {
                    float normalizedLongitude = lon / (float)numLongitudeSegments;
                    float phi = normalizedLongitude * 2.0f * Mathf.PI;

                    float x = Mathf.Sin(theta) * Mathf.Cos(phi);
                    float y = Mathf.Cos(theta);
                    float z = Mathf.Sin(theta) * Mathf.Sin(phi);

                    int index = lat * (numLongitudeSegments + 1) + lon;

                    vertices[index] = new Vector3(x, y, z) * radius;
                    normals[index] = vertices[index].normalized;
                    uv[index] = new Vector2(normalizedLongitude, 1.0f - normalizedLatitude);
                }
            }

            // Generate triangles
            int[] triangles = new int[numLongitudeSegments * numLatitudeSegments * 6];

            int triangleIndex = 0;
            for (int lat = 0; lat < numLatitudeSegments; lat++)
            {
                for (int lon = 0; lon < numLongitudeSegments; lon++)
                {
                    int vertexIndex = lat * (numLongitudeSegments + 1) + lon;

                    triangles[triangleIndex++] = vertexIndex;
                    triangles[triangleIndex++] = vertexIndex + 1;
                    triangles[triangleIndex++] = vertexIndex + numLongitudeSegments + 1;

                    triangles[triangleIndex++] = vertexIndex + 1;
                    triangles[triangleIndex++] = vertexIndex + numLongitudeSegments + 2;
                    triangles[triangleIndex++] = vertexIndex + numLongitudeSegments + 1;
                }
            }

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uv;
            mesh.triangles = triangles;

            meshFilter.mesh = mesh;
            // Assign a material for rendering
            meshRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));


            
        }
    }
}