using UnityEngine;

namespace Terrain_Graph
{
    public interface ITerrainTextureManager
    {
        public RenderTexture IdMap { get; }

        public int AddTerrainLayer(TerrainLayer layer, bool useArray);
        public int GetTerrainLayerIndex(TerrainLayer terrainLayer);
    }
}