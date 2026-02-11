using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TileData
{
    public string tileName;
    public TileType tileType;
    public int onFire = 0; // 0 = not on fire, 1 = on fire, 2 = heavily on fire
    public bool destroyed = false; 
    public bool damaged = false; 
    public int fuelLoad = 0;
    public int currentTileLevel = 0;

    public List<TileUpgrade> upgrades;
}

[Serializable]
public class TileUpgrade {
    public Sprite newSprite;
    public TileType newTileType; // May be useful?
    
    // TODO: Add other attribute upgrade perhaps?
}

public enum TileType
{
    FOREST,
    GRASSLAND,
    GROUNDLAND,
    FARMLAND,
    BUILDING,
    WATERBODY
}
