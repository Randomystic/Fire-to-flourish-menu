using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToDashboard : MonoBehaviour
{
    private static ReturnToDashboard instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            SceneManager.LoadScene("GameDashboard");
        }
    }
}
