using NodeGraph;
using UnityEngine;
using UnityEngine.Rendering;

namespace Terrain_Graph
{
    [NodeMenuItem("Modify/Terrace")]
    public partial class TerraceNode : TerrainInputNode
    {
        [SerializeField] private float size = 8f;
        [SerializeField, Range(0, 8)] private float shape = 0.5f;

        [Input] private RenderTargetIdentifier input;

        public override float Min => GetConnectionMin("input");
        public override float Max => GetConnectionMax("input");

        protected override void Generate(TerrainGraph graph, CommandBuffer command)
        {
            if (!NodeIsConnected("input"))
                return;

            var computeShader = Resources.Load<ComputeShader>("Modify/TerraceNode");
            command.SetComputeTextureParam(computeShader, 0, "Input", input);
            command.SetComputeTextureParam(computeShader, 0, "Result", result);
            command.SetComputeFloatParam(computeShader, "Size", size);
            command.SetComputeFloatParam(computeShader, "Shape", shape);
            command.DispatchNormalized(computeShader, 0, graph.Resolution, graph.Resolution, 1);
        }
    }
}