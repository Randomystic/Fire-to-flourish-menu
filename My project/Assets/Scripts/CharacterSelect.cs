using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelect : MonoBehaviour
{
    public Dictionary<int, string> selected = new Dictionary<int, string>();
    int index = 1;
    public GameObject continueButton;
    // public GameObject randomizeButton;
    [SerializeField] private TextMeshProUGUI confirmationText;
    [SerializeField] GameObject charactersPanel;
    [SerializeField] GameObject confirmationPanel;

    public void Select(GameObject button)
    {
        selected[index] = button.name;
        button.SetActive(false);
        if (index == 6) {
            ShowContinue();
        }
        index++;
    }

    public void RandomizeCharacter()
    {
        var parent = charactersPanel.transform;

        var names = new List<string>();
        foreach (var button in parent.GetComponentsInChildren<Button>(true))
            names.Add(button.name);

        var random = new System.Random();
        for (int x = names.Count - 1; x > 0; x--)
        {
            int y = random.Next(0, x + 1);
            (names[x], names[y]) = (names[y], names[x]);
        }

        selected.Clear();
        index = 1;
        
        for (int i = 1; i <= names.Count; i++)
            selected[i] = names[i - 1];
        
        ShowContinue();
    }

    public void ShowCharacterCards()
    {
        selected.Clear();
        index = 1;

        foreach (var button in charactersPanel.GetComponentsInChildren<Button>(true))
            button.gameObject.SetActive(true);

        confirmationPanel.SetActive(false);
        charactersPanel.SetActive(true);
    }


    void ShowContinue()
    {
        continueButton.SetActive(true);
        string str = "Player Characters:\n\n";

        for (int i = 1; i <= selected.Count; i++) 
        {
            str += $"Player {i}: {selected[i]}\n";
        }
        str += "\nIs this correct?";
        confirmationText.text = str;

        charactersPanel.SetActive(false);
        confirmationPanel.SetActive(true);
    }

    public void ChangeScene()
    {
        SceneManager.LoadScene("Scenario");
    }

    // Start is called before the first frame update
    void Start()
    {
        continueButton.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
