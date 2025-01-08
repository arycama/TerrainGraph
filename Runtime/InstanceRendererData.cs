using UnityEngine;

namespace TerrainGraph
{
    public struct InstanceRendererData
    {
        public bool IsValid { get; }
        public GameObject[] GameObjects { get; private set; }
        public ITerrainGraphGraphicsBufferHandle PositionBuffer { get; private set; }
        public ITerrainGraphGraphicsBufferHandle InstanceTypeIdBuffer { get; private set; }

        public InstanceRendererData(ITerrainGraphGraphicsBufferHandle positionBuffer, ITerrainGraphGraphicsBufferHandle instanceTypeIdBuffer, GameObject[] gameObjects)
        {
            GameObjects = gameObjects;
            PositionBuffer = positionBuffer;
            InstanceTypeIdBuffer = instanceTypeIdBuffer;
            IsValid = true;
        }
    }
}