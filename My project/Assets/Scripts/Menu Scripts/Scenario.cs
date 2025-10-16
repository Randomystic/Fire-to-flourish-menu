using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class ScenarioData
{
    public string name;
    public string effect;
    public string description;
    public Dictionary<string, int> resources = new(); // must be initialized
}

public class Scenario : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text effectText;
    public TMP_Text descriptionText;
    public Button nextButton;

    public List<ScenarioData> scenarios = new List<ScenarioData>();

    public Dictionary<string, ScenarioData> scenarioDict = new Dictionary<string, ScenarioData>();
    int index;

    void Awake()
    {
        if (scenarios.Count == 0)
        {
            scenarios.Add(new ScenarioData
            {
                name = "Sustained Heatwave",
                effect = "+1 Temperature, -2 Morale",
                description = "An extended period of oppressive heat grips the region. The relentless sun pushes temperatures well above seasonal averages, causing widespread fatigue and discomfort. Crops wilt, tempers rise, and community morale begins to waver as residents struggle to stay cool and conserve resources.",
                resources = new Dictionary<string, int>
                {
                    { "temperatureSeason", 1 },
                    { "morale", -2 }
                }
            });

            scenarios.Add(new ScenarioData
            {
                name = "Severe Wind Surge",
                effect = "+1 Wind Speed, +1 Fuel Load for random tile",
                description = "Unexpectedly strong winds roar through the area, scattering dry debris and rapidly fanning vegetation. The rising wind speed heightens fire risk, drying out the landscape and fueling potential ignition sources. Towns must act quickly to manage fuel buildup before conditions worsen.",
                resources = new Dictionary<string, int>
                {
                    {"windSpeed", 1},
                    {"fuelLoad", 1}
                }
            });

            scenarios.Add(new ScenarioData
            {
                name = "Government Budget Freeze",
                effect = "−1 Equipment, −5 Education",
                description = "Due to an unexpected freeze in regional disaster-prevention funding, critical training programs and resource allocations are delayed. Schools and community workshops lose funding, reducing public preparedness. Maintenance crews are limited, allowing vegetation and debris to accumulate dangerously across key areas.",
                resources = new Dictionary<string, int>
                {
                    {"firefightingEquipment", -1},
                    {"education", -5},
                }
            });
        }


        scenarioDict.Clear();
        foreach (var s in scenarios) if (!scenarioDict.ContainsKey(s.name)) scenarioDict.Add(s.name, s);

        if (nextButton) nextButton.onClick.AddListener(NextScenario);
        Show(index);
    }


    void Show(int i)
    {
        if (scenarios.Count == 0) return;
        var s = scenarios[Mathf.Abs(i) % scenarios.Count];
        if (titleText) titleText.text = s.name;
        if (effectText) effectText.text = s.effect;
        if (descriptionText) descriptionText.text = s.description;
    }



    public void NextScenario()
    {
        if (scenarios.Count == 0) return;
        index = (index + 1) % scenarios.Count;
        Show(index);
    }



    public void ChangeScene()
    {
        NextScenario();
        SceneManager.LoadScene("ScenarioResolve");
    }

}
