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
}

public class Scenario : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text effectText;
    public TMP_Text descriptionText;
    public Button nextButton;

    public List<ScenarioData> scenarios = new List<ScenarioData>();

    Dictionary<string, ScenarioData> scenarioDict = new Dictionary<string, ScenarioData>();
    int index;

    void Awake()
    {
        if (scenarios.Count == 0)
        {
            scenarios.Add(new ScenarioData 
            { 
                name = "Sustained Heatwave", 
                effect = "+1 Temperature (all tiles), -2 Morale across the town", 
                description = "An extended period of oppressive heat pushes both people and the environment to their limits. Vegetation dries rapidly, drastically increasing fuel availability. Residents struggle with heat exhaustion and irritability, making coordinated efforts more difficult." 
            });

            scenarios.Add(new ScenarioData 
            { 
                name = "Severe Wind Surge", 
                effect = "+1 Wind Speed, +1 Fuel Load on exposed tiles (Forest/Grassland)", 
                description = "Unexpectedly strong winds roar through the region, whipping up loose debris and drying out already stressed landscapes. Efforts to clear land or build breaks face setbacks as vegetation is scattered and boundary lines eroded." 
            });

            scenarios.Add(new ScenarioData 
            { 
                name = "Government Budget Freeze", 
                effect = "−2 Firefighting Equipment, −5 Education, +1 Fuel Load across tiles", 
                description = "Due to unexpected budget tightening at the state level, existing grants and support programs have been suspended. Planned fire safety upgrades are delayed, training sessions are cancelled. The town must now operate with fewer resources, just when resilience efforts matter." 
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
        SceneManager.LoadScene("TestScene");
    }

}
