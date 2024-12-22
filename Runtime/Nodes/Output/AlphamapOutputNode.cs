using System;
using NodeGraph;
using UnityEngine;
using UnityEngine.Rendering;

namespace TerrainGraph
{
    [Serializable, NodeMenuItem("Output/Alphamap Output")]
    public partial class AlphamapOutputNode : TerrainNode
    {
        [SerializeField] private AlphamapOutputMode mode = AlphamapOutputMode.Blend;
        [SerializeField] private TerrainLayer terrainLayer = null;

        [Input] private RenderTargetIdentifier mask;
        [Input] private RenderTargetIdentifier input;

        public TerrainLayer TerrainLayer => terrainLayer;
        public override bool HasPreviewTexture => true;

        private int index;

        // Need to setup the layers/data before executing
        public void PreOutput(Terrain terrain)
        {
            if (terrainLayer == null)
                return;

            var component = terrain.GetComponent<ITerrainTextureManager>();
            index = component.AddTerrainLayer(terrainLayer, true);
        }

        public override void Process(TerrainGraph graph, CommandBuffer command)
        {
            if (terrainLayer == null)
                return;

           // var previewMaterial = MaterialPool.Get("Hidden/Terrain Node Preview");
            //command.Blit(terrainLayer.diffuseTexture, PreviewTexture, previewMaterial, 2);

            if (!NodeIsConnected("input"))
                return;

            var computeShader = Resources.Load<ComputeShader>("AlphamapOutputNode");

            if (NodeIsConnected("mask"))
            {
                command.SetComputeTextureParam(computeShader, 0, "Mask", mask);
                command.SetComputeFloatParam(computeShader, "_MaskMin", GetConnectionMin("mask"));
                command.SetComputeFloatParam(computeShader, "_MaskMax", GetConnectionMax("mask"));
                command.SetComputeIntParam(computeShader, "_Mask", 1);
            }
            else
            {
                command.SetComputeTextureParam(computeShader, 0, "Mask", Texture2D.blackTexture);
                command.SetComputeIntParam(computeShader, "_Mask", 0);
            }

            var tempArrayId = Shader.PropertyToID("_TempTerrainId");
            command.SetComputeTextureParam(computeShader, 0, "Input", input);
            command.SetComputeTextureParam(computeShader, 0, "_Alphamap", tempArrayId);
            command.SetComputeFloatParam(computeShader, "_Min", GetConnectionMin("input"));
            command.SetComputeFloatParam(computeShader, "_Max", GetConnectionMax("input"));
            command.SetComputeIntParam(computeShader, "_Index", index);
            command.SetComputeVectorParam(computeShader, "_Resolution", new Vector2(graph.Resolution, graph.Resolution));
            command.DispatchNormalized(computeShader, 0, graph.Resolution, graph.Resolution, 1);
        }
    }
    public enum AlphamapOutputMode
    {
        Blend,
        Replace
    }
}