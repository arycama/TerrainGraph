using NodeGraph;
using UnityEngine;
using UnityEngine.Rendering;

namespace TerrainGraph
{
    [NodeMenuItem("Input/Constant")]
    public partial class ConstantNode : TerrainInputNode
    {
        [SerializeField]
        private float value = 0;

        public override float Min => value;

        public override float Max => value;

        protected override void Generate(TerrainGraph graph, CommandBuffer command)
        {
            var computeShader = Resources.Load<ComputeShader>("ConstantNode");
            command.SetComputeFloatParam(computeShader, "Value", value);
            command.SetComputeTextureParam(computeShader, 0, "Result", result);
            command.DispatchNormalized(computeShader, 0, graph.Resolution, graph.Resolution, 1);
        }
    }
}