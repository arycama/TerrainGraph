using UnityEngine;

namespace TerrainGraph
{
    public struct DirectGraphicsBufferHandle : ITerrainGraphGraphicsBufferHandle
    {
        private GraphicsBuffer graphicsBuffer;

        public DirectGraphicsBufferHandle(int count, int stride, GraphicsBuffer.Target target)
        {
            graphicsBuffer = new GraphicsBuffer(target, count, stride);
        }

        //void ITerrainGraphGraphicsBufferHandle.AllocateGraphicsBuffer(int count, int stride, GraphicsBuffer.Target target)
        //{
        //}

        GraphicsBuffer ITerrainGraphGraphicsBufferHandle.GetGraphicsBuffer()
        {
            return graphicsBuffer;
        }
    }
}