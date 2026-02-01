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

    private void Start()
    {
        menuImage.SetActive(false);
        menuButtons.SetActive(false);
        videoPlayer.loopPointReached += VideoFinished;
    }

    private void VideoFinished(VideoPlayer vidPlayer)
    {   
        Debug.Log("Image Switched");
        menuImage.SetActive(true);
        menuButtons.SetActive(true);
    }

}
