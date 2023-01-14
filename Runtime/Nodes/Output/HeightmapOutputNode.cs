using System;
using NodeGraph;
using UnityEngine;
using UnityEngine.Rendering;

namespace Terrain_Graph
{
    [Serializable, NodeMenuItem("Output/Heightmap Output")]
    public partial class HeightmapOutputNode : TerrainNode
    {
        [Input] private RenderTargetIdentifier input;

        public override void Process(TerrainGraph graph, CommandBuffer command)
        {
            if (!NodeIsConnected("input"))
                return;

            var Min = graph.ActiveTerrain.GetPosition().y;
            var Max = Min + graph.ActiveTerrain.terrainData.size.y;
            var terrainRenderer = graph.ActiveTerrain.GetComponent<ITerrainRenderer>();

            var computeShader = Resources.Load<ComputeShader>("HeightmapOutputNode");
            command.SetComputeTextureParam(computeShader, 0, "Input", input);
            command.SetComputeTextureParam(computeShader, 0, "Result", terrainRenderer.Heightmap);
            command.SetComputeFloatParam(computeShader, "_Min", Min);
            command.SetComputeFloatParam(computeShader, "_Max", Max);
            command.DispatchNormalized(computeShader, 0, graph.Resolution, graph.Resolution, 1);
        }
    }
}