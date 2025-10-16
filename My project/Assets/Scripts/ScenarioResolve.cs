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
        var scenario = Scenario.Instance;
        if (!scenario)
        {
            Debug.LogError("ScenarioResolve: no Scenario instance (did you open this scene directly?)");
            scenarioText.text = "Error: No scenario data found. Did you open this scene directly?";
            dashboardText.text = "";
            return;
        }

        // Use the index the player was on, instead of last element
        int i = Mathf.Clamp(scenario.index, 0, scenario.scenarios.Count - 1);
        var data = scenario.scenarios[i];

        if (scenarioText)
            scenarioText.text = $"{data.name}\n{data.effect}\n{data.description}";

        var before = new Dictionary<string, int>
        {
            {"temperatureSeason", townResources.temperatureSeason},
            {"windSpeed", townResources.windSpeed},
            {"firefightingEquipment", townResources.firefightingEquipment},
            {"education", townResources.education},
            {"happiness", townResources.happiness},
        };

        if (dashboardText) dashboardText.text = "";
        foreach (var kv in data.resources)
        {
            int oldVal = before.ContainsKey(kv.Key) ? before[kv.Key] : 0;

            switch (kv.Key)
            {
                case "temperatureSeason": townResources.AdjustTemperatureSeason(kv.Value); break;
                case "windSpeed": townResources.AdjustWindSpeed(kv.Value); break;
                case "firefightingEquipment": townResources.AdjustFireFightingEquipment(kv.Value); break;
                case "education": townResources.AdjustEducation(kv.Value); break;
                case "happiness": townResources.AdjustHappiness(kv.Value); break;
                default:
                    Debug.LogWarning($"ScenarioResolve: unknown resource key '{kv.Key}'");
                    break;
            }

            int newVal = oldVal + kv.Value;
            if (dashboardText) dashboardText.text += $"{kv.Key}: {oldVal} -> {newVal}\n";
        }

        ResourceDashboard.RecalculateFireSafety(Map.Instance, townResources);
    }

    public void Save() {
        if (Scenario.Instance != null)
        Scenario.Instance.NextScenario();

        SceneManager.LoadScene("GameDashboard");
    }
}
