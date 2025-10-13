using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    //Singleton
    public static SceneLoader SceneInstance {get; private set;}
    private string currentScene;

    private void Awake()
    {
        if (SceneInstance != null && SceneInstance != this)
        {
            Destroy(gameObject);
            return;
        }

        SceneInstance = this;
        DontDestroyOnLoad(gameObject);
        currentScene = SceneManager.GetActiveScene().name;
    }

    // Scene switching
    public void SwitchToScene(string newScene)
    {
        if (Application.CanStreamedLevelBeLoaded(newScene))
        {
            SceneManager.LoadScene(newScene);
            currentScene = newScene;
            Debug.Log($"Switched to {newScene}");
        }
        else
        {
            Debug.LogError($"Scene '{newScene}' not found in Build Settings!");
        }
    }
    public string GetCurrentScene() => currentScene;


    
}