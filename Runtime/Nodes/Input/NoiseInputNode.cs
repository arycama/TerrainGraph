using System;
using NodeGraph;
using UnityEngine;
using UnityEngine.Rendering;

namespace Terrain_Graph
{
    [NodeMenuItem("Input/Noise")]
    public partial class NoiseInputNode : TerrainInputNode
    {
        [SerializeField, Min(0)]
        private float size = 32;

        [SerializeField, Min(0)]
        private float height = 8;

        [SerializeField, Range(1, 9)]
        private int layers = 3;

        [SerializeField, Range(0, 1)]
        private float gain = 0.5f;

        [SerializeField, Range(1, 2)]
        private float lacunarity = 2f;

        [SerializeField]
        private NoiseType noiseType = NoiseType.Simplex;

        [SerializeField]
        private FractalType fractalType = FractalType.Fbm;

        public override float Max => height * (Mathf.Pow(gain, layers + 1f) - 1f) / (gain - 1f);

        protected override void Generate(TerrainGraph graph, CommandBuffer command)
        {
            var KernelIndex = (int)noiseType * 3 + (int)fractalType;

            var size = graph.ActiveTerrain.terrainData.size;
            var scale = Vector2.one / (graph.Resolution - 1) * new Vector2(size.x, size.z);

            var position = graph.ActiveTerrain.GetPosition();
            var scaleOffset = new Vector4(scale.x, scale.y, position.x, position.z);

            var computeShader = Resources.Load<ComputeShader>("NoiseNode");
            command.SetComputeTextureParam(computeShader, KernelIndex, "Result", result);
            command.SetComputeVectorParam(computeShader, "ScaleOffset", scaleOffset);
            command.SetComputeFloatParam(computeShader, "Frequency", 1f / this.size);
            command.SetComputeFloatParam(computeShader, "Amplitude", height);
            command.SetComputeFloatParam(computeShader, "Gain", gain);
            command.SetComputeFloatParam(computeShader, "Lacunarity", lacunarity);
            command.SetComputeFloatParam(computeShader, "Octaves", layers);
            command.DispatchNormalized(computeShader, KernelIndex, graph.Resolution, graph.Resolution, 1);
        }
    }
}