using UnityEngine;
using UnityEngine.Rendering;

namespace TerrainGraph
{
    public interface ITerrainRenderer
    {
        public RenderTexture Heightmap { get; }
        public RenderTexture NormalMap { get; }
    }
}