using UnityEngine;

// wallmounted enemy. doesnt move. sees the player within range, spits blobs toward them.
// pairing this with an EnemyHealth so the player can destroy it by attacking. if not paired then just a turret 
public class BlobSpewer : MonoBehaviour
{
    [Header("=== Targeting ===")]
    [SerializeField] private Transform player;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private LayerMask lineOfSightBlockers; // leave 0 to skip line of sight check

    [Header("=== Firing ===")]
    [SerializeField] private EnemyProjectile blobPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 1.1f;          // average ish pace
    [SerializeField] private float firstShotDelay = 0.4f;
    [SerializeField] private float projectileSpeedOverride = -1f; // -1 = use prefab speed

    [Header("=== Animation ===")] //for eliaf :D [i hope i formated this right]
    [SerializeField] private Animator anim;
    [SerializeField] private string shootTrigger = "Shoot";

    [Header("=== Audio ===")]
    [SerializeField] private AudioSource spewerSource;
    [SerializeField] private AudioClip detectionSound;
    [SerializeField] private AudioClip fireSound;

    private float fireTimer;
    private bool playerIsDetected = false; // Tracks state so detection sound only plays ONCE

    private void Start()
    {
        fireTimer = firstShotDelay;

        // Optimization: Grab animator once at start instead of crushing performance in Update()
        if (anim == null) anim = GetComponent<Animator>(); 

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    private void Update()
    {
        if (player == null || blobPrefab == null || firePoint == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        
        // Player out of range
        if (dist > detectionRange) 
        {
            playerIsDetected = false; // Reset state when player leaves range
            return;
        }

        // optional line of sight only shoot if nothing is between us
        if (lineOfSightBlockers.value != 0)
        {
            Vector2 dirToPlayer = ((Vector2)player.position - (Vector2)firePoint.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(firePoint.position, dirToPlayer, dist, lineOfSightBlockers);
            if (hit.collider != null) 
            {
                playerIsDetected = false; // Treat lost sight line as out of range
                return;
            }
        }

        // TRIGGER DETECTION SOUND: Executed exactly once when the player steps into range
        if (!playerIsDetected)
        {
            playerIsDetected = true;
            if (spewerSource != null && detectionSound != null)
            {
                spewerSource.PlayOneShot(detectionSound);
            }
        }

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            Fire();
            fireTimer = fireRate;
        }
    }

    private void Fire()
    {
        Vector2 dir = ((Vector2)player.position - (Vector2)firePoint.position).normalized;

        EnemyProjectile proj = Instantiate(blobPrefab, firePoint.position, Quaternion.identity);
        proj.Launch(dir, projectileSpeedOverride);

        // TRIGGER FIRING SOUND
        if (spewerSource != null && fireSound != null)
        {
            spewerSource.PlayOneShot(fireSound);
        }

        if (anim != null) anim.SetTrigger(shootTrigger);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (firePoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(firePoint.position, 0.15f);
        }
    }
}