using UnityEngine;
// the game manager, for now it manages the check point system, but we can add more stuff to later like score, *because its a game arcade style) and time? maybe if we want to add different mode/ so if u wanna work on the code maybe add a note? and like here we can set the main values for other codes and stuff.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("=== Initial Spawn ===")]
    [SerializeField] private Transform respawnPoint;

    private Vector3 currentCheckpoint;
    private bool checkpointSet;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        currentCheckpoint = respawnPoint != null
            ? respawnPoint.position
            : new Vector3(-8f, -2.5f, 0f);
        checkpointSet = true;
    }

    public void SetCheckpoint(Vector3 position)
    {
        currentCheckpoint = position;
        checkpointSet = true;
        Debug.Log($"[GameManager] Checkpoint updated: {position}");
    }

    public Vector3 GetRespawnPosition()
    {
        return currentCheckpoint;
    }
}