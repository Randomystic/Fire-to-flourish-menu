using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TownActionsDisplay : MonoBehaviour
{
    [Header("References")]

    public ActionCardInput actionCardInput; 
    public TownResourceList townResources;

    public TMP_Text actionsText;
    public TMP_Text resourceChangesText;

    private Dictionary<RoleType, string> selectedCards => actionCardInput.selectedCardsDict;
    private TownResourceList beforeChanges;

    void Start()
    {
        beforeChanges = ScriptableObject.CreateInstance<TownResourceList>();
        beforeChanges.provisions = townResources.provisions;
        beforeChanges.education = townResources.education;
        beforeChanges.population = townResources.population;
        beforeChanges.firefightingEquipment = townResources.firefightingEquipment;
        beforeChanges.fireSafetyRating = townResources.fireSafetyRating;

        DisplayActions();
        ApplyEffectsAndUpdate();
    }

    void DisplayActions()
    {
        actionsText.text = "";
        foreach (var entry in selectedCards)
        {
            string role = entry.Key.ToString().Replace("_", " ");
            string card = entry.Value.Replace("_", " ");
            actionsText.text += $"{role} played: {card}, effect: (see below)\n";
        }
    }

    void ApplyEffectsAndUpdate()
    {
        // For now, simulate random adjustments — replace later with your ActionCard data
        townResources.AdjustProvisions(Random.Range(-2, 4));
        townResources.AdjustEducation(Random.Range(0, 5));
        townResources.AdjustPopulation(Random.Range(-200, 300));
        townResources.AdjustFireFightingEquipment(Random.Range(0, 2));
        townResources.AdjustWindSpeed(Random.Range(-1, 2));
        townResources.AdjustTemperatureSeason(Random.Range(-1, 2));

        UpdateResourceText();
    }

    void UpdateResourceText()
    {
        resourceChangesText.text =
            $"Provisions: {beforeChanges.provisions} → {townResources.provisions}\n" +
            $"Education: {beforeChanges.education} → {townResources.education}\n" +
            $"Population: {beforeChanges.population} → {townResources.population}\n" +
            $"Firefighting Equipment: {beforeChanges.firefightingEquipment} → {townResources.firefightingEquipment}\n" +
            $"Fire Safety Rating: {beforeChanges.fireSafetyRating} → {townResources.fireSafetyRating}\n" +
            $"Wind Speed: {beforeChanges.windSpeed} → {townResources.windSpeed}\n" +
            $"Temperature: {beforeChanges.temperatureSeason} → {townResources.temperatureSeason}";
    }
}