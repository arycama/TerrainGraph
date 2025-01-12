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

        public Func<int, int, GraphicsBuffer.Target, GraphicsBuffer.UsageFlags, ITerrainGraphGraphicsBufferHandle> CreateGraphicsBufferHandle;

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

        public void PreGenerate<T>(Terrain terrain, int resolution) where T : TerrainNode
        {
            using var nodes = ScopedPooledList<T>.Get();
            foreach (var node in Nodes)
                if (node is T typedNode)
                    nodes.Value.Add(typedNode);

            Initialize();

            ActiveTerrain = terrain;
            Resolution = resolution;

            // Update node order
            UpdateNodeOrder<T>(nodes);

            foreach (var node in nodesToProcess)
            {
                if (node == null)
                    continue;

                if (node is not TerrainNode terrainNode)
                    continue;

                terrainNode.PreProcess(this);
            }
        }

        public void PostGenerate(CommandBuffer command)
        {
            foreach (var node in nodesToProcess)
            {
                if (node == null)
                    continue;

                node.UpdateValues();

                if (node is not TerrainNode terrainNode)
                    continue;

                    terrainNode.Process(this, command);
            }

            // Cleanup any resources
            foreach (var node in nodesToProcess)
            {
                if (node == null)
                    continue;

                if (node is not TerrainNode terrainNode)
                    continue;

                    terrainNode.OnFinishProcess(this, command);
            }
        }

        public IEnumerable<T> PostGenerate<T>(CommandBuffer command)
        {
            foreach (var node in nodesToProcess)
            {
                if (node == null)
                    continue;

                node.UpdateValues();

                if (node is not TerrainNode terrainNode)
                    continue;

                terrainNode.Process(this, command);

                if (node is T typedNode)
                    yield return typedNode;
            }

            // Cleanup any resources
            foreach (var node in nodesToProcess)
            {
                if (node == null)
                    continue;

                if (node is not TerrainNode terrainNode)
                    continue;

                terrainNode.OnFinishProcess(this, command);
            }
        }

        public void Generate<T>(Terrain terrain, int resolution, CommandBuffer command) where T : TerrainNode
        {
            PreGenerate<T>(terrain, resolution);
            PostGenerate(command);
        }

        public ITerrainGraphGraphicsBufferHandle GetGraphicsBuffer(int count, int stride, GraphicsBuffer.Target target = GraphicsBuffer.Target.Structured, GraphicsBuffer.UsageFlags usageFlags = GraphicsBuffer.UsageFlags.None)
        {
            var handle = CreateGraphicsBufferHandle == null ? new DirectGraphicsBufferHandle(count, stride, target, usageFlags) : CreateGraphicsBufferHandle(count, stride, target, usageFlags);
            return handle;
        }
    }
}