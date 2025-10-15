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

    private Dictionary<RoleType, string> selectedCards => ActionCardInput.lastSelections;
    
    private TownResourceList beforeChanges;

    void Start()
    {
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
        beforeChanges.population = townResources.population;
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

            //Apply card effects to player and town
            if (Player.allPlayers.TryGetValue(role, out var player))
            {
                player.UseActionCard(card);
            }

            ApplyTownEffects(card.effects);
        
        }
    }

    void ApplyPlayerEffects(Player player, List<ResourceEffect> effects)
    {
        foreach (var e in effects)
        {
            switch (e.resourceName.ToLower())
            {
                case "money": player.resources.AdjustMoney(e.value); break;
                case "morale": player.resources.AdjustMorale(e.value); break;
                case "respect": player.resources.AdjustRespect(e.value); break;
            }
        }
    }

    void ApplyTownEffects(List<ResourceEffect> effects)
    {
        foreach (var e in effects)
        {
            switch (e.resourceName.ToLower())
            {
                case "provisions": townResources.AdjustProvisions(e.value); break;
                case "education": townResources.AdjustEducation(e.value); break;
                case "population": townResources.AdjustPopulation(e.value); break;
                case "firefighting_equipment": townResources.AdjustFireFightingEquipment(e.value); break;
                case "fire_safety_rating": townResources.fireSafetyRating += e.value; break;
                case "wind_speed": townResources.AdjustWindSpeed(e.value); break;
                case "temperature_season": townResources.AdjustTemperatureSeason(e.value); break;
            }
        }
    }


    void DisplayResourceUpdates()
    {
        resourceChangesText.text =
        $"Provisions: {beforeChanges.provisions} -> {townResources.provisions}\n" +
        $"Education: {beforeChanges.education} -> {townResources.education}\n" +
        $"Population: {beforeChanges.population} -> {townResources.population}\n" +
        $"Firefighting Equipment: {beforeChanges.firefightingEquipment} -> {townResources.firefightingEquipment}\n" +
        $"Fire Safety Rating: {beforeChanges.fireSafetyRating} -> {townResources.fireSafetyRating}\n" +
        $"Wind Speed: {beforeChanges.windSpeed} -> {townResources.windSpeed}\n" +
        $"Temperature Season: {beforeChanges.temperatureSeason} -> {townResources.temperatureSeason}";
    }

     void Save()
    {
        SceneManager.LoadScene("TestScene");
    }
}
