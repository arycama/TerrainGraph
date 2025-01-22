using System;
using NodeGraph;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Rendering;

namespace TerrainGraph
{
    // TODO: Might be good to make a general OutputNode<T> that we evalulate?
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

        private ITerrainGraphGraphicsBufferHandle positions, instanceTypes, instanceCounts;

        // TODO: Can we somehow return this from the process function instead?
        public InstanceRendererData Result { get; private set; }

        // TODO: We should have some general ShouldEvaluate function which describes whether a node is valid
        private bool ShouldExecute(TerrainGraph graph)
        {
            // No prefabs
            if (prefabs.Length == 0)
                return false;

            // No terrain renderer component (For now this is just used to access height+normal map but..)
            // TODO: This shouldn't be required
            var terrainRenderer = graph.ActiveTerrain.GetComponent<ITerrainRenderer>();
            if (terrainRenderer == null)
                return false;

            // Don't generate if the size or spacing is zero
            var threadCount = Vector3Int.CeilToInt(graph.ActiveTerrain.terrainData.size / spacing);
            var bufferSize = threadCount.x * threadCount.z;
            if (bufferSize < 1)
                return false;
            
            // If all checks pass, return true
            return true;
        }

        // TODO: To avoid grabbing/storing things, maybe make some kind of data context object that gets passed/assigned
        public override void PreProcess(TerrainGraph graph)
        {
            if(!ShouldExecute(graph)) 
                return;

            var threadCount = Vector3Int.CeilToInt(graph.ActiveTerrain.terrainData.size / spacing);
            var bufferSize = threadCount.x * threadCount.z;
            positions = graph.GetGraphicsBuffer(bufferSize, sizeof(float) * 12, GraphicsBuffer.Target.Counter);
            instanceTypes = graph.GetGraphicsBuffer(bufferSize, sizeof(uint));
            instanceCounts = graph.GetGraphicsBuffer(prefabs.Length, sizeof(int), GraphicsBuffer.Target.Structured);
        }

        public override void Process(TerrainGraph graph, CommandBuffer command)
        {
            if (!ShouldExecute(graph))
                return;

            var terrainRenderer = graph.ActiveTerrain.GetComponent<ITerrainRenderer>();
            var offset = graph.ActiveTerrain.GetPosition();
            var threadCount = Vector3Int.CeilToInt(graph.ActiveTerrain.terrainData.size / spacing);
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

            command.SetComputeFloatParam(computeShader, "_TerrainHeightScale", graph.ActiveTerrain.terrainData.size.y);
            command.SetComputeFloatParam(computeShader, "_TerrainHeightOffset", graph.ActiveTerrain.GetPosition().y);

            command.SetBufferCounterValue(positions.GetGraphicsBuffer(), 0);

            // Initialize to zero
            using (ListPool<int>.Get(out var instanceCountsData))
            {
                for (var i = 0; i < prefabs.Length; i++)
                    instanceCountsData.Add(0);
                command.SetBufferData(instanceCounts.GetGraphicsBuffer(), instanceCountsData);
            }

            command.SetComputeFloatParam(computeShader, "_InstanceTypeCount", prefabs.Length);

            var kernelIndex = NodeIsConnected("input") ? 1 : 0;

            // If a probability input is assigned, there is more work to do
            if (NodeIsConnected("input"))
            {
                // We need a "temporary" compute buffer to store the results, but they don't exist, so we must create and then destroy one
                command.SetComputeFloatParam(computeShader, "_MaskMin", GetConnectionMin("input"));
                command.SetComputeFloatParam(computeShader, "_MaskMax", GetConnectionMax("input"));
                command.SetComputeTextureParam(computeShader, kernelIndex, "_Mask", input);
            }

            command.SetComputeTextureParam(computeShader, kernelIndex, "_Heightmap", terrainRenderer.Heightmap);
            command.SetComputeTextureParam(computeShader, kernelIndex, "_TerrainNormalMap", terrainRenderer.NormalMap);
            command.SetComputeBufferParam(computeShader, kernelIndex, "_Result", positions.GetGraphicsBuffer());
            command.SetComputeBufferParam(computeShader, kernelIndex, "_InstanceTypeIds", instanceTypes.GetGraphicsBuffer());
            command.SetComputeBufferParam(computeShader, kernelIndex, "_InstanceTypeCounts", instanceCounts.GetGraphicsBuffer());

            command.DispatchNormalized(computeShader, kernelIndex, threadCount.x, threadCount.z, 1);

            Result = new InstanceRendererData(positions, instanceTypes, instanceCounts, prefabs);
        }
    }
}