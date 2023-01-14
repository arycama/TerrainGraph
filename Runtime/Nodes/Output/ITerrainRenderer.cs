using UnityEngine.Rendering;

public interface ITerrainRenderer
{
	public RenderTargetIdentifier Heightmap { get; }
	public RenderTargetIdentifier NormalMap { get; }
}