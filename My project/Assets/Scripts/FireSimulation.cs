using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class FireSimulation : MonoBehaviour
{
    [Header("References")]
    public Map map;
    public TownResourceList townResources;
    public TMP_Text resultsText; // assign in Inspector

    [Header("Simulation Settings")]
    [Range(0f, 1f)] public float burnMultiplier = 0.1f;

    private readonly List<MapTile> burnedTiles = new();
    private int totalTiles;

    void Start()
    {
        // --- GET OR SPAWN MAP ---
        if (!map) map = Map.Instance;
        if (!map) map = FindObjectOfType<Map>(true);

        if (!map)
        {
            var prefabGO = Resources.Load<GameObject>("MapRunTime");
            if (prefabGO)
            {
                var inst = Instantiate(prefabGO);
                inst.name = "MapRunTime (spawned)";
                map = inst.GetComponent<Map>();
            }
            else
            {
                Debug.LogError("FireSimulation: No Map found or spawned.");
                return;
            }
        }

        // --- ENSURE THE MAP IS INITIALIZED & GENERATED ---
        map.EnsureInitialized();

        // If tiles aren't generated yet, force-generate using its method
        if (map.tiles.Count == 0)
        {
            Debug.LogWarning("FireSimulation: Map has no tiles. Generating now...");
            
            // Try to generate using its default pattern
            var generateMethod = map.GetType().GetMethod("Generate_1_2_3_2_1", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (generateMethod != null)
                generateMethod.Invoke(map, null);
        }

        Debug.Log($"FireSimulation Ready → Found {map.tiles.Count} tiles.");
        if (map.tiles.Count == 0)
        {
            Debug.LogError("FireSimulation: Still no tiles after trying to generate. Aborting.");
            return;
        }

        map.SetMapVisibility(false);

        // --- ENSURE TOWN RESOURCES ---
        if (!townResources)
            townResources = Resources.Load<TownResourceList>("TownResources");

        if (!townResources)
        {
            Debug.LogError("FireSimulation: No TownResources found.");
            return;
        }

        // --- RUN SIMULATION ---
        RunSimulation();
    }


    void RunSimulation()
    {
        burnedTiles.Clear();

        foreach (var tile in map.tiles.Values)  // <-- USING THE DICTIONARY DIRECTLY
        {
            float risk = Mathf.Clamp01(townResources.fireSafetyRating / 100f) * (2f * burnMultiplier * tile.fuelLoad);
            bool burns = Random.value < risk;

            if (burns)
            {
                tile.onFire = true;
                tile.burnt = true;
                burnedTiles.Add(tile);
            }
        }

        totalTiles = map.tiles.Count;
        Debug.Log($"Simulation Complete → Burned {burnedTiles.Count} / {totalTiles} tiles.");
        CheckGameEndConditions();
    }

    void CheckGameEndConditions()
    {
        bool fireOutOfControl = burnedTiles.Count >= 0.3f * totalTiles;
        bool culturalLoss = CheckCulturalDamage();
        bool satisfactionLoss = CheckSatisfactionLoss(out float satisfaction);

        string reason = "";
        bool lost = false;

        if (fireOutOfControl)
        {
            reason = "The wildfire became uncontrollable.";
            lost = true;
        }
        else if (culturalLoss)
        {
            reason = "Irreparable cultural loss occurred. Indigenous land was destroyed.";
            lost = true;
        }
        else if (satisfactionLoss)
        {
            reason = "The town’s satisfaction and morale dropped below sustainable levels.";
            lost = true;
        }

        // Display Results
        if (resultsText)
        {
            resultsText.text =
                $"FIRE SIMULATION RESULTS\n\n" +
                $"Tiles Burned: {burnedTiles.Count}/{totalTiles}\n" +
                $"Cultural Damage: {(culturalLoss ? "Yes" : "No")}\n" +
                $"Satisfaction: {satisfaction:F1}/100\n\n";

            if (lost)
                resultsText.text += $"You lost because: {reason}";
            else
                resultsText.text += "You successfully contained the fire and preserved community wellbeing!";
        }

        // if (lost)
        // {
        //     Debug.Log($"Game Over - {reason}");
        // }
        // else
        // {
        //     Debug.Log($"Simulation Complete: Fires contained. {burnedTiles.Count}/{totalTiles} tiles burned.");
        // }
    }

    bool CheckCulturalDamage()
    {
        foreach (var kv in map.tiles)
        {
            if (kv.Value.tileName.ToLower().Contains("indigenous_land") && kv.Value.burnt)
                return true;
        }
        return false;
    }

    bool CheckSatisfactionLoss(out float satisfaction)
    {
        float localSatisfaction = (townResources.provisions + townResources.happiness) / 2f;

        float totalMorale = 0f;
        int count = 0;
        foreach (var kv in Player.allPlayers)
        {
            totalMorale += kv.Value.resources.morale;
            count++;
        }

        float avgMorale = (count > 0) ? totalMorale / count : 50f;
        satisfaction = (localSatisfaction * 0.5f) + (avgMorale * 0.5f);

        Debug.Log($"Satisfaction Check → Local: {localSatisfaction:F1}, Morale: {avgMorale:F1}, Weighted: {satisfaction:F1}");
        return satisfaction < 20f;
    }


    public void Save()
    {   
        Debug.Log("FireSimulation complete, returning to ReflectionPhase.");
        SceneManager.LoadScene("ReflectionPhase");
    }
}
