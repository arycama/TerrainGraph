using UnityEngine;

namespace TerrainGraph
{
    public struct InstanceRendererData
    {
        public bool IsValid { get; }
        public GameObject[] GameObjects { get; private set; }
        public ITerrainGraphGraphicsBufferHandle PositionBuffer { get; private set; }
        public ITerrainGraphGraphicsBufferHandle InstanceTypeIdBuffer { get; private set; }
        public int Count { get; set; }

        public InstanceRendererData(ITerrainGraphGraphicsBufferHandle positionBuffer, ITerrainGraphGraphicsBufferHandle instanceTypeIdBuffer, GameObject[] gameObjects, int count)
        {
            GameObjects = gameObjects;
            PositionBuffer = positionBuffer;
            InstanceTypeIdBuffer = instanceTypeIdBuffer;
            Count = count;
            IsValid = true;
        }
    }
}