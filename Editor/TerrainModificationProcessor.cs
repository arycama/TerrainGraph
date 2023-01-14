using UnityEngine;

public class TerrainModificationProcessor : UnityEditor.AssetModificationProcessor
{
	private static string[] OnWillSaveAssets(string[] paths)
    {
		foreach (var terrain in Terrain.activeTerrains)
        {
			terrain.terrainData.SyncHeightmap();
			//terrain.terrainData.SyncTexture(TerrainData.AlphamapTextureName);
			//terrain.terrainData.SyncTexture(TerrainData.HolesTextureName);
		}

		return paths;
    }
}