using NodeGraph;
using UnityEngine;
using UnityEngine.Rendering;

namespace TerrainGraph
{
    [NodeMenuItem("Modify/Slope")]
    public partial class SlopeNode : TerrainInputNode
    {
        [Input] private RenderTargetIdentifier input;

        protected override void Generate(TerrainGraph graph, CommandBuffer command)
        {
            if (!NodeIsConnected("input"))
                return;

            var computeShader = Resources.Load<ComputeShader>("Modify/SlopeNode");
            command.SetComputeTextureParam(computeShader, 0, "Input", input);
            command.SetComputeTextureParam(computeShader, 0, "Result", result);
            command.SetComputeVectorParam(computeShader, "Scale", graph.ActiveTerrain.terrainData.heightmapScale);
            command.DispatchNormalized(computeShader, 0, graph.Resolution, graph.Resolution, 1);
        }
    }
}