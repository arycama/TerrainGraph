using NodeGraph;
using UnityEngine;
using UnityEngine.Rendering;

namespace TerrainGraph
{
    public partial class TextureOutputTerrainNode : TerrainNode
    {
        [SerializeField] private int resolution = 1025;
        [SerializeField] private string textureId = string.Empty;
        [SerializeField] private RenderTextureFormat format = RenderTextureFormat.RFloat;
        [SerializeField] private bool hasMips;
        [SerializeField, Tooltip("Sets this keyword to 1 when generated")] private string shaderKeyword;

        [Input] protected RenderTargetIdentifier input;

        private RenderTexture result;

        public float Min { get; private set; }
        public float Max { get; private set; }

        public RenderTexture Result => result;

        private void OnEnable()
        {
            var resultDescriptor = new RenderTextureDescriptor(resolution, resolution, format)
            {
                enableRandomWrite = true,
            };

            result = new RenderTexture(resultDescriptor)
            {
                useMipMap = hasMips,
                autoGenerateMips = false,
                hideFlags = HideFlags.HideAndDontSave,
            }.Created();
        }

        private void OnDisable()
        {
            DestroyImmediate(result);
        }

        public override void Process(TerrainGraph graph, CommandBuffer command)
        {
            if (!string.IsNullOrEmpty(shaderKeyword))
            {
                Shader.SetGlobalFloat(shaderKeyword, 1f);
            }

            if (!NodeIsConnected("input"))
                return;

            var res = graph.Resolution;
            result.Resize(res, res);

            if (result.format != format || result.useMipMap != hasMips)
            {
                result.Release();
                result.format = format;
                result.useMipMap = hasMips;
                result.Create();
            }

            Min = GetConnectionMin("input");
            Max = GetConnectionMax("input");

            command.CopyTexture(input, 0, 0, result, 0, 0);

            if(hasMips)
                command.GenerateMips(result);

            command.SetGlobalTexture(textureId, result);
        }

        public override void OnFinishProcess(TerrainGraph graph, CommandBuffer command)
        {
        }
    }
}