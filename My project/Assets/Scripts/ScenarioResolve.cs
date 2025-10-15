using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScenarioResolve : MonoBehaviour
{

    public TownResourceList townResources;
    public TMP_Text scenarioText, dashboardText;

    private string scenarioDescription = "Resources Affected by Scenario";
    private int tempChange, windChange, equipChange;

    private TownResourceList beforeChanges;

    void Start()
    {
        scenarioText.text = scenarioDescription;

        beforeChanges = ScriptableObject.CreateInstance<TownResourceList>();
        beforeChanges.temperatureSeason = townResources.temperatureSeason;
        beforeChanges.firefightingEquipment = townResources.firefightingEquipment;
        beforeChanges.windSpeed = townResources.windSpeed;

        townResources.AdjustTemperatureSeason(tempChange);
        townResources.AdjustWindSpeed(windChange);
        townResources.AdjustFireFightingEquipment(equipChange);

        dashboardText.text =

            $"Temperature: {beforeChanges.temperatureSeason} -> {townResources.temperatureSeason}\n" +
            $"Wind Speed: {beforeChanges.windSpeed} -> {townResources.windSpeed}\n" +
            $"Firefighting Equipment: {beforeChanges.firefightingEquipment} -> {townResources.firefightingEquipment}";


        Debug.Log($"Applied Scenario -> Temp:{tempChange}, Wind:{windChange}, Equip:{equipChange}");
    }
}
