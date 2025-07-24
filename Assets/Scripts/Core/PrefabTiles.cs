using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Tiles/Prefab Tile")]
public class PrefabTile : TileBase
{
    public GameObject prefab;

    public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
    {
        if (prefab != null)
        {
            // Create the prefab at the correct location in the world
            GameObject instance = Object.Instantiate(prefab, position + tilemap.GetComponent<Tilemap>().cellSize / 2, Quaternion.identity);
            instance.name = prefab.name;
        }
        return true;
    }
}
