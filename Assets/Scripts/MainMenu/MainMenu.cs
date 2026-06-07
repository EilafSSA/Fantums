using UnityEngine;
using UnityEngine.Audio; // <-- REQUIRED FOR AUDIO MIXER ARCHITECTURE
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("=== Audio Mixing ===")]
    [SerializeField] private AudioMixer mainMixer; // Drag your MainMixer asset here in the inspector

    [Header("=== Options ===")]
    [SerializeField] private OptionsMenu optionsMenu;
    [SerializeField] private GameObject creditsPanel; // Drag your Credits panel here if it's a GameObject

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

    public void SetVolume(float volume)
    {
        if (mainMixer == null)
        {
            Debug.LogWarning("MainMixer reference missing on MainMenu script component!");
            return;
        }

        // Convert the linear slider scale (0.0001 to 1) to a proper logarithmic decibel value (-80dB to 0dB)
        // We clamp the minimum value slightly above 0 to completely prevent mathematical log(0) calculation crashes
        float clampedVolume = Mathf.Clamp(volume, 0.0001f, 1f);
        float decibels = Mathf.Log10(clampedVolume) * 20f;

        // Drive the exposed parameter inside the MainMixer asset structure
        mainMixer.SetFloat("MasterVolume", decibels);

        // Optional: Play a soft UI tick noise when dragging the slider bar handle past 15% delta steps
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