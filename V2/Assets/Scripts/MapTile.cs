using UnityEngine;

public class MapTile : MonoBehaviour
{
    public string tileName;
    public TileType tileType;
    public bool onFire;
    public bool burnt;
    public int fuelLoad;

    // Store cube coordinate for this tile (x+y+z = 0)
    public Vector3Int cubeCoord;

    public bool IsOnFire() => onFire;
    public bool IsBurnt()  => burnt;

    public void AdjustFuelLoad(int amount)
    {
        fuelLoad = Mathf.Clamp(fuelLoad + amount, 0, 4);
    }
    
}

public enum TileType
{
    FOREST,
    GRASSLAND,
    FARMLAND,
    BUILDING,
    WATERBODY
}
