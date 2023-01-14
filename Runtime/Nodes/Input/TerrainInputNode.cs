using NodeGraph;
using UnityEngine;
using UnityEngine.Rendering;

namespace TerrainGraph
{
    public abstract partial class TerrainInputNode : TerrainNode
    {
        [Input] protected RenderTargetIdentifier mask;

        [Output] protected RenderTargetIdentifier result;
        [Output] protected RenderTargetIdentifier inverse;

        public virtual float Min => 0f;
        public virtual float Max => 1f;

        protected TerrainGraph terrainGraph;

        private int resultId;
        private int inverseId;

        public override bool HasPreviewTexture => true;

        public override void Initialize()
        {
            base.Initialize();

            resultId = Shader.PropertyToID($"{GetType()}_{GetInstanceID()}_Result");
            inverseId = Shader.PropertyToID($"{GetType()}_{GetInstanceID()}_Inverse");

            result = resultId;
            inverse = inverseId;
        }

        public override void Process(TerrainGraph graph, CommandBuffer command)
        {
            terrainGraph = graph;
            var resolution = graph.Resolution;

            var descriptor = new RenderTextureDescriptor(resolution, resolution, RenderTextureFormat.RFloat)
            {
                autoGenerateMips = false,
                enableRandomWrite = true,
                useMipMap = true,
            };

            command.GetTemporaryRT(resultId, descriptor);
            Generate(graph, command);

            // After generating the result, we use the inverse CS to create an inverted version
            // Should only do this if inverse is connected, but we only store one-way connections.
            // SHould probably handle this by just inverting on the input of the connected node anyway.
            // (Though then we'd need to make sure we're also applying the mask from this node to the inverted result)
            //if (NodeIsConnected("inverse"))
            {
                var invertCS = Resources.Load<ComputeShader>("Modify/InvertNode");
                command.GetTemporaryRT(inverseId, descriptor);
                command.SetComputeTextureParam(invertCS, 0, "Input", result);
                command.SetComputeTextureParam(invertCS, 0, "Result", inverse);
                command.SetComputeFloatParam(invertCS, "_Min", Min);
                command.SetComputeFloatParam(invertCS, "_Max", Max);
                command.DispatchNormalized(invertCS, 0, resolution, resolution, 1);

                // Apply mask to inverse
                if (NodeIsConnected("mask"))
                {
                    var maskCS = Resources.Load<ComputeShader>("Modify/MaskNode");
                    command.SetComputeTextureParam(maskCS, 0, "Mask", mask);
                    command.SetComputeTextureParam(maskCS, 0, "Result", inverse);
                    command.SetComputeVectorParam(maskCS, "_ChannelMask", new Vector4(1, 0, 0, 0));
                    command.SetComputeFloatParam(maskCS, "_Min", GetConnectionMin("mask"));
                    command.SetComputeFloatParam(maskCS, "_Max", GetConnectionMax("mask"));
                    command.DispatchNormalized(maskCS, 0, resolution, resolution, 1);
                }
            }

            // Apply mask to result
            if (NodeIsConnected("mask"))
            {
                var maskCS = Resources.Load<ComputeShader>("Modify/MaskNode");
                command.SetComputeTextureParam(maskCS, 0, "Mask", mask);
                command.SetComputeTextureParam(maskCS, 0, "Result", result);
                command.SetComputeFloatParam(maskCS, "_Min", GetConnectionMin("mask"));
                command.SetComputeFloatParam(maskCS, "_Max", GetConnectionMax("mask"));
                command.DispatchNormalized(maskCS, 0, resolution, resolution, 1);
            }

            // Update preview image
            // This might mess with rendering..
            var previewMaterial = MaterialPool.Get("Hidden/Terrain Node Preview");

            // TODO: Copy into temp texture with mips enabled?
            command.GenerateMips(result);
            command.SetGlobalVector("_MinMax", new Vector2(Min, Max));
            command.Blit(result, PreviewTexture, previewMaterial, 1);

            // Update min/max range display
            //label.text = $"Range: ({node.Min:F1}:{node.Max:F1})";
        }

        protected abstract void Generate(TerrainGraph graph, CommandBuffer command);

        public override void OnFinishProcess(TerrainGraph graph, CommandBuffer command)
        {
            command.ReleaseTemporaryRT(resultId);
            command.ReleaseTemporaryRT(inverseId);
        }
    }
}