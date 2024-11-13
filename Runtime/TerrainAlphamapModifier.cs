using NodeGraph;
using System.Collections.Generic;
using TerrainGraph;
using UnityEngine;
using UnityEngine.Rendering;

namespace TerrainGraph
{
    public class TerrainAlphamapModifier : MonoBehaviour, ITerrainTextureManager
    {
        [SerializeField]
        private TerrainGraph terrainGraph;

        private bool needsUpdate;

        private Dictionary<TerrainLayer, int> terrainLayers;
        private Dictionary<TerrainLayer, int> terrainLayersProcedural;

        public RenderTexture IdMap { get; private set; }

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

        private void OnEnable()
        {
            // Setup references


            // Create resource and textures
            //IdMap = new RenderTexture(terrainData.alphamapResolution, terrainData.alphamapResolution, 0, GraphicsFormat.R32_UInt)
            //{
            //    enableRandomWrite = true,
            //    name = "Terrain Id Map",
            //}.Created();

            // Only used to trigger a rebuild on the first update
            needsUpdate = true;

            //if (terrainGraph != null)
            //    terrainGraph.AddListener(OnGraphModified, 1);

            // TerrainCallbacks.textureChanged += OnTextureChanged;
            //TerrainCallbacks.heightmapChanged += OnHeightmapChanged;
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

            using var nodes = ScopedPooledList<BaseNode>.Get();
            foreach (var node in terrainGraph.Nodes)
                if (node is AlphamapOutputNode)
                    nodes.Value.Add(node);

            var terrain = GetComponent<Terrain>();
            var terrainData = terrain.terrainData;
            terrainGraph.Generate(terrain, nodes, terrainData.alphamapResolution, command);
        }

        //private void OnHeightmapChanged(Terrain terrain, RectInt heightRegion, bool synched)
        //{
        //    // Normal map will need to be updated
        //    if (terrain != this.terrain)
        //        return;

        //    // Update the graph, as the heightmap change may trigger texture changes, then update the id map, and inform the virtual texturing system
        //    var command = CommandBufferPool.Get("Terrain heightmap Updates");
        //    UpdateGraph(command);
        //    //UpdateIdMap(command);
        //    Graphics.ExecuteCommandBuffer(command);

        //    VirtualTerrainPostRenderNode.OnTerrainTextureChanged(terrain, heightRegion);
        //}

        //private void OnTextureChanged(Terrain terrain, string textureName, RectInt texelRegion, bool synched)
        //{
        //    if (textureName != TerrainData.AlphamapTextureName || terrain != this.terrain)
        //        return;

        //    // Update the graph, as the heightmap change may trigger texture changes, then update the id map, and inform the virtual texturing system
        //    var command = CommandBufferPool.Get("Terrain Alphamap Update");
        //    UpdateGraph(command);
        //    //UpdateIdMap(command);
        //    Graphics.ExecuteCommandBuffer(command);

        //    VirtualTerrainPostRenderNode.OnTerrainTextureChanged(terrain, texelRegion);
        //}

        private void OnDisable()
        {
            if (terrainGraph != null)
                terrainGraph.RemoveListener(OnGraphModified);

            // TerrainCallbacks.textureChanged -= OnTextureChanged;
            //TerrainCallbacks.heightmapChanged -= OnHeightmapChanged;
        }

        private void OnGraphModified()
        {
            //needsUpdate = false;

            // var command = CommandBufferPool.Get("Terrain Update");
            //UpdateGraph(command);
            //UpdateIdMap(command);
        }

        private void Update()
        {
            // if (needsUpdate)
            //     OnGraphModified();
        }

        private void UpdateGraph(CommandBuffer command)
        {
            if (terrainGraph == null)
                return;

            // terrainLayersProcedural.Clear();

            using var nodes = ScopedPooledList<BaseNode>.Get();
            foreach (var node in terrainGraph.Nodes)
                if (node is AlphamapOutputNode || node is TextureOutputTerrainNode)
                    nodes.Value.Add(node);

            var terrain = GetComponent<Terrain>();
            var terrainData = terrain.terrainData;
            terrainGraph.Generate(terrain, nodes, terrainData.alphamapResolution, command);
        }

        //private void UpdateIdMap(CommandBuffer command)
        //{
        //    var computeShader = Resources.Load<ComputeShader>("GenerateIDMap");
        //    command.SetComputeIntParam(computeShader, "_TextureCount", terrainData.alphamapLayers);
        //    command.SetComputeIntParam(computeShader, "_TotalLayers", terrainLayers.Count);
        //    command.SetComputeTextureParam(computeShader, 0, "_Result", IdMap);
        //    command.SetComputeVectorParam(computeShader, "_Resolution", new Vector2(IdMap.width, IdMap.height));
        //    command.SetComputeBufferParam(computeShader, 0, "_TerrainLayerData", terrainLayerDataBuffer);

        //    // Shader supports up to 8 layers. Can easily be increased by modifying shader though
        //    for (var i = 0; i < 8; i++)
        //    {
        //        var texture = i < terrainData.alphamapTextureCount ? terrainData.alphamapTextures[i] : Texture2D.blackTexture;
        //        command.SetComputeTextureParam(computeShader, 0, $"_Input{i}", texture);
        //    }

        //    // Need to build buffer of layer to array index
        //    var layers = new NativeArray<int>(terrainLayers.Count, Allocator.Temp);
        //    foreach (var layer in terrainLayers)
        //    {
        //        if (terrainLayersProcedural.TryGetValue(layer.Key, out var proceduralIndex))
        //        {
        //            // Use +1 so we can use 0 to indicate no data
        //            layers[layer.Value] = proceduralIndex + 1;
        //        }
        //    }

        //    command.SetBufferData(indexBuffer, layers);

        //    var tempArrayId = Shader.PropertyToID("_TempTerrainId");
        //    command.SetComputeTextureParam(computeShader, 0, "_ExtraLayers", tempArrayId);
        //    command.SetComputeBufferParam(computeShader, 0, "_ProceduralIndices", indexBuffer);
        //    command.DispatchNormalized(computeShader, 0, terrainData.alphamapResolution, terrainData.alphamapResolution, 1);
        //    command.ReleaseTemporaryRT(tempArrayId);
        //}
    }
}