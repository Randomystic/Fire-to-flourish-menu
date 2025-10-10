using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelect : MonoBehaviour
{
    public Dictionary<int, string> selected = new Dictionary<int, string>();
    int index = 1;
    public GameObject continueButton;
    [SerializeField] private TextMeshProUGUI confirmationText;

    public void Select(GameObject button)
    {
        selected[index] = button.name;
        button.SetActive(false);
        Debug.Log(button.name);
        Debug.Log(index);
        if (index == 6) {
            ShowContinue();
        }
        index++;
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
    }

    public void ChangeScene()
    {
        SceneManager.LoadScene("Scenario");
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
