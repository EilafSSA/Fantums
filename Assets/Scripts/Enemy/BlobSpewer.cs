using UnityEngine;

// Wall-mounted enemy. Remains static. Detects the player within range and launches projectile blobs toward them.
// Can be paired with an EnemyHealth component for destructibility, or left standalone to function as an immutable turret.
public class BlobSpewer : MonoBehaviour
{
    [Header("=== Targeting ===")]
    [SerializeField] private Transform player;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private LayerMask lineOfSightBlockers; // Set to 0/Nothing to bypass line of sight verification

    [Header("=== Firing ===")]
    [SerializeField] private EnemyProjectile blobPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 1.1f;               // Delay cadence between subsequent shots
    [SerializeField] private float firstShotDelay = 0.4f;
    [SerializeField] private float projectileSpeedOverride = -1f; // -1 defaults to standard prefab velocity settings

    [Header("=== Animation ===")] 
    [SerializeField] private Animator anim;
    [SerializeField] private string shootTrigger = "Shoot";

    [Header("=== Audio ===")]
    [SerializeField] private AudioClip detectionSound;
    [SerializeField] private AudioClip fireSound;
    [Range(0f, 1f)] [SerializeField] private float fireVolume = 1f;

    private float fireTimer;
    private bool playerIsDetected = false; // Internal tracking state to prevent detection sound spamming

    private void Start()
    {
        fireTimer = firstShotDelay;

        if (anim == null) 
            anim = GetComponent<Animator>(); 

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) 
                player = playerObj.transform;
        }
    }

    private void Update()
    {
        if (player == null || blobPrefab == null || firePoint == null) 
            return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer > detectionRange) 
        {
            playerIsDetected = false; 
            return;
        }

        // Optional line-of-sight raycast validation
        if (lineOfSightBlockers.value != 0)
        {
            Vector2 directionToPlayer = ((Vector2)player.position - (Vector2)firePoint.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(firePoint.position, directionToPlayer, distanceToPlayer, lineOfSightBlockers);
            if (hit.collider != null) 
            {
                playerIsDetected = false; 
                return;
            }
        }

        if (!playerIsDetected)
        {
            playerIsDetected = true;
            TriggerAudioPlayback(detectionSound);
        }

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            ExecuteFiringSequence();
            fireTimer = fireRate;
        }
    }

    private void ExecuteFiringSequence()
    {
        TriggerAudioPlayback(fireSound);

        if (player != null && firePoint != null && blobPrefab != null)
        {
            Vector2 launchDirection = ((Vector2)player.position - (Vector2)firePoint.position).normalized;
            EnemyProjectile projectileInstance = Instantiate(blobPrefab, firePoint.position, Quaternion.identity);
            projectileInstance.Launch(launchDirection, projectileSpeedOverride);
        }

        if (anim != null) 
        {
            anim.SetTrigger(shootTrigger);
        }
    }

    private void TriggerAudioPlayback(AudioClip clipToPlay)
    {
        if (clipToPlay != null)
        {
            GameObject temporaryAudioContainer = new GameObject("RuntimeAudio_" + clipToPlay.name);
            temporaryAudioContainer.transform.position = transform.position;

            AudioSource dynamicSource = temporaryAudioContainer.AddComponent<AudioSource>();
            dynamicSource.clip = clipToPlay;
            dynamicSource.volume = fireVolume;
            dynamicSource.spatialBlend = 0.0f; // Constrains playback to 2D channel space for maximum consistency
            dynamicSource.Play();

            Destroy(temporaryAudioContainer, clipToPlay.length);
        }
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