using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scenario : MonoBehaviour
{
    public void ChangeScene()
    {
        SceneManager.LoadScene("TestScene");
    }
}
