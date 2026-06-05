using UnityEngine;

public class MusicTrigger : MonoBehaviour
{
    [SerializeField] private AudioClip levelMusic;
    [SerializeField] private AudioClip bossMusic;

    private void Start()
    {
        // Set the background music at the start
        if (levelMusic != null) AudioManager.Instance.PlayBossMusic(levelMusic, 0f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && bossMusic != null)
        {
            AudioManager.Instance.PlayBossMusic(bossMusic, 1f); // Fade in boss music
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && levelMusic != null)
        {
            AudioManager.Instance.PlayBossMusic(levelMusic, 1.5f); // Fade back to level music
        }
    }
}