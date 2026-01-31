using UnityEngine;

[CreateAssetMenu(fileName = "NewMapTile", menuName = "Map/Tile Data")]
public class MapTileData : ScriptableObject
{
    public string tileName;
    public TileType tileType;
    public bool onFire;
    public bool burnt;
    public int fuelLoad;
}
