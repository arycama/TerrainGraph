using UnityEngine;
using UnityEngine.Rendering;
using NodeGraph;

public interface IInstanceRendererGPU
{
    public int AddInstanceType(GameObject prefab);
    public void AddInstanceData(InstanceRendererData instanceData, CommandBuffer command);
}