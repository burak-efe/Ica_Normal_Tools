using UnityEngine;
using UnityEngine.Rendering;

namespace Ica.Tests.Shared
{
    public static class MeshCreate
    {
        public static GameObject CreateUvSphere(int numLongitudeSegments = 20, int numLatitudeSegments = 40, float radius = 1f)
        {
            var obj = new GameObject("UV Sphere");
            MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();

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

            meshFilter.sharedMesh = mesh;
            // Assign a material for rendering
            meshRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            return obj;
        }

        public static GameObject CreateCube(Vector3 position1, Vector3 vector3)
        {
            var obj = new GameObject("Cube");
            var mf = obj.AddComponent<MeshFilter>();
            obj.AddComponent<MeshRenderer>();
            Mesh mesh = new Mesh();

            Vector3[] vertices = new Vector3[24];
            //int[] triangles = new int[36];
            Vector3[] normals = new Vector3[24];

            // Define the vertices of the cube
            Vector3[] cubeVertices = new Vector3[]
            {
                // Front face
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f),

                // Back face
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f),

                // Left face
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),

                // Right face
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),

                // Top face
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f),

                // Bottom face
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f)
            };

            // Define the triangles for the cube's faces
            int[] cubeTriangles = new int[]
            {
                // Front face
                0, 2, 1,
                0, 3, 2,

                // Back face
                4, 5, 6,
                4, 6, 7,

                // Left face
                8, 10, 9,
                8, 11, 10,

                // Right face
                12, 13, 14,
                12, 14, 15,

                // Top face
                16, 18, 17,
                16, 19, 18,

                // Bottom face
                20, 21, 22,
                20, 22, 23
            };

            for (int i = 0; i < 24; i++)
            {
                vertices[i] = Vector3.Scale(cubeVertices[i], vector3) + position1;
            }

            for (int i = 0; i < 36; i += 3)
            {
                Vector3 v1 = vertices[cubeTriangles[i]];
                Vector3 v2 = vertices[cubeTriangles[i + 1]];
                Vector3 v3 = vertices[cubeTriangles[i + 2]];

                Vector3 normal = Vector3.Cross(v2 - v1, v3 - v1).normalized;

                normals[cubeTriangles[i]] = normal;
                normals[cubeTriangles[i + 1]] = normal;
                normals[cubeTriangles[i + 2]] = normal;
            }

            mesh.vertices = vertices;
            mesh.triangles = cubeTriangles;
            mesh.normals = normals;
            mf.sharedMesh = mesh;
            return obj;
        }
    }
}