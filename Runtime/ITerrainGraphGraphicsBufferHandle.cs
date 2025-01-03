using UnityEngine;

public interface ITerrainGraphGraphicsBufferHandle
{
    //void AllocateGraphicsBuffer(int count, int stride, GraphicsBuffer.Target target);
    GraphicsBuffer GetGraphicsBuffer();
}
