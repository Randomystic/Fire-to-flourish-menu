using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class NarrativeScene : MonoBehaviour
{

    public GameObject continueButton;

    public VideoPlayer videoPlayer;
    public VideoClip[] videos;

    int currentIndex = 0;
    bool waiting = false;


    // Start is called before the first frame update
    void Start()
    {
        // ShowLine(currentIndex);
        
        videoPlayer.clip = videos[currentIndex];
        videoPlayer.isLooping = false;
        videoPlayer.loopPointReached += OnEnd;
        videoPlayer.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            NextVideo();
        }
    }

    void OnEnd(VideoPlayer vp)
    {
        videoPlayer.Pause();
        waiting = true;
    }


    public void NextVideo()
    {
        if (currentIndex + 1 >= videos.Length) {
            SceneManager.LoadScene("GameDashboard");
            currentIndex = 1;
        }

        currentIndex++;
        waiting = false;
        videoPlayer.clip = videos[currentIndex];
        videoPlayer.Play();

    }

}