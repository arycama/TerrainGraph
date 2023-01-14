using NodeGraph;
using UnityEngine;
using UnityEngine.Rendering;

namespace TerrainGraph
{
    [NodeMenuItem("Input/Alphamap Input")]
    public partial class AlphamapInputNode : TerrainInputNode
    {
        [SerializeField] private TerrainLayer terrainLayer;

        protected override void Generate(TerrainGraph graph, CommandBuffer command)
        {
            var textureManager = graph.ActiveTerrain.GetComponent<ITerrainTextureManager>();
            if (textureManager == null)
                return;

            var input = textureManager.IdMap;
            var index = textureManager.GetTerrainLayerIndex(terrainLayer);

            var computeShader = Resources.Load<ComputeShader>("AlphamapInputNode");
            command.SetComputeTextureParam(computeShader, 0, "_TerrainControlMap", input);
            command.SetComputeTextureParam(computeShader, 0, "_Result", result);
            command.SetComputeIntParam(computeShader, "_TargetLayer", index);
            command.DispatchNormalized(computeShader, 0, graph.Resolution, graph.Resolution, 1);
        }
    }
}