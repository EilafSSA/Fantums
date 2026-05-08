using UnityEngine;
using System.Collections;

public class ShadowBoss : MonoBehaviour
{
    [Header("=== Health ===")]
    [SerializeField] private int maxHealth = 4;
    [SerializeField] private float invincibilityCooldown = 15f;
    
    [Header("=== References ===")]
    [SerializeField] private BossArena arena;
    [SerializeField] private ShadowBarrier barrier;
    [SerializeField] private Transform valveTransform;
    
    [Header("=== Animation ===")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    [Header("=== Visual Feedback ===")]
    [SerializeField] private Color hurtFlashColor = Color.white;
    [SerializeField] private float hurtFlashDuration = 0.1f;
    [SerializeField] private Color phase2Tint = new Color(0.8f, 0.6f, 1f);
    [SerializeField] private Color phase3Tint = new Color(0.6f, 0.3f, 0.8f);
    
    [Header("=== Breathing Effect ===")]
    [SerializeField] private float breatheSpeed = 2f;
    [SerializeField] private float breatheAmountY = 0.1f;
    [SerializeField] private float breatheAmountScale = 0.02f;
    
    [Header("=== Audio ===")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip phaseTransitionSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip valveSound;

    public enum BossState { Idle, Hurt, TurningToValve, SpinningValve, TurningBack, Invincible, Defeated }
    public enum BossPhase { Phase1, Phase2, Phase3 }

    private BossState currentState = BossState.Idle;
    private BossPhase currentPhase = BossPhase.Phase1;
    private int currentHealth;
    private float invincibilityTimer = 0f;
    private Color originalColor;
    private bool isFightActive = false;
    
    private Vector3 basePosition;
    private Vector3 baseScale;
    private bool isDefeated = false;

    //private static readonly int AnimPhase = Animator.StringToHash("Phase");
    //private static readonly int AnimIsHurt = Animator.StringToHash("IsHurt");
    //private static readonly int AnimIsInvincible = Animator.StringToHash("IsInvincible");
    //private static readonly int AnimTurnToValve = Animator.StringToHash("TurnToValve");
    //private static readonly int AnimSpinValve = Animator.StringToHash("SpinValve");
    //private static readonly int AnimTurnBack = Animator.StringToHash("TurnBack");
     //static readonly int AnimDefeated = Animator.StringToHash("Defeated");

    public System.Action<int> OnHealthChanged;
    public System.Action<BossPhase> OnPhaseChanged;
    public System.Action OnBossDefeated;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public BossState State => currentState;
    public BossPhase Phase => currentPhase;
    public bool IsInvincible => currentState == BossState.Invincible || 
                                 currentState == BossState.Hurt ||
                                 currentState == BossState.TurningToValve ||
                                 currentState == BossState.SpinningValve ||
                                 currentState == BossState.TurningBack;

    private void Awake()
    {
        currentHealth = maxHealth;
        
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
            
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
            
        if (animator == null)
            animator = GetComponent<Animator>();
        
        if (barrier == null)
            barrier = GetComponentInChildren<ShadowBarrier>();
            
        if (barrier != null)
            barrier.Initialize(transform);
        
        if (arena == null)
            arena = FindFirstObjectByType<BossArena>();
        
        basePosition = transform.position;
        baseScale = transform.localScale;
    }

    private void Start()
    {
        if (arena != null)
            arena.SetPhase(1);
            
        basePosition = transform.position;
        
        PlayerHealth.OnPlayerDied += OnPlayerDied;
    }
    
    private void OnDestroy()
    {
        PlayerHealth.OnPlayerDied -= OnPlayerDied;
    }
    
    private void OnPlayerDied()
    {
        if (isDefeated) return;
        
        StopAllCoroutines();
        ResetBoss();
    }
    
    private void ResetBoss()
    {
        currentHealth = maxHealth;
        currentPhase = BossPhase.Phase1;
        currentState = BossState.Idle;
        isFightActive = false;
        invincibilityTimer = 0f;
        
        if (barrier != null)
            barrier.Deactivate();
        
        if (arena != null)
        {
            arena.OnBossInvincibilityEnd();
            arena.SetPhase(1);
        }
        
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
        
        if (animator != null)
        {
            animator.SetBool("AnimIsInvincible", false); //eilaf
            animator.SetInteger("AnimPhase", 1); //eilaf important
        }
        
        transform.localScale = baseScale;
        
        OnHealthChanged?.Invoke(currentHealth);
        OnPhaseChanged?.Invoke(currentPhase);
    }

    private void Update()
    {
        UpdateBreathing();
        
        if (!isFightActive || currentState == BossState.Defeated) return;

        switch (currentState)
        {
            case BossState.Invincible:
                UpdateInvincibility();
                break;
        }
    }
    
    private void UpdateBreathing()
    {
        if (currentState == BossState.Defeated) return;
        
        float breathe = Mathf.Sin(Time.time * breatheSpeed);
        
        Vector3 pos = basePosition;
        pos.y += breathe * breatheAmountY;
        transform.position = pos;
        
        float scaleOffset = breathe * breatheAmountScale;
        transform.localScale = new Vector3(
            baseScale.x + scaleOffset,
            baseScale.y - scaleOffset * 0.5f,
            baseScale.z
        );
    }

    public void StartFight()
    {
        if (isFightActive || isDefeated) return;
        
        isFightActive = true;
        currentState = BossState.Idle;
    }

    public void TakeDamage(int damage)
    {
        if (!isFightActive || currentState == BossState.Defeated || isDefeated) return;
        
        if (IsInvincible)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            StartCoroutine(DefeatSequence());
        }
        else
        {
            StartCoroutine(HurtSequence());
        }
    }

    private IEnumerator HurtSequence()
    {
        currentState = BossState.Hurt;
        
        StartCoroutine(FlashSprite());
        
        if (animator != null)
        {
            animator.SetTrigger("AnimIsHurt"); //eilaf
        }
        
        PlaySound(hurtSound);
        
        if (CameraFollow.Instance != null)
        {
            CameraFollow.Instance.TriggerShake();
        }
        
        yield return new WaitForSeconds(0.3f);
        
        UpdatePhase();
        
        StartCoroutine(ValveSequence());
    }

    private IEnumerator ValveSequence()
    {
        currentState = BossState.TurningToValve;
        
        if (animator != null)
        {
            animator.SetTrigger("AnimTurnToValve"); //eilaf
        }
        
        yield return new WaitForSeconds(0.5f);
        
        currentState = BossState.SpinningValve;
        
        if (animator != null)
        {
            animator.SetTrigger("AnimSpinValve"); //eilaf 
        }
        
        PlaySound(valveSound);
        
        yield return new WaitForSeconds(1f);
        
        currentState = BossState.TurningBack;
        
        if (animator != null)
        {
            animator.SetTrigger("AnimTurnBack"); //eilaf
        }
        
        yield return new WaitForSeconds(0.5f);
        
        EnterInvincibility();
    }

    private void EnterInvincibility()
    {
        currentState = BossState.Invincible;
        invincibilityTimer = invincibilityCooldown;
        
        if (barrier != null)
            barrier.Activate();
        
        if (arena != null)
            arena.OnBossInvincibilityStart();
        
        if (animator != null)
            animator.SetBool("AnimIsInvincible", true); //eilaf
    }

    private void UpdateInvincibility()
    {
        invincibilityTimer -= Time.deltaTime;
        
        if (invincibilityTimer <= 0f)
        {
            ExitInvincibility();
        }
    }

    private void ExitInvincibility()
    {
        currentState = BossState.Idle;
        
        if (barrier != null)
        {
            barrier.Deactivate();
        }
        
        if (arena != null)
        {
            arena.OnBossInvincibilityEnd();
        }
        
        if (animator != null)
        {
            animator.SetBool("AnimIsInvincible", false); //eilaf
        }
        

    }

    private void UpdatePhase()
    {
        BossPhase newPhase = currentPhase;
        
        if (currentHealth >= 4)
        {
            newPhase = BossPhase.Phase1;
        }
        else if (currentHealth >= 2)
        {
            newPhase = BossPhase.Phase2;
        }
        else
        {
            newPhase = BossPhase.Phase3;
        }
        
        if (newPhase != currentPhase)
        {
            TransitionToPhase(newPhase);
        }
    }

    private void TransitionToPhase(BossPhase newPhase)
    {
        currentPhase = newPhase;
        
        int phaseNum = (int)currentPhase + 1;
        

        
        if (arena != null)
        {
            arena.SetPhase(phaseNum);
        }
        
        if (spriteRenderer != null)
        {
            switch (currentPhase)
            {
                case BossPhase.Phase1:
                    spriteRenderer.color = originalColor;
                    break;
                case BossPhase.Phase2:
                    spriteRenderer.color = phase2Tint;
                    break;
                case BossPhase.Phase3:
                    spriteRenderer.color = phase3Tint;
                    break;
            }
        }
        
        if (animator != null)
        {
            animator.SetInteger("AnimPhase", phaseNum);
        }
        
        PlaySound(phaseTransitionSound);
        
        if (CameraFollow.Instance != null)
        {
            CameraFollow.Instance.TriggerShake();
        }
        
        OnPhaseChanged?.Invoke(currentPhase);
    }

    private IEnumerator DefeatSequence()
    {
        currentState = BossState.Defeated;
        isFightActive = false;
        isDefeated = true;
        
        if (barrier != null)
            barrier.Deactivate();
        
        if (arena != null)
            arena.OnBossInvincibilityEnd();
        
        if (animator != null)
            animator.SetTrigger("AnimDefeated");
        
        PlaySound(deathSound);
        
        if (CameraFollow.Instance != null)
            CameraFollow.Instance.TriggerShake();
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddEnemyKillScore();
            GameManager.Instance.AddScore(1000);
        }
        
        yield return new WaitForSeconds(2f);
        
        OnBossDefeated?.Invoke();
    }

    private IEnumerator FlashSprite()
    {
        if (spriteRenderer == null) yield break;
        
        Color startColor = spriteRenderer.color;
        spriteRenderer.color = hurtFlashColor;
        
        yield return new WaitForSeconds(hurtFlashDuration);
        
        switch (currentPhase)
        {
            case BossPhase.Phase1:
                spriteRenderer.color = originalColor;
                break;
            case BossPhase.Phase2:
                spriteRenderer.color = phase2Tint;
                break;
            case BossPhase.Phase3:
                spriteRenderer.color = phase3Tint;
                break;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public float GetInvincibilityTimeRemaining()
    {
        return currentState == BossState.Invincible ? invincibilityTimer : 0f;
    }

    public float GetInvincibilityProgress()
    {
        if (currentState != BossState.Invincible) return 0f;
        return 1f - (invincibilityTimer / invincibilityCooldown);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1f);
        
        if (valveTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, valveTransform.position);
            Gizmos.DrawWireSphere(valveTransform.position, 0.5f);
        }
    }
}
