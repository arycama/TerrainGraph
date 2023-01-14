using System;
using System.Collections.Generic;
using NodeGraph;
using UnityEngine;
using UnityEngine.Rendering;

namespace Terrain_Graph
{
    [CreateAssetMenu(menuName = "Data/Terrain/Terrain Graph")]
    public class TerrainGraph : NodeGraph.NodeGraph
    {
        private bool isInitialized;

        public override Type NodeType => typeof(TerrainNode);

        public Terrain ActiveTerrain { get; set; }

        public int Resolution { get; private set; }

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

        public void Generate(Terrain terrain, List<BaseNode> nodes, int resolution, CommandBuffer command)
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
    }
}