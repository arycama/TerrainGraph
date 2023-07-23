using NodeGraph;
using UnityEngine;
using UnityEngine.Rendering;

namespace TerrainGraph
{
    [NodeMenuItem("Modify/Ambient Occlusion")]
    public partial class TerrainAmbientOcclusionNode : TerrainInputNode
    {
        [Input] private RenderTargetIdentifier input;

        protected override RenderTextureFormat GetFormat => RenderTextureFormat.ARGB32;

        protected override void Generate(TerrainGraph graph, CommandBuffer command)
        {
            if (!NodeIsConnected("input"))
                return;

            var computeShader = Resources.Load<ComputeShader>("AmbientOcclusionNode");
            command.SetComputeTextureParam(computeShader, 0, "Input", input);
            command.SetComputeTextureParam(computeShader, 0, "Result", result);
            command.SetComputeIntParam(computeShader, "Resolution", graph.Resolution);
            command.SetComputeVectorParam(computeShader, "Scale", graph.ActiveTerrain.terrainData.heightmapScale);
            command.SetComputeVectorParam(computeShader, "TerrainSize", graph.ActiveTerrain.terrainData.size);
            command.SetComputeFloatParam(computeShader, "Height", graph.ActiveTerrain.terrainData.size.y);
            command.SetComputeFloatParam(computeShader, "Offset", graph.ActiveTerrain.GetPosition().y);

            command.DispatchNormalized(computeShader, 0, graph.Resolution, graph.Resolution, 1);
        }
    }
}