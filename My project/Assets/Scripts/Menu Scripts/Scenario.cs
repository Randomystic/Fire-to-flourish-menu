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
                description = "An extended period of oppressive heat...",
                resources = new Dictionary<string, int>
                {
                    { "temperatureSeason", 1 },
                    { "morale", -2 }
                }
            });

            scenarios.Add(new ScenarioData
            {
                name = "Severe Wind Surge",
                effect = "+1 Wind Speed, +1 Fuel Load",
                description = "Unexpectedly strong winds roar through the region...",
                resources = new Dictionary<string, int>
                {
                    { "windSpeed", 1 },
                    { "fuelLoad", 1 }
                }
            });

            scenarios.Add(new ScenarioData
            {
                name = "Government Budget Freeze",
                effect = "−2 Equipment, −5 Education, +1 Fuel Load",
                description = "Due to unexpected budget tightening...",
                resources = new Dictionary<string, int>
                {
                    { "firefightingEquipment", -2 },
                    { "education", -5 },
                    { "fuelLoad", 1 }
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
        SceneManager.LoadScene("ScenarioResolve");
    }

}
