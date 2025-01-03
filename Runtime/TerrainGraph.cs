using System;
using System.Collections.Generic;
using NodeGraph;
using UnityEngine;
using UnityEngine.Rendering;

namespace TerrainGraph
{
    [CreateAssetMenu(menuName = "Data/Terrain/Terrain Graph")]
    public class TerrainGraph : NodeGraph.NodeGraph
    {
        private bool isInitialized;

        public override Type NodeType => typeof(TerrainNode);

        public Terrain ActiveTerrain { get; set; }

        public int Resolution { get; private set; }

        public Func<int, int, GraphicsBuffer.Target, ITerrainGraphGraphicsBufferHandle> CreateGraphicsBufferHandle;

        private void OnEnable()
        {
            Initialize();
        }

        private void OnDisable()
        {
            if (isInitialized)
            {
                foreach (var node in Nodes)
                {
                    if (node != null)
                        node.Cleanup();
                }

                isInitialized = false;
            }
        }

        private void Initialize()
        {
            if (!isInitialized)
            {
                foreach (var node in Nodes)
                {
                    if (node != null)
                        node.Initialize();
                }

                isInitialized = true;
            }
        }

        public void PreGenerate(Terrain terrain, List<BaseNode> nodes, int resolution, CommandBuffer command)
        {
            Initialize();

            ActiveTerrain = terrain;
            Resolution = resolution;

            // Update node order
            UpdateNodeOrder(nodes);

            foreach (var node in nodesToProcess)
            {
                if (node == null)
                    continue;

                if (!(node is TerrainNode terrainNode))
                    continue;

                terrainNode.PreProcess(this, command);
            }
        }

        public void PostGenerate(Terrain terrain, List<BaseNode> nodes, int resolution, CommandBuffer command)
        {
            foreach (var node in nodesToProcess)
            {
                if (node == null)
                    continue;

                node.UpdateValues();

                if (!(node is TerrainNode terrainNode))
                    continue;

                using (var profilerScope = command.ProfilerScope($"{terrainNode.GetType().Name}.Process"))
                    terrainNode.Process(this, command);
            }

            // Cleanup any resources
            foreach (var node in nodesToProcess)
            {
                if (node == null)
                    continue;

                if (!(node is TerrainNode terrainNode))
                    continue;

                using (var profilerScope = command.ProfilerScope($"{terrainNode.GetType().Name}.OnFinishProcess"))
                    terrainNode.OnFinishProcess(this, command);
            }
        }


        public void Generate(Terrain terrain, List<BaseNode> nodes, int resolution, CommandBuffer command)
        {
            PreGenerate(terrain, nodes, resolution, command);
            PostGenerate(terrain, nodes, resolution, command);
        }

        public ITerrainGraphGraphicsBufferHandle GetGraphicsBuffer(int count, int stride, GraphicsBuffer.Target target)
        {
            var handle = CreateGraphicsBufferHandle == null ? new DirectGraphicsBufferHandle(count, stride, target) : CreateGraphicsBufferHandle(count, stride, target);
            return handle;
        }
    }
}