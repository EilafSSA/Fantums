using UnityEngine;

public class LevelMusicInitializer : MonoBehaviour
{
    [SerializeField] private AudioClip level2Music;

    private void Start()
    {
        if (AudioManager.Instance != null && level2Music != null)
        {
            // Force the switch immediately on start
            AudioManager.Instance.SwitchMusic(level2Music, 0.5f, 1.0f);
        }
        else
        {
            Debug.LogError("Music Init Failed: Manager or Clip is missing!");
        }
    }
}