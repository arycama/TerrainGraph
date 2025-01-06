using NodeGraph;
using UnityEngine.Rendering;

namespace TerrainGraph
{
    public abstract class TerrainNode : BaseNode
    {
        protected float GetConnectionMin(string fieldName)
        {
            var node = GetConnectedNode(fieldName) as TerrainInputNode;
            return node != null ? node.Min : 0;
        }

        protected float GetConnectionMax(string fieldName)
        {
            var node = GetConnectedNode(fieldName) as TerrainInputNode;
            return node != null ? node.Max : 1;
        }

        // Used to allocate resources before executing
        public virtual void PreProcess(TerrainGraph graph) { }

        public abstract void Process(TerrainGraph graph, CommandBuffer command);

        public virtual void OnFinishProcess(TerrainGraph graph, CommandBuffer command) { }
    }
}