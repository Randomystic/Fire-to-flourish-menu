using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurnManagement : MonoBehaviour
{

    public Button endTurnButton;
    public TextMeshProUGUI turnText;

    public int currentTurn = 1;
    // Start is called before the first frame update
    void Start()
    {
        currentTurn = 1;
        turnText.text = "Current Turn: " + currentTurn.ToString();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void EndTurnButtonClicked()
    {
        Debug.Log("End Turn Button clicked!");
        currentTurn++;
        turnText.text = "Current Turn: " + currentTurn.ToString();


    }
}
