using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class TownActionsDisplay : MonoBehaviour
{
    [Header("References")]
    public ActionCardInput actionCardInput;
    public TownResourceList townResources;
    public TMP_Text actionsText;
    public TMP_Text resourceChangesText;
    public Button saveButton;

    public Map map;

    private Dictionary<RoleType, string> selectedCards => ActionCardInput.lastSelections;
    
    private TownResourceList beforeChanges;

    void Start()
    {
        map = Map.Instance;                    // ignore any serialized ref in Inspector
        if (!map) { Debug.LogError("TownActionsDisplay: No Map instance found."); return; }
        map.EnsureInitialized();
        Debug.Log($"TownActionsDisplay: Map tiles = {map.tiles.Count} (InstanceID {map.GetInstanceID()})");

        CacheStartingTownValues();
        DisplayActions();
        ApplyCardEffects();
        DisplayResourceUpdates();
    }


    void CacheStartingTownValues()
    {
        beforeChanges = ScriptableObject.CreateInstance<TownResourceList>();
        beforeChanges.provisions = townResources.provisions;
        beforeChanges.education = townResources.education;
        // beforeChanges.population = townResources.population;
        beforeChanges.happiness = townResources.happiness;
        beforeChanges.firefightingEquipment = townResources.firefightingEquipment;
        beforeChanges.fireSafetyRating = townResources.fireSafetyRating;
    }

    void DisplayActions()
    {
        actionsText.text = "";
        foreach (var entry in selectedCards)
        {
            string role = entry.Key.ToString().Replace("_", " ");
            string cardName = entry.Value.Replace("_", " ");
            actionsText.text += $"{role} played: {cardName}\n";
        }
    }

    void ApplyCardEffects()
    {
        foreach (var entry in selectedCards)
        {
            RoleType role = entry.Key;
            string cardName = entry.Value;

            //Load the ActionCard Scriptable Object by name
            var card = Resources.Load<ActionCardData>($"Cards/{cardName}");
            if (card == null)
            {
                Debug.LogWarning($"Card not found: {cardName}");
                continue;
            }

            foreach (var p in Player.allPlayers)
                Debug.Log($"Registered Player: {p.Key} -> {p.Value.playerName}");

            // Apply player + tile effects
            if (Player.allPlayers.TryGetValue(role, out var player))
                ApplyPlayerEffects(player, card);

            // Apply town effects
            ApplyTownEffects(card.effects);

        
        }
    }

    void ApplyPlayerEffects(Player player, ActionCardData card)
    {
        // User player applies card effects to self
        player.UseActionCard(card);
        Debug.Log($"Applied effects of {card.cardName} to player {player.playerName}");

    }


    void ApplyTownEffects(List<ResourceEffect> effects)
    {
        foreach (var e in effects)
        {
            string key = e.resourceName.Replace("_", "").ToLower();
            switch (key)
            {
                case "provisions": townResources.AdjustProvisions(e.value); break;
                case "education": townResources.AdjustEducation(e.value); break;
                // case "population": townResources.AdjustPopulation(e.value); break;
                case "happiness": townResources.AdjustHappiness(e.value); break;
                case "firefightingequipment": townResources.AdjustFireFightingEquipment(e.value); break;
                case "firesafetyrating": townResources.fireSafetyRating += e.value; break;
                case "windspeed": townResources.AdjustWindSpeed(e.value); break;
                case "temperatureseason": townResources.AdjustTemperatureSeason(e.value); break;
            }
        }
    }


    void DisplayResourceUpdates()
    {
        ResourceDashboard.RecalculateFireSafety(Map.Instance, townResources);

        resourceChangesText.text =
        $"Provisions: {beforeChanges.provisions} -> {townResources.provisions}\n" +
        $"Education: {beforeChanges.education} -> {townResources.education}\n" +
        // $"Population: {beforeChanges.population} -> {townResources.population}\n" +
        $"Happiness: {beforeChanges.happiness} -> {townResources.happiness}\n" +
        $"Firefighting Equipment: {beforeChanges.firefightingEquipment} -> {townResources.firefightingEquipment}\n" +
        // $"Fire Safety Rating: {beforeChanges.fireSafetyRating} -> {townResources.fireSafetyRating}\n" +
        $"Wind Speed: {beforeChanges.windSpeed} -> {townResources.windSpeed}\n" +
        $"Temperature Season: {beforeChanges.temperatureSeason} -> {townResources.temperatureSeason}\n";
        
        resourceChangesText.text +=
        $"Updated Fire Safety Rating → {townResources.fireSafetyRating} (avg fuel {townResources.averageFuelLoad:F1})\n";;
        
        // TownActionsDisplay.cs — inside DisplayResourceUpdates()
        if (ActionCardInput.updatedTiles != null && ActionCardInput.updatedTiles.Count > 0)
        {
            foreach (var entry in ActionCardInput.updatedTiles)
            {
                var coord = entry.Key;
                var msg = entry.Value; // "Input Accepted → ..."

                if (map && map.tiles.TryGetValue(coord, out var tile))
                {
                    resourceChangesText.text +=
                        $"{msg}\n" +
                        $"→ Current FuelLoad: {tile.fuelLoad}\n";
                }
                else
                {
                    resourceChangesText.text += $"{msg}\n(Warning: tile not found in current Map)\n";
                }
            }
        }
        else
        {
            resourceChangesText.text += "No manual tile updates this round.\n";
        }

    }

    public void Save()
    {
        ActionCardInput.updatedTiles.Clear();
        SceneManager.LoadScene("DiscussionPhase");
    }
}
