using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace IcaNormal
{
    /// <summary>
    /// A big data container that hold mesh data (or merged data of list of meshes) for needed to normal and tangent calculation
    /// </summary>
    public class MeshDataCache : IDisposable
    {
        public int TotalVertexCount;

        public int TotalIndexCount;

        //these are need for normal and tangent
        //these are need for normal 
        public NativeArray<float3> VertexData;
        public NativeList<int> IndexData;
        public NativeArray<float3> NormalData;
        public NativeList<int> AdjacencyList;
        public NativeArray<int2> AdjacencyMapper;
        private NativeArray<int> _vertexSeparatorData;
        private NativeArray<int> _indexSeparatorData;

        //these are need for tangent
        public NativeArray<float4> TangentData;
        public NativeArray<float3> Tan1Data;
        public NativeArray<float3> Tan2Data;
        public NativeArray<float2> UVData;
        public NativeArray<float3> TriNormalData;

        private Mesh.MeshDataArray _mda;
        private bool _initialized;
        private bool _cachedForTangents;

        public void InitFromMultipleMesh(List<Mesh> meshes, bool cacheForTangents)
        {
            Dispose();
            _mda = Mesh.AcquireReadOnlyMeshData(meshes);

            _vertexSeparatorData = new NativeArray<int>(_mda.Length + 1, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            _indexSeparatorData = new NativeArray<int>(_mda.Length + 1, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            MergedMeshDataUtils.GetTotalVertexCountFomMDA(_mda, out TotalVertexCount);

            VertexData = new NativeArray<float3>(TotalVertexCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            MergedMeshDataUtils.GetMergedVertices(_mda, ref VertexData, ref _vertexSeparatorData);

            NormalData = new NativeArray<float3>(TotalVertexCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            MergedMeshDataUtils.GetMergedNormals(_mda, ref NormalData, ref _vertexSeparatorData);

            MergedMeshDataUtils.GetAllIndicesCountOfMultipleMeshes(_mda, out TotalIndexCount);
            IndexData = new NativeList<int>(TotalIndexCount, Allocator.Persistent);
            MergedMeshDataUtils.GetMergedIndices(_mda, ref IndexData, ref _indexSeparatorData);

            TriNormalData = new NativeArray<float3>(TotalIndexCount / 3, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            VertexPositionMapper.GetVertexPosHashMap(VertexData, out var tempPosGraph, Allocator.Temp);
            IcaNormal.AdjacencyMapper.CalculateAdjacencyData(VertexData, IndexData, tempPosGraph, out AdjacencyList, out AdjacencyMapper, Allocator.Persistent);

            if (cacheForTangents)
            {
                TangentData = new NativeArray<float4>(TotalVertexCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                MergedMeshDataUtils.GetMergedTangents(_mda, ref TangentData, ref _vertexSeparatorData);
                UVData = new NativeArray<float2>(TotalVertexCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                MergedMeshDataUtils.GetMergedUVs(_mda, ref UVData, ref _vertexSeparatorData);
                Tan1Data = new NativeArray<float3>(TotalIndexCount / 3, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                Tan2Data = new NativeArray<float3>(TotalIndexCount / 3, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                _cachedForTangents = true;
            }

            _initialized = true;
        }

        public void UpdateOnlyVertexData(in Mesh.MeshDataArray mda)
        {
            Profiler.BeginSample("UpdateOnlyVertexData");
            MergedMeshDataUtils.GetMergedVertices(mda, ref VertexData, ref _vertexSeparatorData);
            Profiler.EndSample();
        }

        public UnsafeList<NativeList<float3>> GetTempSplittedNormalData()
        {
            var n = new UnsafeList<NativeList<float3>>(_mda.Length, Allocator.Temp);
            for (int meshIndex = 0; meshIndex < _mda.Length; meshIndex++)
            {
                var meshNormal = new NativeList<float3>(_vertexSeparatorData[meshIndex + 1] - _vertexSeparatorData[meshIndex], Allocator.Temp);
                meshNormal.CopyFrom(NormalData.GetSubArray(_vertexSeparatorData[meshIndex], _vertexSeparatorData[meshIndex + 1] - _vertexSeparatorData[meshIndex]));
                n.Add(meshNormal);
            }

            return n;
        }

        public UnsafeList<NativeList<float4>> GetTempSplittedTangentData()
        {
            var t = new UnsafeList<NativeList<float4>>(_mda.Length, Allocator.Temp);
            for (int meshIndex = 0; meshIndex < _mda.Length; meshIndex++)
            {
                var mt = new NativeList<float4>(Allocator.Temp);
                mt.CopyFrom(TangentData.GetSubArray(_vertexSeparatorData[meshIndex], _vertexSeparatorData[meshIndex + 1] - _vertexSeparatorData[meshIndex]));
                t.Add(mt);
            }

            return t;
        }

        public void ApplyNormalsToBuffers(List<ComputeBuffer> buffers)
        {
            Profiler.BeginSample("ApplyNormalsToBuffers");
            for (int meshIndex = 0; meshIndex < buffers.Count; meshIndex++)
            {
                buffers[meshIndex].SetData(
                    NormalData.GetSubArray(_vertexSeparatorData[meshIndex], _vertexSeparatorData[meshIndex + 1] - _vertexSeparatorData[meshIndex])
                );
            }

            Profiler.EndSample();
        }

        public void ApplyTangentsToBuffers(List<ComputeBuffer> buffers)
        {
            for (int meshIndex = 0; meshIndex < buffers.Count; meshIndex++)
            {
                buffers[meshIndex].SetData(
                    TangentData.GetSubArray(_vertexSeparatorData[meshIndex], _vertexSeparatorData[meshIndex + 1] - _vertexSeparatorData[meshIndex])
                );
            }
        }

        public void ApplyNormalsToMeshes(List<Mesh> meshes)
        {
            for (int meshIndex = 0; meshIndex < meshes.Count; meshIndex++)
            {
                meshes[meshIndex].SetNormals(
                    NormalData.GetSubArray(_vertexSeparatorData[meshIndex], _vertexSeparatorData[meshIndex + 1] - _vertexSeparatorData[meshIndex])
                );
            }
        }

        public void ApplyTangentsToMeshes(List<Mesh> meshes)
        {
            for (int meshIndex = 0; meshIndex < meshes.Count; meshIndex++)
            {
                meshes[meshIndex].SetTangents(
                    TangentData.GetSubArray(_vertexSeparatorData[meshIndex], _vertexSeparatorData[meshIndex + 1] - _vertexSeparatorData[meshIndex])
                );
            }
        }

        public void Dispose()
        {
            if (_initialized == false)
                return;
            _initialized = false;
            _mda.Dispose();
            VertexData.Dispose();
            IndexData.Dispose();
            NormalData.Dispose();
            AdjacencyList.Dispose();
            AdjacencyMapper.Dispose();
            _vertexSeparatorData.Dispose();
            _indexSeparatorData.Dispose();
            TriNormalData.Dispose();
            if (_cachedForTangents)
            {
                _cachedForTangents = false;
                TangentData.Dispose();
                UVData.Dispose();
                Tan1Data.Dispose();
                Tan2Data.Dispose();
            }
        }

// ?? is this working?
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