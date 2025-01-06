using NodeGraph;
using System.Collections.Generic;
using TerrainGraph;
using UnityEngine;
using UnityEngine.Rendering;

namespace TerrainGraph
{
    public class TerrainAlphamapModifier : MonoBehaviour, ITerrainTextureManager
    {
        [SerializeField] private TerrainGraph terrainGraph;

        private TerrainGraph cachedTerrainGraph;
        private int cachedTerrainGraphVersion;

        private Dictionary<TerrainLayer, int> terrainLayers;
        private Dictionary<TerrainLayer, int> terrainLayersProcedural;

        public RenderTexture IdMap { get; private set; }
        public bool NeedsUpdate 
        { 
            get 
            {
                if (terrainGraph != cachedTerrainGraph)
                    return true;

                if (terrainGraph != null && terrainGraph.Version > cachedTerrainGraphVersion)
                    return true;

                return false;
            } 
        }

        public int GetTerrainLayerIndex(TerrainLayer layer)
        {
            if (!terrainLayers.TryGetValue(layer, out var index))
                index = -1;

            return index;
        }

        public int AddTerrainLayer(TerrainLayer terrainLayer, bool useArray)
        {
            if (!terrainLayers.TryGetValue(terrainLayer, out var index))
            {
                index = terrainLayers.Count;
                terrainLayers.Add(terrainLayer, index);
            }

            if (useArray)
            {
                // Procedural layers use a seperate array.
                if (!terrainLayersProcedural.TryGetValue(terrainLayer, out index))
                {
                    index = terrainLayersProcedural.Count;
                    terrainLayersProcedural.Add(terrainLayer, index);
                }
            }

            return index;
        }

        public void PreGenerate(Dictionary<TerrainLayer, int> terrainLayers, Dictionary<TerrainLayer, int> proceduralLayers)
        {
            if (terrainGraph == null)
                return;

            this.terrainLayers = terrainLayers;
            this.terrainLayersProcedural = proceduralLayers;

            var terrain = GetComponent<Terrain>();
            using var nodes = ScopedPooledList<BaseNode>.Get();
            foreach (var node in terrainGraph.Nodes)
            {
                if (node is AlphamapOutputNode)
                {
                    (node as AlphamapOutputNode).PreOutput(terrain);
                }
            }
        }

        public void Generate(CommandBuffer command, Dictionary<TerrainLayer, int> terrainLayers, Dictionary<TerrainLayer, int> proceduralLayers, RenderTexture idMap)
        {
            if (terrainGraph == null)
                return;

            this.terrainLayers = terrainLayers;
            this.terrainLayersProcedural = proceduralLayers;
            this.IdMap = idMap;

            var terrain = GetComponent<Terrain>();
            var terrainData = terrain.terrainData;
            terrainGraph.Generate<AlphamapOutputNode>(terrain, terrainData.alphamapResolution, command);

            cachedTerrainGraph = terrainGraph;
            cachedTerrainGraphVersion = terrainGraph.Version;
        }
    }
}