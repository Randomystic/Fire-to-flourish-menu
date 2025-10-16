using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class MainMenu : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public GameObject menuImage;
    public GameObject menuButtons;
    public TownResourceList townResources;


    public void ExitButton()
    {
        Application.Quit();
        Debug.Log("Game Closed");
    }

    public void StartButton()
    {
        SceneManager.LoadScene("Narrative");
        Debug.Log("Game Started");
    }

    void Start()
    {
        townResources.ResetToDefaults();

        menuImage.SetActive(false);
        menuButtons.SetActive(false);
        videoPlayer.loopPointReached += VideoFinished;
    }

    void VideoFinished(VideoPlayer vidPlayer)
    {   
        Debug.Log("Image Switched");
        menuImage.SetActive(true);
        menuButtons.SetActive(true);
    }


}
