using UnityEngine;
// checkpoint system for checkpoints, for the player to respawn at one place anad also give viusual feed back to player they acivated the checkpoint but that's temporary because we can or will add flag that goes up or down when activated.
public class Checkpoint : MonoBehaviour
{
    [Header("=== Settings ===")]
    [Tooltip("Where the player respawns (if empty, uses this object's position)")]
    [SerializeField] private Transform respawnOverride;

    [Header("=== Visual Feedback ===")]
    [SerializeField] private Color activatedColor = Color.green;

    private SpriteRenderer sr;
    private bool activated;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;

        if (!other.CompareTag("Player")) return;

        activated = true;

        Vector3 spawnPos;

        if (respawnOverride != null)
        {
            spawnPos = respawnOverride.position;
        }
        else
        {
            spawnPos = transform.position;
        }

        GameManager.Instance.SetCheckpoint(spawnPos);

        if (sr != null)
            sr.color = activatedColor;

        Debug.Log($"[Checkpoint] Activated: {gameObject.name}");
    }
}