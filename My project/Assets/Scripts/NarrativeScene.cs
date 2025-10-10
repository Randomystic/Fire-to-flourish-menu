using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class NarrativeScene : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dialogueText;

    int currentIndex = 1;

    // Dictionary for the diagloue
    private Dictionary<int, string> dialogueLines = new Dictionary<int, string>()
    {
        { 1, "Narrative Dialogue" },
        { 2, "Dialogue 1" },
        { 3, "Dialogue 2" },
        { 4, "Dialogue 3" },
    };

    // Start is called before the first frame update
    void Start()
    {
        ShowLine(currentIndex);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShowNextLine();
        }
    }

    public void ShowNextLine()
    {
        currentIndex++;

        if (currentIndex > dialogueLines.Count)
        {
            // //Loops back to 1
            // currentIndex = 1;
            SceneManager.LoadScene("TestScene");
            
        }
        ShowLine(currentIndex);
    }

    // Gets the index number of dictionary, displays corresponding text
    void ShowLine(int index)
    {
        if (dialogueLines.ContainsKey(index))
            dialogueText.text = dialogueLines[index];
        else
            dialogueText.text = "[Missing Line]";
            Debug.Log("Dialogue Line missing!");
    }

}
