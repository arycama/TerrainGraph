using NodeGraph;
using UnityEngine;
using UnityEngine.Rendering;

namespace Terrain_Graph
{
    public interface IInstanceRendererGPU
    {
        public int AddInstanceType(GameObject prefab);
        public void AddInstanceData(InstanceRendererData instanceData, CommandBuffer command);
    }
}