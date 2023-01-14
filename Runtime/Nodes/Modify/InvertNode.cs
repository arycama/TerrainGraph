using NodeGraph;
using UnityEngine;
using UnityEngine.Rendering;

namespace TerrainGraph
{
    /// <summary>
    /// Inverts the input node, so it will go from Max-Min, instead of Min-Max
    /// </summary>
    [NodeMenuItem("Modify/Invert")]
    public partial class InvertNode : TerrainInputNode
    {
        [Input] private RenderTargetIdentifier input;

        public override float Min => GetConnectionMin("input");
        public override float Max => GetConnectionMax("input");

        protected override void Generate(TerrainGraph graph, CommandBuffer command)
        {
            if (!NodeIsConnected("input"))
                return;

            var computeShader = Resources.Load<ComputeShader>("Modify/InvertNode");
            command.SetComputeTextureParam(computeShader, 0, "Input", input);
            command.SetComputeTextureParam(computeShader, 0, "Result", result);
            command.SetComputeFloatParam(computeShader, "_Min", Min);
            command.SetComputeFloatParam(computeShader, "_Max", Max);
            command.DispatchNormalized(computeShader, 0, graph.Resolution, graph.Resolution, 1);
        }
    }
}