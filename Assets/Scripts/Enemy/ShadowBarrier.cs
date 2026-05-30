using UnityEngine;

public class ShadowBarrier : MonoBehaviour
{
    [Header("=== Barrier Settings ===")]
    [SerializeField] private int armCount = 6;
    [SerializeField] private float radius = 2f;
    [SerializeField] private float formSpeed = 3f;
    [SerializeField] private float dissolveSpeed = 5f;
    
    [Header("=== Animation ===")]
    [SerializeField] private float rotationSpeed = 30f;
    [SerializeField] private float pulseAmount = 0.2f;
    [SerializeField] private float pulseSpeed = 2f;
    
    
    [Header("=== Visual ===")]
    [SerializeField] private Color barrierColor = new Color(0.5f, 0f, 0.8f, 1f);
    
    private enum BarrierState { Inactive, Forming, Active, Dissolving }
    private BarrierState state = BarrierState.Inactive;
    
    private Animator anim; //addedbyEilaf
    private GameObject[] barrierArms;
    private SpriteRenderer[] armRenderers;
    private float formProgress = 0f;
    private float currentRotation = 0f;
    private Transform bossTransform;

    public bool IsActive => state == BarrierState.Active || state == BarrierState.Forming;

    private void Start()
    {
        CreateBarrierArms();
    }

    private void Update()
    {
        switch (state)
        {
            case BarrierState.Forming:
                UpdateForming();
                break;
            case BarrierState.Active:
                UpdateActive();
                break;
            case BarrierState.Dissolving:
                UpdateDissolving();
                break;
        }

        if (bossTransform != null)
        {
            transform.position = bossTransform.position;
        }
    }

    private void CreateBarrierArms()
    {
        barrierArms = new GameObject[armCount];
        armRenderers = new SpriteRenderer[armCount];
        
        Sprite defaultSprite = CreateDefaultSprite();
        
        for (int i = 0; i < armCount; i++)
        {
            GameObject arm = new GameObject($"BarrierArm_{i}");
            arm.transform.SetParent(transform);
            arm.transform.localPosition = Vector3.zero;
            
            SpriteRenderer sr = arm.AddComponent<SpriteRenderer>();
            sr.sprite = defaultSprite;
            sr.color = barrierColor;
            sr.sortingOrder = 100;
            arm.transform.localScale = new Vector3(0.4f, 2f, 1f);

            
            
            arm.SetActive(false);
            barrierArms[i] = arm;
            armRenderers[i] = sr;
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

    public void Initialize(Transform boss)
    {
        bossTransform = boss;
        transform.position = boss.position;
    }

    public void Activate()
    {
        if (barrierArms == null)
            CreateBarrierArms();
            
        formProgress = 0f;
        state = BarrierState.Forming;
        
        for (int i = 0; i < barrierArms.Length; i++)
        {
            if (barrierArms[i] != null)
            {
                barrierArms[i].SetActive(true);
                barrierArms[i].transform.localScale = Vector3.zero;
            }
        }
        
        PositionArms(0f);
    }

    public void Deactivate()
    {
        if (state == BarrierState.Inactive || state == BarrierState.Dissolving) return;
        state = BarrierState.Dissolving;
    }

    private void UpdateForming()
    {
        formProgress += Time.deltaTime * formSpeed;
        formProgress = Mathf.Clamp01(formProgress);
        
        float scale = Mathf.SmoothStep(0f, 1f, formProgress);
        
        for (int i = 0; i < barrierArms.Length; i++)
        {
            if (barrierArms[i] != null)
            {
                barrierArms[i].transform.localScale = new Vector3(0.4f * scale, 2f * scale, 1f);
            }
        }
        
        PositionArms(formProgress);
        
        if (formProgress >= 1f)
        {
            state = BarrierState.Active;
        }
    }

    private void UpdateActive()
    {
        currentRotation += rotationSpeed * Time.deltaTime;
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        PositionArms(1f, pulse);
    }

    private void UpdateDissolving()
    {
        formProgress -= Time.deltaTime * dissolveSpeed;
        formProgress = Mathf.Clamp01(formProgress);
        
        float scale = Mathf.SmoothStep(0f, 1f, formProgress);
        
        for (int i = 0; i < barrierArms.Length; i++)
        {
            if (barrierArms[i] != null)
            {
                barrierArms[i].transform.localScale = new Vector3(0.4f * scale, 2f * scale, 1f);
                
                if (armRenderers[i] != null)
                {
                    Color c = barrierColor;
                    c.a = formProgress;
                    armRenderers[i].color = c;
                }
            }
        }
        
        if (formProgress <= 0f)
        {
            state = BarrierState.Inactive;
            
            for (int i = 0; i < barrierArms.Length; i++)
            {
                if (barrierArms[i] != null)
                {
                    barrierArms[i].SetActive(false);
                    if (armRenderers[i] != null)
                        armRenderers[i].color = barrierColor;
                }
            }
        }
    }

    private void PositionArms(float progress, float radiusMultiplier = 1f)
    {
        float currentRadius = radius * radiusMultiplier;
        float angleStep = 360f / armCount;
        
        for (int i = 0; i < barrierArms.Length; i++)
        {
            if (barrierArms[i] == null) continue;
            
            float angle = (angleStep * i + currentRotation) * Mathf.Deg2Rad;
            float armRadius = Mathf.Lerp(currentRadius * 3f, currentRadius, progress);
            
            Vector3 localPos = new Vector3(
                Mathf.Cos(angle) * armRadius,
                Mathf.Sin(angle) * armRadius,
                0f
            );
            
            barrierArms[i].transform.localPosition = localPos;
            
            float rotZ = angle * Mathf.Rad2Deg - 90f;
            barrierArms[i].transform.localRotation = Quaternion.Euler(0f, 0f, rotZ);
        }
    }
}
