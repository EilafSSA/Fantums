using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class IntroManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private VideoPlayer videoPlayer;
    
    [Header("Scene Transition Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // Match your scene name exactly
    [SerializeField] private float extraBlackScreenTime = 4f;

    void Start()
    {
        // Subscribe to Unity's built-in event that fires when the video finishes playing
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    void OnVideoFinished(VideoPlayer source)
    {
        // Unsubscribe to prevent any double-firing bugs
        videoPlayer.loopPointReached -= OnVideoFinished;
        
        // Start the delayed transition routine
        StartCoroutine(WaitAndTransition());
    }

    private IEnumerator WaitAndTransition()
    {
        // 1. Hide the video player output so the screen falls back to the black background
        videoPlayer.enabled = false;

        // 2. Wait for your remaining audio to finish playing (4 seconds)
        yield return new WaitForSeconds(extraBlackScreenTime);

        // 3. Load the Main Menu
        SceneManager.LoadScene(mainMenuSceneName);
    }
}