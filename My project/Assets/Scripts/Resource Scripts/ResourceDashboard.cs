using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceDashboard : MonoBehaviour
{
    public Map map;
    public List<Player> players = new List<Player>();
    public TownResourceList townResource;

    public DashboardContent GetDashboardContent()
    {
        if (townResource == null) return new DashboardContent();
        return new DashboardContent
        {
            Provisions = townResource.provisions,
            Education  = townResource.education,
            // Population = townResource.population
            Happiness = townResource.happiness,
            FirefightingEquipment = townResource.firefightingEquipment,
            FireSafetyRating = townResource.fireSafetyRating,
            WindSpeed = townResource.windSpeed,
            TemperatureSeason = townResource.temperatureSeason
        };
    }

    public static void RecalculateFireSafety(Map map, TownResourceList town)
    {
        if (map == null || town == null) return;

        town.averageFuelLoad = map.GetAverageFuelLoad();
        float newRating = town.CalculateFireSafety();
        town.fireSafetyRating = Mathf.RoundToInt(newRating);

        Debug.Log($"Fire Safety recalculated -> {newRating:F2} (AvgFuel={town.averageFuelLoad:F2})");
    }

    
}

public class DashboardContent
{
    public int Provisions;
    public int Education;
    // public int Population;
    public int Happiness;
    public int FirefightingEquipment;
    public int FireSafetyRating;
    public int WindSpeed;
    public int TemperatureSeason;
}
