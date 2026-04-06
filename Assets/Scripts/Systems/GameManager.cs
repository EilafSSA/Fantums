using UnityEngine;

// the game manager - handles checkpoints, score, and can grow into time/modes later.
// to add score from anywhere: GameManager.Instance.AddScore(points)
// to read score: GameManager.Instance.GetScore()
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("=== Initial Spawn ===")]
    [SerializeField] private Transform respawnPoint;

    [Header("=== Score ===")]
    [SerializeField] private int scorePerEnemy = 100;  // tweak per enemy type if needed later

    private Vector3 currentCheckpoint;
    private bool checkpointSet;
    private int score;

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
        score = 0;
    }

    // --- Checkpoint ---

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


    public void AddScore(int points)
    {
        score += points;
        Debug.Log($"[GameManager] Score: {score}");
    }

    public void AddEnemyKillScore()
    {
        AddScore(scorePerEnemy);
    }

    public int GetScore() => score;

    public void ResetScore()
    {
        score = 0;
    }
}