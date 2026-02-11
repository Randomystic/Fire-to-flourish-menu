using UnityEngine;

public class MapEndConditionStats : MonoBehaviour
{
    [Header("Link")]
    [Tooltip("Assign your Map GameObject here, which should have Tile components in its children.")]
    
    [SerializeField] private string mapRootName = "Map";
    private Transform mapRoot;

    [Header("Cultural Site Settings")]
    [SerializeField] private string culturalSiteName = "Indigenous Land";
    [SerializeField] private int expectedCulturalSites = 3;


    // Computed values
    public int TotalCulturalSites { get; private set; }
    public int DestroyedCulturalSites { get; private set; }

    public int TotalBuildingTiles { get; private set; }

    public int DamagedBuildingTiles { get; private set; }
    public int DestroyedBuildingTiles { get; private set; }


    private void EnsureMapRoot()
    {
        if (mapRoot != null) return;

        GameObject mapGO = GameObject.Find(mapRootName);

        if (mapGO != null) mapRoot = mapGO.transform;
    }


    public void Refresh()
    {
        EnsureMapRoot();
        
        if (mapRoot == null)
        {

            Debug.LogError("[MapEndConditionStats] mapRoot not assigned.");
            return;

        }


        TotalCulturalSites = 0;
        DestroyedCulturalSites = 0;

        TotalBuildingTiles = 0;
        DamagedBuildingTiles = 0;
        DestroyedBuildingTiles = 0;


        Tile[] tiles = mapRoot.GetComponentsInChildren<Tile>(true);




        for (int i = 0; i < tiles.Length; i++)
        {

            Tile t = tiles[i];
            if (t == null || t.tileData == null) continue;

            TileData d = t.tileData;



            // Cultural Sites
            if (IsCulturalSiteByName(d.tileName))
            {
                TotalCulturalSites++;

                if (d.destroyed)
                    DestroyedCulturalSites++;

            }


            // Buildings
            if (d.tileType == TileType.BUILDING)
            {
                TotalBuildingTiles++;

                if (d.destroyed) DestroyedBuildingTiles++;
                else if (d.damaged) DamagedBuildingTiles++;

            }


        }

        if (TotalCulturalSites != expectedCulturalSites)

        {
            Debug.LogWarning(
                $"[MapEndConditionStats] Cultural site count mismatch. " +
                $"Found {TotalCulturalSites} tiles named '{culturalSiteName}', expected {expectedCulturalSites}."
            );

        }


    }



    private bool IsCulturalSiteByName(string tileName)
    {
        if (string.IsNullOrWhiteSpace(tileName)) return false;

        // Minimal but robust: trim + case-insensitive compare
        return string.Equals(tileName.Trim(), culturalSiteName.Trim(), System.StringComparison.OrdinalIgnoreCase);

    }
}
