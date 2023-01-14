using UnityEngine.Rendering;

namespace TerrainGraph
{
    public interface ITerrainRenderer
    {
        public RenderTargetIdentifier Heightmap { get; }
        public RenderTargetIdentifier NormalMap { get; }
    }
}