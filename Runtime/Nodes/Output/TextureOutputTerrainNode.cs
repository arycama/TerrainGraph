using NodeGraph;
using UnityEngine;
using UnityEngine.Rendering;

namespace Terrain_Graph
{
    public partial class TextureOutputTerrainNode : TerrainNode
    {
        [SerializeField] private string textureId = string.Empty;
        [SerializeField] private RenderTextureFormat format = RenderTextureFormat.RFloat;
        [SerializeField, Tooltip("Sets this keyword to 1 when generated")] private string shaderKeyword;

        [Input] protected RenderTexture input = null;

        private RenderTexture result;

        public float Min { get; private set; }
        public float Max { get; private set; }

        public RenderTexture Result => result;

        private void OnEnable()
        {
            var resolution = 1025;
            var resultDescriptor = new RenderTextureDescriptor(resolution, resolution, format)
            {
                enableRandomWrite = true,
            };

            result = new RenderTexture(resultDescriptor)
            {
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

            if (input == null)
                return;

            var res = graph.Resolution;
            result.Resize(res, res);

            if (result.format != format)
            {
                result.Release();
                result.format = format;
                result.Create();
            }

            Min = GetConnectionMin("input");
            Max = GetConnectionMax("input");
            Graphics.CopyTexture(input, 0, 0, result, 0, 0);

            Shader.SetGlobalTexture(textureId, result);
        }

        public override void OnFinishProcess(TerrainGraph graph, CommandBuffer command)
        {
        }
    }
}