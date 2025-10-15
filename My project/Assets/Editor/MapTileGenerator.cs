using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class MapTileGenerator
{
    [MenuItem("Tools/Generate Map Tiles")]
    public static void GenerateTiles()
    {
        string folderPath = "Assets/Tiles";
        if (!AssetDatabase.IsValidFolder(folderPath))
            AssetDatabase.CreateFolder("Assets", "Tiles");

        var tileDataList = new Dictionary<string, TileType>
        {
            {"grassland", TileType.GRASSLAND},
            {"school", TileType.BUILDING},
            {"river", TileType.RIVER},
            {"home", TileType.BUILDING},
            {"farmland", TileType.FARMLAND},
            {"fire_station", TileType.BUILDING},
            {"indigenous_land", TileType.FOREST},
            {"hospital", TileType.BUILDING},
            {"town_center", TileType.BUILDING},
            {"indigenous_land_2", TileType.FOREST},
            {"town_center_2", TileType.BUILDING},
            {"home_2", TileType.BUILDING},
            {"building", TileType.BUILDING},
            {"town_center_3", TileType.BUILDING}
        };

        foreach (var kvp in tileDataList)
        {
            MapTileData asset = ScriptableObject.CreateInstance<MapTileData>();
            asset.tileName = kvp.Key;
            asset.tileType = kvp.Value;
            asset.onFire = false;
            asset.burnt = false;
            asset.fuelLoad = GetDefaultFuelLoad(kvp.Value);

            string assetPath = $"{folderPath}/{kvp.Key}.asset";
            AssetDatabase.CreateAsset(asset, assetPath);
            Debug.Log($"Created {assetPath}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static int GetDefaultFuelLoad(TileType type)
    {
        
        switch (type)
        {
            case TileType.FOREST: return 3;
            case TileType.GRASSLAND: return 2;
            case TileType.FARMLAND: return 1;
            case TileType.BUILDING: return 2;
            case TileType.RIVER: return 0;
            default: return 0;
        }


    }
}

