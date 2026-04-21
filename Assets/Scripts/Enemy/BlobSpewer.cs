using UnityEngine;

// wallmounted enemy. doesnt move. sees the player within range, spits blobs toward them.
// pairing this with an EnemyHealth so the player can destroy it by attacking. if not paired then just a turret 
public class BlobSpewer : MonoBehaviour
{
    [Header("=== Targeting ===")]
    [SerializeField] private Transform player;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private LayerMask lineOfSightBlockers; // leave 0 to skip LoS check

    [Header("=== Firing ===")]
    [SerializeField] private EnemyProjectile blobPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 1.1f;          // average ish pace
    [SerializeField] private float firstShotDelay = 0.4f;
    [SerializeField] private float projectileSpeedOverride = -1f; // -1 = use prefab speed

    [Header("=== Animation ===")] //for eliaf :D [i hope i formated this right]
    [SerializeField] private Animator anim;
    [SerializeField] private string shootTrigger = "Shoot";

    private float fireTimer;

    private void Start()
    {
        fireTimer = firstShotDelay;

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
        if (dist > detectionRange) return;

        // optional line ofsight only shoot if nothing is between us
        if (lineOfSightBlockers.value != 0)
        {
            Vector2 dirToPlayer = ((Vector2)player.position - (Vector2)firePoint.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(firePoint.position, dirToPlayer, dist, lineOfSightBlockers);
            if (hit.collider != null) return;
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