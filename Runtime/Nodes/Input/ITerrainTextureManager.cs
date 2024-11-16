using UnityEngine;

namespace TerrainGraph
{
    public interface ITerrainTextureManager
    {
        public bool NeedsUpdate { get; }
        public RenderTexture IdMap { get; }

        public int AddTerrainLayer(TerrainLayer layer, bool useArray);
        public int GetTerrainLayerIndex(TerrainLayer terrainLayer);
    }
}