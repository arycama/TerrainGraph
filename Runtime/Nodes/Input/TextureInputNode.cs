using System;
using NodeGraph;
using UnityEngine;
using UnityEngine.Rendering;

namespace TerrainGraph
{
    [Serializable, NodeMenuItem("Input/Texture")]
    public partial class TextureInputNode : TerrainInputNode
    {
        [SerializeField] private float multiplier = 1;
        [SerializeField] private bool flipVertically = false;
        [SerializeField] private SamplingMode samplingMode = SamplingMode.Bilinear;
        [SerializeField] private Texture2D texture = null;

        public override float Max => multiplier;

        protected override void Generate(TerrainGraph graph, CommandBuffer command)
        {
            if (texture == null)
                return;

            var computeShader = Resources.Load<ComputeShader>("TextureInputNode");
            var kernelIndex = (int)samplingMode;
            command.SetComputeTextureParam(computeShader, kernelIndex, "Input", texture);
            command.SetComputeTextureParam(computeShader, kernelIndex, "Result", result);
            command.SetComputeFloatParam(computeShader, "Multiplier", multiplier);
            command.SetComputeFloatParam(computeShader, "FlipVertically", flipVertically ? 1f : 0f);
            command.DispatchNormalized(computeShader, kernelIndex, graph.Resolution, graph.Resolution, 1);
        }
    }
}