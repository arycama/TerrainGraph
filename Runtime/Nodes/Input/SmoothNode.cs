using System;
using NodeGraph;
using UnityEngine;
using UnityEngine.Rendering;

namespace TerrainGraph
{
    [NodeMenuItem("Modify/Smooth")]
    public partial class SmoothNode : TerrainInputNode
    {
        [SerializeField, Range(0, 32)] private int radius = 3;
        [SerializeField] private bool gaussian = true;

        [Input] private RenderTargetIdentifier input;

        private int tempId;
        private ComputeShader computeShader;

        public override float Min => GetConnectionMin("input");

        public override float Max => GetConnectionMax("input");

        public override void Initialize()
        {
            base.Initialize();

            tempId = Shader.PropertyToID($"{GetType()}_{GetInstanceID()}_Temp");
            computeShader = Resources.Load<ComputeShader>("Modify/SmoothNode");
        }

        protected override void Generate(TerrainGraph graph, CommandBuffer command)
        {
            if (!NodeIsConnected("input"))
                return;

            var tempDescriptor = new RenderTextureDescriptor(graph.Resolution, graph.Resolution, RenderTextureFormat.RFloat)
            {
                enableRandomWrite = true,
            };

            command.GetTemporaryRT(tempId, tempDescriptor);
            command.SetComputeIntParam(computeShader, "_Gaussian", gaussian ? 1 : 0);
            command.SetComputeIntParam(computeShader, "_Radius", radius);

            command.SetComputeTextureParam(computeShader, 0, "Input", input);
            command.SetComputeTextureParam(computeShader, 0, "Result", tempId);
            command.DispatchNormalized(computeShader, 0, graph.Resolution, graph.Resolution, 1);

            command.SetComputeTextureParam(computeShader, 1, "Input", tempId);
            command.SetComputeTextureParam(computeShader, 1, "Result", result);
            command.DispatchNormalized(computeShader, 1, graph.Resolution, graph.Resolution, 1);
            command.ReleaseTemporaryRT(tempId);
        }
    }
}