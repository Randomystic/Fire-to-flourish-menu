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
    
    public static Scenario Instance;
    public int index;


    void Awake()
    {
        if (Instance != null && Instance != this) {Destroy(gameObject); return;}
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (scenarios.Count == 0)
        {
            scenarios.Add(new ScenarioData
            {
                name = "Sustained Heatwave",
                effect = "+1 Temperature, -5 Happiness",
                description = "An extended period of oppressive heat grips the region. causing widespread fatigue and discomfort. Community happiness drops as residents struggle to stay cool and conserve resources.",
                resources = new Dictionary<string, int>
                {
                    { "temperatureSeason", 1 },
                    { "happiness", -5 }
                }
            });

            scenarios.Add(new ScenarioData
            {
                name = "Severe Wind Surge",
                effect = "+1 Wind Speed",
                description = "Unexpectedly strong winds roar through the area. The rising wind speed heightens fire risk, Towns must act quickly before conditions worsen.",
                resources = new Dictionary<string, int>
                {
                    {"windSpeed", 1},
                }
            });

            scenarios.Add(new ScenarioData
            {
                name = "Government Budget Freeze",
                effect = "−1 Equipment, −5 Education",
                description = "Due to an unexpected freeze in regional disaster prevention funding, critical training programs and resource allocations are delayed. Schools and community workshops lose funding, reducing public preparedness.",
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
        foreach (var canvas in GetComponentsInChildren<Canvas>(true))
        canvas.gameObject.SetActive(false);

        SceneManager.LoadScene("ScenarioResolve");
    }

}
