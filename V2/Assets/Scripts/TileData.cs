using UnityEngine;

[CreateAssetMenu(fileName = "NewMapTile", menuName = "Map/Tile Data")]
[System.Serializable]
public class TileData : ScriptableObject
{
    public string tileName;
    public TileType tileType;
    public bool onFire;
    public bool burnt;
    public int fuelLoad;
    public int currentTileLevel = 0;
}
