using System;
using NodeGraph;
using UnityEngine;
using UnityEngine.Rendering;

namespace TerrainGraph
{
    [NodeMenuItem("Object/Scatter")]
    public partial class ScatterNode : TerrainNode
    {
        [SerializeField] private float minScale = 1f;
        [SerializeField] private float maxScale = 1f;
        [SerializeField, Min(1e-6f)] private float spacing = 10f;
        [SerializeField] private int seed = 0;
        [SerializeField] private bool rotateToTerrain;
        [SerializeField] private GameObject prefab;
        [SerializeField] private GameObject[] prefabs = Array.Empty<GameObject>();

        [Input] private RenderTargetIdentifier input;

        private ComputeBuffer inputTypeIdsBuffer;
        private ITerrainGraphGraphicsBufferHandle result, instanceTypeIds;

        public override void Initialize()
        {
            inputTypeIdsBuffer = new ComputeBuffer(1, sizeof(uint));
        }

        public override void Cleanup()
        {
            inputTypeIdsBuffer.Release();
        }

        public override void PreProcess(TerrainGraph graph, CommandBuffer command)
        {
            var threadCount = Vector3Int.CeilToInt(graph.ActiveTerrain.terrainData.size / spacing);

            var bufferSize = threadCount.x * threadCount.z;
            if (bufferSize < 1)
            {
                return;
            }

            result = graph.GetGraphicsBuffer(bufferSize, sizeof(float) * 12, GraphicsBuffer.Target.Counter);
            instanceTypeIds = graph.GetGraphicsBuffer(bufferSize, sizeof(uint), GraphicsBuffer.Target.Structured);
        }

        public override void Process(TerrainGraph graph, CommandBuffer command)
        {
            var terrainRenderer = graph.ActiveTerrain.GetComponent<ITerrainRenderer>();
            if (terrainRenderer == null)
                return;

            var offset = graph.ActiveTerrain.GetPosition();
            var threadCount = Vector3Int.CeilToInt(graph.ActiveTerrain.terrainData.size / spacing);

            var bufferSize = threadCount.x * threadCount.z;
            if (bufferSize < 1)
            {
                return;
            }

            var instanceRenderer = graph.ActiveTerrain.GetComponent<IInstanceRendererGPU>();
            if (instanceRenderer == null || prefabs.Length == 0)
                return;

            var instanceIds = ScopedPooledList<int>.Get();
            foreach (var prefab in prefabs)
            {
                var instanceTypeId = instanceRenderer.AddInstanceType(prefab);
                instanceIds.Value.Add(instanceTypeId);
            }

            var size = graph.ActiveTerrain.terrainData.size;

            // Set up some parameters for distributing the points, and their scale
            var computeShader = Resources.Load<ComputeShader>("Objects/ScatterNode");
            command.SetComputeFloatParam(computeShader, "Spacing", spacing);
            command.SetComputeVectorParam(computeShader, "Count", new Vector2(threadCount.x, threadCount.z));
            command.SetComputeIntParam(computeShader, "XCount", threadCount.x);
            command.SetComputeIntParam(computeShader, "YCount", threadCount.z);

            command.SetComputeIntParam(computeShader, "Seed", seed);
            command.SetComputeIntParam(computeShader, "_RotateToTerrain", rotateToTerrain ? 1 : 0);
            command.SetComputeVectorParam(computeShader, "Scale", new Vector4(minScale, maxScale, 0, 0));
            command.SetComputeVectorParam(computeShader, "ScaleOffset", new Vector4(size.x, size.z, offset.x, offset.z));

            command.SetComputeTextureParam(computeShader, 0, "_Heightmap", terrainRenderer.Heightmap);
            command.SetComputeTextureParam(computeShader, 1, "_Heightmap", terrainRenderer.Heightmap);

            command.SetComputeFloatParam(computeShader, "_TerrainHeightScale", graph.ActiveTerrain.terrainData.size.y);
            command.SetComputeFloatParam(computeShader, "_TerrainHeightOffset", graph.ActiveTerrain.GetPosition().y);

            command.SetBufferCounterValue(result.GetGraphicsBuffer(), 0);

            command.SetComputeIntParam(computeShader, "_Type", instanceIds.Value[0]);

            command.SetComputeFloatParam(computeShader, "_InstanceTypeCount", instanceIds.Value.Count);
            command.ExpandAndSetComputeBufferData(ref inputTypeIdsBuffer, instanceIds.Value);

            // If a probability input is assigned, there is more work to do
            if (NodeIsConnected("input"))
            {
                // We need a "temporary" compute buffer to store the results, but they don't exist, so we must create and then destroy one
                command.SetComputeFloatParam(computeShader, "_MaskMin", GetConnectionMin("input"));
                command.SetComputeFloatParam(computeShader, "_MaskMax", GetConnectionMax("input"));
                command.SetComputeTextureParam(computeShader, 1, "_Mask", input);
                command.SetComputeTextureParam(computeShader, 1, "_TerrainNormalMap", terrainRenderer.NormalMap);
                command.SetComputeBufferParam(computeShader, 1, "_Result", result.GetGraphicsBuffer());
                command.SetComputeBufferParam(computeShader, 1, "_InstanceTypeIds", instanceTypeIds.GetGraphicsBuffer());
                command.SetComputeBufferParam(computeShader, 1, "_InputTypeIds", inputTypeIdsBuffer);
                command.DispatchNormalized(computeShader, 1, threadCount.x, threadCount.z, 1);
            }
            else
            {
                command.SetComputeBufferParam(computeShader, 0, "_Result", result.GetGraphicsBuffer());
                command.SetComputeBufferParam(computeShader, 0, "_InstanceTypeIds", instanceTypeIds.GetGraphicsBuffer());
                command.SetComputeTextureParam(computeShader, 0, "_TerrainNormalMap", terrainRenderer.NormalMap);
                command.SetComputeBufferParam(computeShader, 0, "_InputTypeIds", inputTypeIdsBuffer);
                command.DispatchNormalized(computeShader, 0, threadCount.x, threadCount.z, 1);
            }

            var instanceData = new InstanceRendererData(result, instanceTypeIds, prefabs, -1);
            instanceRenderer.AddInstanceData(instanceData, command);
        }
    }
}