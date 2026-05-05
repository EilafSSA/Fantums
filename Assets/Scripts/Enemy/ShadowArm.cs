using UnityEngine;

public class ShadowArm : MonoBehaviour
{
    public enum EmergeDirection { Up, Down, Left, Right }
    
    [Header("=== Warning ===")]
    [SerializeField] private float warningDuration = 1f;
    [SerializeField] private Color warningColor = new Color(1f, 0f, 0f, 0.5f);
    
    [Header("=== Emergence ===")]
    [SerializeField] private float riseHeight = 3f;
    [SerializeField] private float riseSpeed = 5f;
    [SerializeField] private float retractSpeed = 8f;
    
    [Header("=== Combat ===")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float knockbackForce = 8f;
    [SerializeField] private float damageCooldown = 0.5f;
    
    [Header("=== Visual ===")]
    [SerializeField] private Color armColor = new Color(0.3f, 0f, 0.5f, 1f);
    
    private enum ArmState { Warning, Rising, Active, Retracting, Inactive }
    private ArmState state = ArmState.Inactive;
    
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Vector3 emergeDir;
    private Vector3 perpDir;
    private float riseProgress = 0f;
    private float warningTimer = 0f;
    private float lastDamageTime = -999f;
    private bool isActive = false;
    private SpriteRenderer spriteRenderer;
    private EmergeDirection direction = EmergeDirection.Up;

    public System.Action<ShadowArm> OnFullyRetracted;
    public bool IsActive => isActive;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null && spriteRenderer.sprite == null)
        {
            Texture2D tex = new Texture2D(4, 4);
            Color[] colors = new Color[16];
            for (int i = 0; i < 16; i++) colors[i] = Color.white;
            tex.SetPixels(colors);
            tex.Apply();
            spriteRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            spriteRenderer.sortingOrder = 50;
        }
    }

    private void Update()
    {
        switch (state)
        {
            case ArmState.Warning:
                UpdateWarning();
                break;
            case ArmState.Rising:
                UpdateRising();
                break;
            case ArmState.Active:
                UpdateActive();
                break;
            case ArmState.Retracting:
                UpdateRetracting();
                break;
        }
    }

    public void Emerge(Vector3 surfacePosition, EmergeDirection emergeDirection = EmergeDirection.Up)
    {
        direction = emergeDirection;
        SetDirectionVectors();
        
        startPosition = surfacePosition;
        targetPosition = surfacePosition + emergeDir * riseHeight;
        transform.position = startPosition;
        
        ApplyWarningScale();
        ApplyRotation();
        
        warningTimer = warningDuration;
        state = ArmState.Warning;
        isActive = true;
        
        gameObject.SetActive(true);
        
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = warningColor;
        }
    }
    
    private void SetDirectionVectors()
    {
        switch (direction)
        {
            case EmergeDirection.Up:
                emergeDir = Vector3.up;
                perpDir = Vector3.right;
                break;
            case EmergeDirection.Down:
                emergeDir = Vector3.down;
                perpDir = Vector3.right;
                break;
            case EmergeDirection.Left:
                emergeDir = Vector3.left;
                perpDir = Vector3.up;
                break;
            case EmergeDirection.Right:
                emergeDir = Vector3.right;
                perpDir = Vector3.up;
                break;
        }
    }
    
    private void ApplyRotation()
    {
        float angle = 0f;
        switch (direction)
        {
            case EmergeDirection.Up: angle = 0f; break;
            case EmergeDirection.Down: angle = 180f; break;
            case EmergeDirection.Left: angle = 90f; break;
            case EmergeDirection.Right: angle = -90f; break;
        }
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
    
    private void ApplyWarningScale()
    {
        transform.localScale = new Vector3(1.5f, 0.3f, 1f);
    }

    public void Retract()
    {
        if (state == ArmState.Inactive || state == ArmState.Retracting) return;
        state = ArmState.Retracting;
    }

    public void EnableCorruption(bool enable)
    {
    }

    private void UpdateWarning()
    {
        warningTimer -= Time.deltaTime;
        
        float pulse = Mathf.Abs(Mathf.Sin(Time.time * 10f));
        
        if (spriteRenderer != null)
        {
            Color c = warningColor;
            c.a = Mathf.Lerp(0.2f, 0.7f, pulse);
            spriteRenderer.color = c;
        }
        
        float scaleX = 1.5f + Mathf.Sin(Time.time * 8f) * 0.2f;
        transform.localScale = new Vector3(scaleX, 0.3f, 1f);
        
        if (warningTimer <= 0f)
        {
            riseProgress = 0f;
            state = ArmState.Rising;
            
            if (spriteRenderer != null)
                spriteRenderer.color = armColor;
                
            transform.localScale = new Vector3(0.5f, 0.1f, 1f);
        }
    }

    private void UpdateRising()
    {
        riseProgress += Time.deltaTime * riseSpeed / riseHeight;
        riseProgress = Mathf.Clamp01(riseProgress);
        
        transform.position = Vector3.Lerp(startPosition, targetPosition, riseProgress);
        
        float scaleY = Mathf.Lerp(0.1f, 3f, riseProgress);
        transform.localScale = new Vector3(0.5f, scaleY, 1f);
        
        if (riseProgress >= 1f)
        {
            state = ArmState.Active;
        }
    }

    private void UpdateActive()
    {
        float sway = Mathf.Sin(Time.time * 3f + transform.position.x + transform.position.y) * 0.05f;
        transform.position = targetPosition + perpDir * sway;
    }

    private void UpdateRetracting()
    {
        transform.position = Vector3.MoveTowards(
            transform.position, 
            startPosition, 
            retractSpeed * Time.deltaTime
        );
        
        float dist = Vector3.Distance(transform.position, startPosition);
        float scaleY = Mathf.Lerp(0.1f, 3f, dist / riseHeight);
        transform.localScale = new Vector3(0.5f, scaleY, 1f);
        
        if (dist < 0.1f)
        {
            state = ArmState.Inactive;
            isActive = false;
            OnFullyRetracted?.Invoke(this);
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamagePlayer(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamagePlayer(other);
    }

    private void TryDamagePlayer(Collider2D other)
    {
        if (state != ArmState.Active && state != ArmState.Rising) return;
        if (Time.time - lastDamageTime < damageCooldown) return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth == null)
            playerHealth = other.GetComponentInParent<PlayerHealth>();
            
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            lastDamageTime = Time.time;
            
            Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
            if (playerRb == null)
                playerRb = other.GetComponentInParent<Rigidbody2D>();
                
            if (playerRb != null)
            {
                Vector2 knockbackDir = (other.transform.position - transform.position).normalized;
                knockbackDir.y = Mathf.Max(0.3f, knockbackDir.y);
                knockbackDir.Normalize();
                playerRb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
            }
        }
    }
}
