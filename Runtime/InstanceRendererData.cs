using UnityEngine;

namespace TerrainGraph
{
    public struct InstanceRendererData
    {
        public bool IsValid { get; }
        public GameObject[] GameObjects { get; private set; }
        public ITerrainGraphGraphicsBufferHandle Positions { get; private set; }
        public ITerrainGraphGraphicsBufferHandle InstanceTypes { get; private set; }
        public ITerrainGraphGraphicsBufferHandle InstanceTypeCounts { get; private set; }

        public InstanceRendererData(ITerrainGraphGraphicsBufferHandle positionBuffer, ITerrainGraphGraphicsBufferHandle instanceTypeIdBuffer, ITerrainGraphGraphicsBufferHandle instanceTypeCounts, GameObject[] gameObjects)
        {
            GameObjects = gameObjects;
            Positions = positionBuffer;
            InstanceTypes = instanceTypeIdBuffer;
            InstanceTypeCounts = instanceTypeCounts;
            IsValid = true;
        }
    }
}