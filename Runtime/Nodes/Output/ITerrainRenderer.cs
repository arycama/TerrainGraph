using UnityEngine.Rendering;

namespace Terrain_Graph
{
    public interface ITerrainRenderer
    {
        public RenderTargetIdentifier Heightmap { get; }
        public RenderTargetIdentifier NormalMap { get; }
    }
}