using UnityEngine;

namespace TerrainGraph
{
    public struct DirectGraphicsBufferHandle : ITerrainGraphGraphicsBufferHandle
    {
        private GraphicsBuffer graphicsBuffer;

        public DirectGraphicsBufferHandle(int count, int stride, GraphicsBuffer.Target target, GraphicsBuffer.UsageFlags usageFlags)
        {
            graphicsBuffer = new GraphicsBuffer(target, usageFlags, count, stride);
        }

        GraphicsBuffer ITerrainGraphGraphicsBufferHandle.GetGraphicsBuffer()
        {
            return graphicsBuffer;
        }
    }
}