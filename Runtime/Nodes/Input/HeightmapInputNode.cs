using NodeGraph;
using UnityEngine;
using UnityEngine.Rendering;

namespace TerrainGraph
{
    [NodeMenuItem("Input/Heightmap Input")]
    public partial class HeightmapInputNode : TerrainInputNode
    {
        public override float Min => terrainGraph.ActiveTerrain == null ? 0 : terrainGraph.ActiveTerrain.GetPosition().y;

        public override float Max => terrainGraph.ActiveTerrain == null ? 1 : terrainGraph.ActiveTerrain.terrainData.size.y + terrainGraph.ActiveTerrain.GetPosition().y;

        private int inputId;

        public override void Initialize()
        {
            base.Initialize();

            inputId = Shader.PropertyToID($"{GetType()}_{GetInstanceID()}_Input");
        }

        protected override void Generate(TerrainGraph graph, CommandBuffer command)
        {
            var heightmap = graph.ActiveTerrain.terrainData.heightmapTexture;
            var heightmapResolution = graph.ActiveTerrain.terrainData.heightmapResolution;
            if (heightmap == null)
                return;

            var descriptor = new RenderTextureDescriptor(heightmapResolution, heightmapResolution, RenderTextureFormat.R16)
            {
                autoGenerateMips = false,
                enableRandomWrite = true,
                useMipMap = true,
            };

            // Heightmap may not match out current res (Eg for alphamap), so copy to a temp texture with mips, and sample in shader
            command.GetTemporaryRT(inputId, descriptor);
            command.CopyTexture(heightmap, 0, 0, inputId, 0, 0);
            command.GenerateMips(inputId);

            var computeShader = Resources.Load<ComputeShader>("HeightmapInputNode");
            command.SetComputeTextureParam(computeShader, 0, "Input", inputId);
            command.SetComputeTextureParam(computeShader, 0, "Result", result);
            command.SetComputeFloatParam(computeShader, "Height", graph.ActiveTerrain.terrainData.size.y * 2);
            command.SetComputeFloatParam(computeShader, "Offset", graph.ActiveTerrain.GetPosition().y);
            command.SetComputeFloatParam(computeShader, "Resolution", graph.Resolution);
            command.DispatchNormalized(computeShader, 0, graph.Resolution, graph.Resolution, 1);

            command.ReleaseTemporaryRT(inputId);
        }
    }
}