using NodeGraph;
using UnityEngine;
using UnityEngine.Rendering;

namespace TerrainGraph
{
    /// <summary>
    /// Remaps input from Min/Max to 0-1
    /// </summary>
    [NodeMenuItem("Modify/Normalize")]
    public partial class NormalizeNode : TerrainInputNode
    {
        [Input] private RenderTargetIdentifier input;

        protected override void Generate(TerrainGraph graph, CommandBuffer command)
        {
            if (!NodeIsConnected("input"))
                return;

            var computeShader = Resources.Load<ComputeShader>("Modify/NormalizeNode");
            command.SetComputeTextureParam(computeShader, 0, "Input", input);
            command.SetComputeTextureParam(computeShader, 0, "Result", result);
            command.SetComputeFloatParam(computeShader, "_Min", GetConnectionMin("input"));
            command.SetComputeFloatParam(computeShader, "_Max", GetConnectionMax("input"));
            command.DispatchNormalized(computeShader, 0, graph.Resolution, graph.Resolution, 1);
        }
    }
}