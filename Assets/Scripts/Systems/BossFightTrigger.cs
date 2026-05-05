using UnityEngine;

public class BossFightTrigger : MonoBehaviour
{
    [Header("=== References ===")]
    [SerializeField] private ShadowBoss boss;
    
    [Header("=== Arena Lockdown ===")]
    [SerializeField] private GameObject[] activateOnFightStart;
    [SerializeField] private GameObject[] deactivateOnFightStart;
    
    [Header("=== Settings ===")]
    [SerializeField] private float startDelay = 0.5f;
    
    private bool fightActive = false;
    private bool bossDefeated = false;

    private void Start()
    {
        if (boss != null)
            boss.OnBossDefeated += OnBossDefeated;
            
        PlayerHealth.OnPlayerDied += OnPlayerDied;
    }
    
    private void OnDestroy()
    {
        PlayerHealth.OnPlayerDied -= OnPlayerDied;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (fightActive || bossDefeated) return;
        
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null)
            player = other.GetComponentInParent<PlayerController>();
            
        if (player != null)
        {
            fightActive = true;
            StartCoroutine(StartFightSequence());
        }
    }

    private System.Collections.IEnumerator StartFightSequence()
    {
        foreach (var obj in activateOnFightStart)
        {
            if (obj != null) obj.SetActive(true);
        }
        
        foreach (var obj in deactivateOnFightStart)
        {
            if (obj != null) obj.SetActive(false);
        }
        
        yield return new WaitForSeconds(startDelay);
        
        if (boss != null)
            boss.StartFight();
    }
    
    private void OnPlayerDied()
    {
        if (bossDefeated) return;
        
        fightActive = false;
        
        foreach (var obj in activateOnFightStart)
        {
            if (obj != null) obj.SetActive(false);
        }
        
        foreach (var obj in deactivateOnFightStart)
        {
            if (obj != null) obj.SetActive(true);
        }
    }

    private void OnBossDefeated()
    {
        bossDefeated = true;
        fightActive = false;
        
        foreach (var obj in activateOnFightStart)
        {
            if (obj != null) obj.SetActive(false);
        }
        
        foreach (var obj in deactivateOnFightStart)
        {
            if (obj != null) obj.SetActive(true);
        }
    }

    private void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            
            if (col is BoxCollider2D box)
            {
                Gizmos.DrawCube(transform.position + (Vector3)box.offset, box.size);
            }
            else if (col is CircleCollider2D circle)
            {
                Gizmos.DrawSphere(transform.position + (Vector3)circle.offset, circle.radius);
            }
        }
        
        if (boss != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, boss.transform.position);
        }
    }
}
