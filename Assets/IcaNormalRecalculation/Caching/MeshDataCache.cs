using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace IcaNormal
{
    public class MeshDataCache : IDisposable
    {
        public NativeList<int> _nativeAdjacencyList;
        public NativeList<int2> _nativeAdjacencyMap;
        public NativeList<int> _indices;
        public NativeList<float3> _vertices;
        public NativeList<float3> _normals;
        public NativeList<float4> _tangents;
        public NativeList<float2> _uvs;
        public Mesh.MeshData _mainMeshData;

        private Mesh.MeshDataArray _mda;

        public MeshDataCache(List<Mesh> m, bool shouldCacheForTangents = true)
        {
            _mda = Mesh.AcquireReadOnlyMeshData(m);
            _mainMeshData = _mda[0];
            CalculateCache();
            
        }
        
        public void CalculateCache()
        {
            
            

            NativeContainerUtils.CreateMergedVertices(_mda, out  _vertices, out var vMap, Allocator.TempJob);
            NativeContainerUtils.CreateMergedIndices(_mda, out  _indices, out var iMap, Allocator.TempJob);
             _normals = new NativeList<float3>(_vertices.Length, Allocator.TempJob);
            VertexPositionMapper.GetVertexPosHashMap( _vertices.AsArray(), out var tempPosGraph, Allocator.Temp);
            AdjacencyMapper.CalculateAdjacencyData( _vertices,  _indices,  tempPosGraph, out _nativeAdjacencyList, out _nativeAdjacencyMap, Allocator.Persistent);
            
            
            //CachedParallelMethod.CalculateNormalDataUncached(mergedVertices.AsArray(), mergedIndices.AsArray(), ref mergedNormals);

            // Apply
            // for (int i = 0; i < _mda.Length; i++)
            // {
            //     var sub = mergedNormals.GetSubArray(vMap[i], vMap[i + 1] - vMap[i]);
            //
            //     Meshes[i].SetNormals(sub);
            // }
            //
            
            //_tempMesh = new Mesh();
            //_meshDataArray = Mesh.AcquireReadOnlyMeshData(_mesh);
            // _mainMeshData = _meshDataArray[0];
            // _vertices = new NativeArray<float3>(_mesh.vertexCount, Allocator.Persistent);
            // _normals = new NativeArray<float3>(_mesh.vertexCount, Allocator.Persistent);
            // _tangents = new NativeArray<float4>(_mesh.vertexCount, Allocator.Persistent);
            // _uvs = new NativeArray<float2>(_mesh.vertexCount, Allocator.Persistent);
            // _mainMeshData.GetNormals(_normals.Reinterpret<Vector3>());
            // _mainMeshData.GetTangents(_tangents.Reinterpret<Vector4>());
            // _mainMeshData.GetUVs(0, _uvs.Reinterpret<Vector2>());
            //
            //_mainMeshData.GetAllIndices(out _indices, Allocator.Persistent);
            //var tempVertices = new NativeArray<float3>(_mainMeshData.vertexCount, Allocator.Temp);
            //_mainMeshData.GetVertices(tempVertices.Reinterpret<Vector3>());

        }


        public void Dispose()
        {
            _mda.Dispose();
            _nativeAdjacencyList.Dispose();
            _nativeAdjacencyMap.Dispose();
            _indices.Dispose();
            _vertices.Dispose();
            _normals.Dispose();
            _tangents.Dispose();
            _uvs.Dispose();
        }


        public static Mesh MakeReadableMeshCopy(Mesh nonReadableMesh)
        {
            Mesh meshCopy = new Mesh();
            meshCopy.indexFormat = nonReadableMesh.indexFormat;

            // Handle vertices
            GraphicsBuffer verticesBuffer = nonReadableMesh.GetVertexBuffer(0);
            int totalSize = verticesBuffer.stride * verticesBuffer.count;
            byte[] data = new byte[totalSize];
            verticesBuffer.GetData(data);
            meshCopy.SetVertexBufferParams(nonReadableMesh.vertexCount, nonReadableMesh.GetVertexAttributes());
            meshCopy.SetVertexBufferData(data, 0, 0, totalSize);
            verticesBuffer.Release();

            // Handle triangles
            meshCopy.subMeshCount = nonReadableMesh.subMeshCount;
            GraphicsBuffer indexesBuffer = nonReadableMesh.GetIndexBuffer();
            int tot = indexesBuffer.stride * indexesBuffer.count;
            byte[] indexesData = new byte[tot];
            indexesBuffer.GetData(indexesData);
            meshCopy.SetIndexBufferParams(indexesBuffer.count, nonReadableMesh.indexFormat);
            meshCopy.SetIndexBufferData(indexesData, 0, 0, tot);
            indexesBuffer.Release();

            // Restore submesh structure
            uint currentIndexOffset = 0;
            for (int i = 0; i < meshCopy.subMeshCount; i++)
            {
                uint subMeshIndexCount = nonReadableMesh.GetIndexCount(i);
                meshCopy.SetSubMesh(i, new SubMeshDescriptor((int)currentIndexOffset, (int)subMeshIndexCount));
                currentIndexOffset += subMeshIndexCount;
            }

            // Recalculate normals and bounds
            meshCopy.RecalculateNormals();
            meshCopy.RecalculateBounds();

            return meshCopy;
        }
    }
}