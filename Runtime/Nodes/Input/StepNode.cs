using NodeGraph;
using UnityEngine;
using UnityEngine.Rendering;

namespace TerrainGraph
{
    [NodeMenuItem("Modify/Step")]
    public partial class StepNode : TerrainInputNode
    {
        [SerializeField] private float threshold = 0.5f;
        [SerializeField, Min(0)] private float falloff = 1f;

        [Input] private RenderTargetIdentifier input;

        protected override void Generate(TerrainGraph graph, CommandBuffer command)
        {
            if (!NodeIsConnected("input"))
                return;

            var computeShader = Resources.Load<ComputeShader>("Modify/StepNode");
            command.SetComputeTextureParam(computeShader, 0, "Input", input);
            command.SetComputeTextureParam(computeShader, 0, "Result", result);
            command.SetComputeFloatParam(computeShader, "Threshold", threshold);
            command.SetComputeFloatParam(computeShader, "Falloff", falloff);
            command.DispatchNormalized(computeShader, 0, graph.Resolution, graph.Resolution, 1);
        }
    }
}