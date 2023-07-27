using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace IcaNormal
{
    public class RuntimeNormalSolver : MonoBehaviour
    {
        public enum NormalOutputEnum
        {
            WriteToMesh,
            WriteToMaterial
        }

        public NormalOutputEnum NormalOutputTarget = NormalOutputEnum.WriteToMesh;

        [Tooltip("Smoothing angle only usable with bursted method")]
        public bool RecalculateOnStart;

        public bool RecalculateTangents;

        [Tooltip("Data cache asset required when using cached method. You can create this on project tab context menu/plugins /Mesh data cache.")]
        public MeshDataCacheAsset _dataCacheAsset;

        public List<SkinnedMeshRenderer> TargetSkinnedMeshRenderers;
        private MeshDataCache _meshDataCache;
        private List<Mesh> _meshes;

        [Tooltip("Asset of this model in zero pose. Only necessary when using Calculate Blend Shapes option")]
        public List<GameObject> Prefabs;

        private List<GameObject> TempObjects;
        private List<SkinnedMeshRenderer> TempSMRs;
        private List<Mesh> _tempMeshes;
        private List<ComputeBuffer> _normalBuffers;
        private List<ComputeBuffer> _tangentBuffers;
        private bool _isComputeBuffersCreated;
        private JobHandle _normalHandle;
        private bool _isNormalHandleScheduled;
        private bool _isInitialized;
        private List<List<Material>> _materials;


        private void Start()
        {
            Init();
        }

        // private void LateUpdate()
        // {
        //     if (_isNormalHandleScheduled)
        //     {
        //         _normalHandle.Complete();
        //         _isNormalHandleScheduled = false;
        //     }
        // }

        public void Init()
        {
            if (_isInitialized)
            {
                OnDestroy();
            }
            var meshCount = TargetSkinnedMeshRenderers.Count;

            _meshes = new List<Mesh>(meshCount);
            TempObjects = new List<GameObject>(meshCount);
            TempSMRs = new List<SkinnedMeshRenderer>(meshCount);
            _tempMeshes = new List<Mesh>(meshCount);


            foreach (var smr in TargetSkinnedMeshRenderers)
            {
                _meshes.Add(smr.sharedMesh);
                _tempMeshes.Add(new Mesh());
            }

            _meshDataCache = new MeshDataCache();
            _meshDataCache.InitFromMultipleMesh(_meshes);

            if (NormalOutputTarget == NormalOutputEnum.WriteToMesh)
            {
                foreach (var mesh in _meshes)
                    mesh.MarkDynamic();
            }
            else if (NormalOutputTarget == NormalOutputEnum.WriteToMaterial)
            {
                SetupForWriteToMaterial();
            }

            for (int meshIndex = 0; meshIndex < meshCount; meshIndex++)
            {
                var obj = Instantiate(Prefabs[meshIndex], transform);
                obj.SetActive(false);
                TempObjects.Add(obj);
                TempSMRs.Add(obj.GetComponentInChildren<SkinnedMeshRenderer>());
            }

            if (RecalculateOnStart)
                RecalculateNormals();
            _isInitialized = true;
        }

        private void SetupForWriteToMaterial()
        {
            var meshCount = TargetSkinnedMeshRenderers.Count;
            _normalBuffers = new List<ComputeBuffer>(meshCount);
            _tangentBuffers = new List<ComputeBuffer>(meshCount);
            _materials = new List<List<Material>>(meshCount);
            for (int i = 0; i < meshCount; i++)
            {
                var smr = TargetSkinnedMeshRenderers[i];
                var mats = new List<Material>(1);
                smr.GetMaterials(mats);
                _materials.Add(mats);
                //var mats = smr.materials;
                var nBuffer = new ComputeBuffer(_meshes[i].vertexCount, sizeof(float) * 3);
                var tBuffer = new ComputeBuffer(_meshes[i].vertexCount, sizeof(float) * 4);
                _tangentBuffers.Add(tBuffer);
                _normalBuffers.Add(nBuffer);
                for (int matIndex = 0; matIndex < mats.Count; matIndex++)
                {
                    mats[matIndex].SetBuffer("normalsOutBuffer", nBuffer);
                    mats[matIndex].SetBuffer("tangentsOutBuffer", tBuffer);
                    mats[matIndex].SetFloat("_Initialized", 1);
                }
            }

            _meshDataCache.ApplyNormalsToBuffers(_normalBuffers);
            _isComputeBuffersCreated = true;
        }

        private void OnDestroy()
        {
            //Compute buffers need to be destroyed
            if (_isComputeBuffersCreated)
            {
                foreach (var buffer in _normalBuffers)
                    buffer.Dispose();

                foreach (var buffer in _tangentBuffers)
                    buffer.Dispose();
            }

            foreach (var tempMesh in _tempMeshes)
            {
                Destroy(tempMesh);
            }

            foreach (var tempObject in TempObjects)
            {
                Destroy(tempObject);
            }

            if (_materials != null)
            {
                foreach (var matList in _materials)
                {
                    if (matList != null)
                    {
                        foreach (var material in matList)
                        {
                            Destroy(material);
                        }
                        
                    }
                }
            }

            _meshDataCache.Dispose();
        }

        [ContextMenu("RecalculateNormals")]
        public void RecalculateNormals()
        {
            RecalculateCachedParallel();
        }

        private void RecalculateCachedParallel()
        {
            UpdateVertices();
            CachedParallelMethod.CalculateNormalData(_meshDataCache.VertexData, _meshDataCache.IndexData,
                ref _meshDataCache.NormalData, _meshDataCache.AdjacencyList, _meshDataCache.AdjacencyMapper, _meshDataCache.TriNormalData,out _normalHandle);
            //_isNormalHandleScheduled = true;
            _normalHandle.Complete();

            SetNormals();
            if (RecalculateTangents)
            {
                CachedParallelMethod.CalculateTangentData(_meshDataCache.VertexData, _meshDataCache.NormalData, _meshDataCache.IndexData,
                    _meshDataCache.UVData, _meshDataCache.AdjacencyList, _meshDataCache.AdjacencyMapper, ref _meshDataCache.TangentData);
                SetTangents();
            }
        }

        private void SetNormals()
        {
            if (NormalOutputTarget == NormalOutputEnum.WriteToMesh)
                _meshDataCache.ApplyNormalsToMeshes(_meshes);
            else if (NormalOutputTarget == NormalOutputEnum.WriteToMaterial)
                _meshDataCache.ApplyNormalsToBuffers(_normalBuffers);
        }

        private void SetTangents()
        {
            if (NormalOutputTarget == NormalOutputEnum.WriteToMesh)
                _meshDataCache.ApplyTangentsToMeshes(_meshes);
            else if (NormalOutputTarget == NormalOutputEnum.WriteToMaterial)
                _meshDataCache.ApplyTangentsToBuffers(_tangentBuffers);
        }

        public void UpdateVertices()
        {
            Profiler.BeginSample("UpdateVertices");
            Profiler.BeginSample("TransferBlendShapeValues");
            for (int meshIndex = 0; meshIndex < TargetSkinnedMeshRenderers.Count; meshIndex++)
            {
                var smr = TargetSkinnedMeshRenderers[meshIndex];
                for (int bsIndex = 0; bsIndex < smr.sharedMesh.blendShapeCount; bsIndex++)
                {
                    TempSMRs[meshIndex].SetBlendShapeWeight(bsIndex, smr.GetBlendShapeWeight(bsIndex));
                }

                TempSMRs[meshIndex].BakeMesh(_tempMeshes[meshIndex]);
            }

            Profiler.EndSample();

            var tempMDA = Mesh.AcquireReadOnlyMeshData(_tempMeshes);
            _meshDataCache.UpdateOnlyVertexData(tempMDA);
            tempMDA.Dispose();
            Profiler.EndSample();
        }
    }
}