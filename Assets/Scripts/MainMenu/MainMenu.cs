using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Audio;

public class MainMenu : MonoBehaviour
{

    [Header("=== Options ===")]
    [SerializeField] private OptionsMenu optionsMenu;
    [SerializeField] private GameObject creditsPanel; // Drag your Credits panel here if it's a GameObject

    public AudioMixer audioMixer;

    private float lastPlayedVolume = -1f;

    // Call this when the final launch button is pressed to load the level
    public void PlayGame()
    {
        StartCoroutine(PlayGameRoutine());
    }

    private IEnumerator PlayGameRoutine()
    {
        if (UIAudioManager.Instance != null)
        {
            UIAudioManager.Instance.PlayClick();
            
            // Tell the music track to smoothly fade out over 0.25 seconds 
            // so it blends seamlessly into Level 1's atmosphere
            UIAudioManager.Instance.FadeOutMenuMusic(0.25f);
        }

        yield return new WaitForSecondsRealtime(0.25f);
        SceneManager.LoadSceneAsync(1); // Level1
    }

    // GENERIC CLICK: Use this for buttons that just open sub-menus (like your new START sub-menu)
    public void PlayMenuClickSound()
    {
        if (UIAudioManager.Instance != null)
        {
            UIAudioManager.Instance.PlayClick();
        }
    }

    public void QuitGame()
    {
        if (UIAudioManager.Instance != null)
        {
            UIAudioManager.Instance.PlayClick();
        }

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    public void OpenOptions()
    {
        if (UIAudioManager.Instance != null)
        {
            UIAudioManager.Instance.PlayClick();
        }

        if (optionsMenu != null) optionsMenu.OpenOptions();
    }

    public void CloseOptions()
    {
        if (UIAudioManager.Instance != null)
        {
            UIAudioManager.Instance.PlayCancel();
        }

        if (optionsMenu != null) optionsMenu.CloseOptions();
    }

    // CREDITS METHODS
    public void OpenCredits()
    {
        if (UIAudioManager.Instance != null)
        {
            UIAudioManager.Instance.PlayClick();
        }

        if (creditsPanel != null) creditsPanel.SetActive(true);
    }

    public void CloseCredits()
    {
        if (UIAudioManager.Instance != null)
        {
            UIAudioManager.Instance.PlayCancel();
        }

        if (creditsPanel != null) creditsPanel.SetActive(false);
    }
    //here
    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("MasterAudio", volume);
        audioMixer.SetFloat("MusicAudio", volume);
        audioMixer.SetFloat("SFXAudio", volume);
        if (Mathf.Abs(volume - lastPlayedVolume) >= 0.15f)
        {
            if (UIAudioManager.Instance != null)
            {
                UIAudioManager.Instance.PlayHover();
            }
            lastPlayedVolume = volume;
        }
    }
}