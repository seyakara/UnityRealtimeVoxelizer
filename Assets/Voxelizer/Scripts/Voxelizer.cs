using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.Rendering;

namespace UnityRealtimeVoxelizer
{

    public struct VoxelData
    {
        public Vector3 position;
        public Color color;
    }

    [ExecuteAlways]
    public class Voxelizer : MonoBehaviour
    {
        public LayerMask m_targetLayer;
        public Mesh m_mesh;
        public Vector3 m_centerPos;
        public float m_areaSize;
        public int m_gridWidth = 128;
        public Transform m_targetPos;
        public float m_heightScale = 1.0f;
        public float m_FPS;

        public ComputeShader m_computShader;
        public Material m_material;

        int m_kernel_reset;
        int m_kernel_makelist;

        GraphicsBuffer m_voxelBuffer;
        GraphicsBuffer m_colorBuffer;
        GraphicsBuffer m_argsBuffer;
        Camera m_voxelCamera;
        int m_prevgridWidth = 0;

        float m_lastRenderedTime;

        RenderTexture m_renderTexture;

        void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (m_voxelCamera.enabled && camera == m_voxelCamera)
            {
                Graphics.SetRandomWriteTarget(1, m_colorBuffer, false);
            }
        }

        void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (m_voxelCamera.enabled && camera == m_voxelCamera)
            {
                Graphics.ClearRandomWriteTargets();

                m_voxelBuffer.SetCounterValue(0);

                uint x, y, z;
                m_computShader.GetKernelThreadGroupSizes(m_kernel_makelist, out x, out y, out z);
                m_computShader.Dispatch(m_kernel_makelist, m_gridWidth / (int)x, m_gridWidth / (int)y, m_gridWidth / (int)z);
                GraphicsBuffer.CopyCount(m_voxelBuffer, m_argsBuffer, 4);
            }
        }

        void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;

            m_lastRenderedTime = 0f;

            Setup();

            UpdateVoxelCamera();
        }

        private void LateUpdate()
        {
            float current = Time.time;
            float frameInterval = m_FPS > 0f ? 1.0f / m_FPS : 0f;
            if (current - m_lastRenderedTime >= frameInterval)
            {
                if (m_targetPos != null)
                {
                    m_centerPos = m_targetPos.position;
                    UpdateVoxelCamera();
                }

                m_lastRenderedTime = current;
                m_voxelCamera.enabled = true;
            }
            else
            {
                m_voxelCamera.enabled = false;
            }

            Graphics.DrawMeshInstancedIndirect(m_mesh, 0, m_material, new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)), m_argsBuffer);
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;

            ReleaseBuffers();
        }

        private void UpdateVoxelCamera()
        {
            m_voxelCamera.orthographicSize = m_areaSize;
            float blocksize = m_voxelCamera.orthographicSize / (float)m_gridWidth * 2.0f;
            Vector3 VoxelUnitSize = new Vector3(blocksize, blocksize * m_heightScale, blocksize);

            Vector3 campos = new Vector3(VoxelUnitSize.x * (int)(m_centerPos.x / VoxelUnitSize.x),
                                            VoxelUnitSize.y * (int)(m_centerPos.y / VoxelUnitSize.y),
                                            VoxelUnitSize.z * (int)(m_centerPos.z / VoxelUnitSize.z));

            m_voxelCamera.transform.position = campos - new Vector3(0, 0, m_areaSize + m_voxelCamera.nearClipPlane);
            m_voxelCamera.farClipPlane = m_voxelCamera.nearClipPlane + m_areaSize * 2.0f;


            Vector3 VoxelBasePos = campos - VoxelUnitSize * (m_gridWidth / 2);

            Shader.SetGlobalFloat("_BlockSize", blocksize);
            Shader.SetGlobalFloat("_HeightScale", m_heightScale);
            Shader.SetGlobalInt("_GridWidth", m_gridWidth);
            Shader.SetGlobalVector("_BasePos", VoxelBasePos);

            m_material.SetFloat("_BlockSize", blocksize);

            m_computShader.SetVector("_BasePos", VoxelBasePos);
            m_computShader.SetInt("_GridWidth", m_gridWidth);
            m_computShader.SetFloat("_BlockSize", blocksize);
            m_computShader.SetFloat("_HeightScale", m_heightScale);
        }

        private void Setup()
        {
            ReleaseBuffers();

            if (m_renderTexture != null)
            {
                m_renderTexture.Release();
            }
            m_renderTexture = new RenderTexture(m_gridWidth, m_gridWidth, 0, RenderTextureFormat.ARGB32);
            m_renderTexture.filterMode = FilterMode.Point;
            m_renderTexture.antiAliasing = 8;
            m_renderTexture.useMipMap = false;
            m_renderTexture.Create(); 

            m_voxelCamera = GetComponentInChildren<Camera>();
            m_voxelCamera.cullingMask = m_targetLayer;
            m_voxelCamera.targetTexture = m_renderTexture;

            m_kernel_reset = m_computShader.FindKernel("ResetList");
            m_kernel_makelist = m_computShader.FindKernel("MakeList");

            m_argsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 5, Marshal.SizeOf(typeof(uint)));
            uint[] args = new uint[] { 0, 0, 0, 0, 0 };
            args[0] = m_mesh.GetIndexCount(0);
            args[1] = 0;
            args[2] = m_mesh.GetIndexStart(0);
            args[3] = m_mesh.GetBaseVertex(0);
            m_argsBuffer.SetData(args);

            int buffsize = m_gridWidth * m_gridWidth * m_gridWidth;
            m_voxelBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Counter, buffsize, Marshal.SizeOf(typeof(VoxelData)));
            m_voxelBuffer.SetCounterValue(0);
            m_material.SetBuffer("_VoxelBuffer", m_voxelBuffer);

            m_colorBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, buffsize, Marshal.SizeOf(typeof(uint)));
            m_computShader.SetBuffer(m_kernel_reset, "_ColorBuffer", m_colorBuffer);

            m_computShader.SetBuffer(m_kernel_makelist, "_ColorBuffer", m_colorBuffer);
            m_computShader.SetBuffer(m_kernel_makelist, "_ResultBuffer", m_voxelBuffer);

            uint x, y, z;
            m_computShader.GetKernelThreadGroupSizes(m_kernel_reset, out x, out y, out z);
            m_computShader.Dispatch(m_kernel_reset, m_gridWidth / (int)x, m_gridWidth / (int)y, m_gridWidth / (int)z);
        }

        private void ReleaseBuffers()
        {
            if (m_voxelBuffer != null)
            {
                m_voxelBuffer.Release();
            }

            if (m_colorBuffer != null)
            {
                m_colorBuffer.Release();
            }

            if (m_argsBuffer != null)
            {
                m_argsBuffer.Release();
            }
        }

        private void OnValidate()
        {
            if (m_prevgridWidth != m_gridWidth)
            {
                m_gridWidth = Mathf.Max(8, m_gridWidth);

                Setup();
                m_prevgridWidth = m_gridWidth;
            }

            if(m_voxelCamera != null)
            {
                m_voxelCamera.cullingMask = m_targetLayer;
                UpdateVoxelCamera();
            }
        }
    }
}
