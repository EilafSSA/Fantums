using System.Collections;
using UnityEngine;

public class FinalBossArm : MonoBehaviour
{
    [Header("=== Health ===")]
    [SerializeField] private int maxHealth = 4;
    private int currentHealth;

    [Header("=== Visuals ===")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color idleColor = Color.white;
    [SerializeField] private Color attack1Color = Color.red;
    [SerializeField] private Color attack2Color = Color.blue;
    [SerializeField] private Color attack3Color = Color.magenta;
    [SerializeField] private Color hurtColor = new Color(1f, 1f, 1f, 0.5f);

    [Header("=== Idle Hover ===")]
    [SerializeField] private float hoverSpeed = 5f;
    [SerializeField] private float hoverDistance = 0.6f;
    [SerializeField] private float breatheScaleSpeed = 4f;
    [SerializeField] private float breatheScaleAmount = 0.1f;
    
    [Header("=== Damage ===")]
    [SerializeField] private int touchDamage = 1;

    public bool IsDead => currentHealth <= 0;
    public bool IsAttacking => isAttacking;
    
    private Vector3 idleBasePosition;
    private Vector3 initialScale;
    private bool isAttacking = false;
    private bool hasBeenHitThisAttack = false;
    private bool isReturning = false;
    private Coroutine attackRoutine;
    
    public System.Action<FinalBossArm> OnDeath;

    private void Awake()
    {
        currentHealth = maxHealth;
        initialScale = transform.localScale;
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) spriteRenderer.color = idleColor;
    }

    public void ResetArm()
    {
        if (attackRoutine != null) StopCoroutine(attackRoutine);
        gameObject.SetActive(true);
        currentHealth = maxHealth;
        isAttacking = false;
        hasBeenHitThisAttack = false;
        isReturning = false;
        transform.localScale = initialScale;
        if (spriteRenderer != null) spriteRenderer.color = idleColor;
    }

    public void SetIdlePosition(Vector3 pos)
    {
        idleBasePosition = pos;
        if (!isAttacking)
        {
            transform.position = pos;
        }
    }

    private void Update()
    {
        if (IsDead) return;

        if (!isAttacking)
        {

            float time = Time.time;
            transform.position = idleBasePosition + Vector3.up * Mathf.Sin(time * hoverSpeed) * hoverDistance;
            
            float scaleBreath = Mathf.Sin(time * breatheScaleSpeed) * breatheScaleAmount;
            transform.localScale = initialScale + new Vector3(scaleBreath, -scaleBreath * 0.5f, 0f);
        }
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;

        if (!isAttacking) return;
        if (hasBeenHitThisAttack) return;

        hasBeenHitThisAttack = true;
        currentHealth -= amount;
        StartCoroutine(HurtFlash());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (attackRoutine != null) StopCoroutine(attackRoutine);
        gameObject.SetActive(false);
        OnDeath?.Invoke(this);
    }

    private IEnumerator HurtFlash()
    {
        if (spriteRenderer == null) yield break;
        
        Color baseColor = spriteRenderer.color;
        float duration = 0.6f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float pulse = Mathf.Abs(Mathf.Sin((elapsed / duration) * Mathf.PI * 2f));
            spriteRenderer.color = Color.Lerp(baseColor, hurtColor, pulse);
            yield return null;
        }

        if (spriteRenderer != null)
        {
            if (!isAttacking) 
                spriteRenderer.color = idleColor;
            else
                spriteRenderer.color = baseColor;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsDead) return;

        PlayerHealth ph = other.GetComponent<PlayerHealth>();
        if (ph == null) ph = other.GetComponentInParent<PlayerHealth>();
        if (ph != null)
        {

            if (!isReturning)
            {
                ph.TakeDamage(touchDamage);
            }
        }
        else
        {

            ShadowBossHitbox hitbox = other.GetComponent<ShadowBossHitbox>();
            if (hitbox == null) hitbox = other.GetComponentInParent<ShadowBossHitbox>();

        }
    }

    public void DoAttack1(float arenaLeft, float arenaRight, float yLevel, float duration)
    {
        if (IsDead) return;
        attackRoutine = StartCoroutine(SweepAttackRoutine(arenaLeft, arenaRight, yLevel, attack1Color, duration));
    }

    public void DoAttack2(float targetX, float startY, float floorY, float duration)
    {
        if (IsDead) return;
        attackRoutine = StartCoroutine(SmashAttackRoutine(targetX, startY, floorY, attack2Color, duration));
    }

    public void DoAttack3(float startX, float endX, float yLevel, float duration)
    {
        if (IsDead) return;
        attackRoutine = StartCoroutine(SweepAttackRoutine(startX, endX, yLevel, attack3Color, duration));
    }

    private IEnumerator SweepAttackRoutine(float startX, float endX, float yLevel, Color telegraphColor, float duration)
    {
        isAttacking = true;
        hasBeenHitThisAttack = false;
        isReturning = false;
        transform.localScale = initialScale;
        if (spriteRenderer != null) spriteRenderer.color = telegraphColor;

        Vector3 startPos = new Vector3(startX, yLevel, 0f);
        yield return MoveTo(startPos, 0.5f);

        yield return new WaitForSeconds(0.5f);

        Vector3 endPos = new Vector3(endX, yLevel, 0f);
        yield return MoveTo(endPos, duration);

        yield return new WaitForSeconds(0.2f);

        isReturning = true;

        if (spriteRenderer != null) spriteRenderer.color = idleColor;
        yield return MoveTo(idleBasePosition, 0.5f);
        
        isAttacking = false;
        isReturning = false;
    }

    private IEnumerator SmashAttackRoutine(float targetX, float startY, float floorY, Color telegraphColor, float duration)
    {
        isAttacking = true;
        hasBeenHitThisAttack = false;
        isReturning = false;
        transform.localScale = initialScale;
        if (spriteRenderer != null) spriteRenderer.color = telegraphColor;

        Vector3 hoverPos = new Vector3(targetX, startY, 0f);
        yield return MoveTo(hoverPos, 0.5f);

        yield return new WaitForSeconds(0.5f);

        Vector3 floorPos = new Vector3(targetX, floorY, 0f);
        yield return MoveTo(floorPos, 0.15f);

        yield return new WaitForSeconds(0.5f);

        isReturning = true;

        if (spriteRenderer != null) spriteRenderer.color = idleColor;
        yield return MoveTo(idleBasePosition, 0.5f);

        isAttacking = false;
        isReturning = false;
    }

    private IEnumerator MoveTo(Vector3 target, float duration)
    {
        Vector3 start = transform.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(start, target, elapsed / duration);
            yield return null;
        }
        transform.position = target;
    }
}
