using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class NarrativeScene : MonoBehaviour
{
    // [SerializeField] private TextMeshProUGUI dialogueText;
    public GameObject continueButton;

    public VideoPlayer videoPlayer;
    public VideoClip[] videos;

    private int currentIndex = 0;
    private bool waiting;

    // // Dictionary for the diagloue
    // private Dictionary<int, string> dialogueLines = new Dictionary<int, string>()
    // {
    //     { 1, "Narrative Dialogue" },
    //     { 2, "Dialogue 1" },
    //     { 3, "Dialogue 2" },
    //     { 4, "Dialogue 3" },
    // };

    // Start is called before the first frame update
    private void Start()
    {
        // ShowLine(currentIndex);
        
        videoPlayer.clip = videos[currentIndex];
        videoPlayer.isLooping = false;
        videoPlayer.loopPointReached += OnEnd;
        videoPlayer.Play();
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            NextVideo();
        }
    }

    private void OnEnd(VideoPlayer vp)
    {
        videoPlayer.Pause();
        waiting = true;
    }


    public void NextVideo()
    {
        if (currentIndex + 1 >= videos.Length) {
            SceneManager.LoadScene("CharacterSelect");
            currentIndex = 1;
        }

        currentIndex++;
        waiting = false;
        videoPlayer.clip = videos[currentIndex];
        videoPlayer.Play();

    }

    // public void ShowNextLine()
    // {
    //     currentIndex++;

    //     Debug.Log($"{currentIndex} / {dialogueLines.Count}");

    //     if (currentIndex > dialogueLines.Count)
    //     {
    //         // //Loops back to 1
    //         // currentIndex = 1;
    //         SceneManager.LoadScene("CharacterSelect");
            
    //     }
    //     ShowLine(currentIndex);
    // }

    // // Gets the index number of dictionary, displays corresponding text
    // void ShowLine(int index)
    // {   
    //     print(index);
    //     if (dialogueLines.ContainsKey(index))
    //         dialogueText.text = dialogueLines[index];
    //     else
    //         dialogueText.text = "[Missing Line]";
    //         Debug.Log("Dialogue Line missing!");
    // }

}
