using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ScenarioResolve : MonoBehaviour
{
    public TownResourceList townResources;
    public TMP_Text scenarioText, dashboardText;

    void Start()
    {
        var scenario = FindObjectOfType<Scenario>();

        if (!scenario) return;

        var data = scenario.scenarios[Mathf.Clamp(scenario.scenarios.Count - 1, 0, scenario.scenarios.Count - 1)];

        scenarioText.text = $"{data.name}\n{data.effect}\n{data.description}";

        var before = new Dictionary<string, int>
        {

            { "temperatureSeason", townResources.temperatureSeason },
            { "windSpeed", townResources.windSpeed },
            { "firefightingEquipment", townResources.firefightingEquipment },
            { "education", townResources.education },
            { "fuelLoad", 0 }, // tile-based, tracked separately
        };

        dashboardText.text = "";



        foreach (var kv in data.resources)
        {
            int oldVal = before.ContainsKey(kv.Key) ? before[kv.Key] : 0;

            switch (kv.Key)
            {

                case "temperatureSeason": townResources.AdjustTemperatureSeason(kv.Value); break;
                case "windSpeed": townResources.AdjustWindSpeed(kv.Value); break;
                case "firefightingEquipment": townResources.AdjustFireFightingEquipment(kv.Value); break;
                case "education": townResources.AdjustEducation(kv.Value); break;
                case "morale": Debug.Log($"Morale changed by {kv.Value} (applies to players)"); break;
                case "fuelLoad": Debug.Log($"Fuel load changed by {kv.Value} (applies to random tile)"); break;
            }

            int newVal = before.ContainsKey(kv.Key) ? oldVal + kv.Value : kv.Value;

            dashboardText.text += $"{kv.Key}: {oldVal} -> {newVal}\n";
        }
    }

    public void Save() {
        SceneManager.LoadScene("GameDashboard");
    }
}
