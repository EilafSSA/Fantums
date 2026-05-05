using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CeilingHook : MonoBehaviour
{
    [Header("=== Movement ===")]
    [SerializeField] private HookRail rail;
    [SerializeField] private float baseSpeed = 2f;
    [SerializeField] private float arrivalThreshold = 0.1f;
    
    [Header("=== Speed Modifiers ===")]
    [SerializeField] private float speedMultiplier = 1f;
    
    [Header("=== Corruption ===")]
    [SerializeField] private bool isCorrupted = false;
    [SerializeField] private float corruptionDuration = 3f;
    [SerializeField] private Color corruptedColor = new Color(0.5f, 0f, 0.5f, 1f);
    
    [Header("=== Visual ===")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    private int currentWaypointIndex = 0;
    private int direction = 1;
    private float corruptionTimer = 0f;
    private Color originalColor;
    private bool isDestroying = false;

    public System.Action<CeilingHook> OnDestroyed;

    public bool IsCorrupted => isCorrupted;
    public HookRail Rail => rail;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
            
        if (spriteRenderer != null)
        {
            if (spriteRenderer.sprite == null)
            {
                spriteRenderer.sprite = CreateDefaultSprite();
            }
            originalColor = spriteRenderer.color;
        }
    }
    
    private Sprite CreateDefaultSprite()
    {
        Texture2D tex = new Texture2D(4, 4);
        Color[] colors = new Color[16];
        for (int i = 0; i < 16; i++) colors[i] = Color.white;
        tex.SetPixels(colors);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
    }

    private void Update()
    {
        if (isDestroying) return;
        
        MoveAlongRail();
        
        if (isCorrupted)
        {
            UpdateCorruption();
        }
    }

    private void MoveAlongRail()
    {
        if (rail == null || rail.WaypointCount == 0) return;

        Vector3 targetPos = rail.GetWaypointPosition(currentWaypointIndex);
        float speed = baseSpeed * speedMultiplier;
        
        transform.position = Vector3.MoveTowards(
            transform.position, 
            targetPos, 
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPos) < arrivalThreshold)
        {
            AdvanceToNextWaypoint();
        }
    }

    private void AdvanceToNextWaypoint()
    {
        if (rail.Loop)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % rail.WaypointCount;
        }
        else
        {
            currentWaypointIndex += direction;
            
            if (currentWaypointIndex >= rail.WaypointCount)
            {
                currentWaypointIndex = rail.WaypointCount - 2;
                direction = -1;
            }
            else if (currentWaypointIndex < 0)
            {
                currentWaypointIndex = 1;
                direction = 1;
            }
        }
    }

    public void SetRail(HookRail newRail, int startingWaypoint = 0)
    {
        rail = newRail;
        currentWaypointIndex = Mathf.Clamp(startingWaypoint, 0, rail.WaypointCount - 1);
        
        if (rail != null && rail.WaypointCount > 0)
        {
            transform.position = rail.GetWaypointPosition(currentWaypointIndex);
        }
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }

    public void Corrupt()
    {
        if (isCorrupted || isDestroying) return;
        
        isCorrupted = true;
        corruptionTimer = corruptionDuration;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = corruptedColor;
        }
    }

    public void SetCorruptionDuration(float duration)
    {
        corruptionDuration = duration;
    }

    private void UpdateCorruption()
    {
        corruptionTimer -= Time.deltaTime;
        
        if (spriteRenderer != null)
        {
            float flickerSpeed = Mathf.Lerp(2f, 10f, 1f - (corruptionTimer / corruptionDuration));
            float alpha = Mathf.Abs(Mathf.Sin(Time.time * flickerSpeed));
            Color c = corruptedColor;
            c.a = Mathf.Lerp(0.3f, 1f, alpha);
            spriteRenderer.color = c;
        }
        
        if (corruptionTimer <= 0f)
        {
            DestroyHook();
        }
    }

    private void DestroyHook()
    {
        isDestroying = true;
        OnDestroyed?.Invoke(this);
        Destroy(gameObject);
    }

    public void ResetCorruption()
    {
        isCorrupted = false;
        corruptionTimer = 0f;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (rail != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, rail.transform.position);
        }
    }
}
