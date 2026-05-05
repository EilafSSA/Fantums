using System.Collections.Generic;
using UnityEngine;

public class BossArena : MonoBehaviour
{
    [Header("=== Hook System ===")]
    [SerializeField] private GameObject hookPrefab;
    [SerializeField] private HookRail[] hookRails;
    [SerializeField] private int initialHookCount = 3;
    [SerializeField] private int maxHooks = 6;
    
    [Header("=== Shadow Arms ===")]
    [SerializeField] private GameObject shadowArmPrefab;
    [SerializeField] private int armPoolSize = 20;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;
    [Range(0f, 1f)]
    [SerializeField] private float wallSpawnChance = 0.5f;
    
    [Header("=== Arena Bounds ===")]
    [SerializeField] private float arenaWidth = 10f;
    [SerializeField] private float arenaHeight = 6f;
    
    [Header("=== Phase Settings ===")]
    [SerializeField] private PhaseSettings phase1Settings;
    [SerializeField] private PhaseSettings phase2Settings;
    [SerializeField] private PhaseSettings phase3Settings;
    
    private List<CeilingHook> activeHooks = new List<CeilingHook>();
    private List<ShadowArm> armPool = new List<ShadowArm>();
    private List<ShadowArm> activeArms = new List<ShadowArm>();
    private int currentPhase = 1;
    private PhaseSettings currentSettings;
    
    private float armSpawnTimer = 0f;
    private float hookSpawnTimer = 0f;
    private bool isInvincibilityActive = false;
    private Transform bossTransform;

    [System.Serializable]
    public class PhaseSettings
    {
        public float hookSpeedMultiplier = 1f;
        public float hookSpawnInterval = 5f;
        public int maxActiveHooks = 4;
        public float armSpawnInterval = 1f;
        public int armsPerWave = 2;
        public bool canCorruptHooks = false;
        public float corruptionDuration = 3f;
        public float randomness = 0.1f;
    }

    private void Awake()
    {
        currentSettings = phase1Settings;
    }

    private void Start()
    {
        ShadowBoss boss = FindFirstObjectByType<ShadowBoss>();
        if (boss != null)
            bossTransform = boss.transform;
            
        InitializeArmPool();
        SpawnInitialHooks();
    }

    private void Update()
    {
        if (isInvincibilityActive)
        {
            UpdateArmSpawning();
        }
        
        UpdateHookSpawning();
    }

    private void InitializeArmPool()
    {
        if (shadowArmPrefab == null)
            return;

        for (int i = 0; i < armPoolSize; i++)
        {
            GameObject armObj = Instantiate(shadowArmPrefab, transform);
            armObj.SetActive(false);
            
            ShadowArm arm = armObj.GetComponent<ShadowArm>();
            if (arm != null)
            {
                arm.OnFullyRetracted += OnArmRetracted;
                armPool.Add(arm);
            }
        }
    }

    private void SpawnInitialHooks()
    {
        if (hookPrefab == null || hookRails == null || hookRails.Length == 0)
            return;

        for (int i = 0; i < initialHookCount && i < hookRails.Length; i++)
        {
            SpawnHookOnRail(hookRails[i]);
        }
    }

    public void SetPhase(int phase)
    {
        currentPhase = Mathf.Clamp(phase, 1, 3);
        
        switch (currentPhase)
        {
            case 1: currentSettings = phase1Settings; break;
            case 2: currentSettings = phase2Settings; break;
            case 3: currentSettings = phase3Settings; break;
        }
        
        foreach (var hook in activeHooks)
        {
            if (hook != null)
                hook.SetSpeedMultiplier(currentSettings.hookSpeedMultiplier);
        }
    }

    public void OnBossInvincibilityStart()
    {
        isInvincibilityActive = true;
        armSpawnTimer = 0f;
        SpawnArmWave();
    }

    public void OnBossInvincibilityEnd()
    {
        isInvincibilityActive = false;
        
        foreach (var arm in activeArms)
        {
            if (arm != null && arm.IsActive)
                arm.Retract();
        }
    }

    private void UpdateArmSpawning()
    {
        armSpawnTimer += Time.deltaTime;
        
        float interval = currentSettings.armSpawnInterval;
        interval *= 1f + Random.Range(-currentSettings.randomness, currentSettings.randomness);
        
        if (armSpawnTimer >= interval)
        {
            armSpawnTimer = 0f;
            SpawnArmWave();
        }
    }

    private void UpdateHookSpawning()
    {
        if (activeHooks.Count >= currentSettings.maxActiveHooks) return;
        
        hookSpawnTimer += Time.deltaTime;
        
        if (hookSpawnTimer >= currentSettings.hookSpawnInterval)
        {
            hookSpawnTimer = 0f;
            TrySpawnNewHook();
        }
    }

    private void SpawnArmWave()
    {
        int armsToSpawn = currentSettings.armsPerWave;
        
        if (currentPhase == 3)
            armsToSpawn += Random.Range(0, 3);

        for (int i = 0; i < armsToSpawn; i++)
        {
            SpawnArm();
        }
    }

    private void SpawnArm()
    {
        ShadowArm arm = GetAvailableArm();
        if (arm == null || bossTransform == null) return;

        bool spawnFromWall = Random.value < wallSpawnChance;
        
        if (spawnFromWall)
        {
            SpawnArmFromWall(arm);
        }
        else
        {
            SpawnArmFromGround(arm);
        }
        
        activeArms.Add(arm);
    }
    
    private void SpawnArmFromGround(ShadowArm arm)
    {
        float x = bossTransform.position.x + Random.Range(-arenaWidth, arenaWidth);
        Vector3 spawnPos = new Vector3(x, bossTransform.position.y, 0f);
        
        RaycastHit2D hit = Physics2D.Raycast(spawnPos, Vector2.down, 20f, groundLayer);
        if (hit.collider != null)
            spawnPos.y = hit.point.y;
        else
            spawnPos.y = bossTransform.position.y - 5f;
        
        arm.Emerge(spawnPos, ShadowArm.EmergeDirection.Up);
    }
    
    private void SpawnArmFromWall(ShadowArm arm)
    {
        bool spawnLeft = Random.value < 0.5f;
        float y = bossTransform.position.y + Random.Range(-arenaHeight * 0.5f, arenaHeight * 0.5f);
        Vector3 spawnPos = new Vector3(bossTransform.position.x, y, 0f);
        
        Vector2 rayDir = spawnLeft ? Vector2.left : Vector2.right;
        LayerMask hitLayer = wallLayer.value != 0 ? wallLayer : groundLayer;
        
        RaycastHit2D hit = Physics2D.Raycast(spawnPos, rayDir, 30f, hitLayer);
        if (hit.collider != null)
        {
            spawnPos.x = hit.point.x;
            ShadowArm.EmergeDirection dir = spawnLeft 
                ? ShadowArm.EmergeDirection.Right 
                : ShadowArm.EmergeDirection.Left;
            arm.Emerge(spawnPos, dir);
        }
        else
        {
            SpawnArmFromGround(arm);
        }
    }

    private ShadowArm GetAvailableArm()
    {
        foreach (var arm in armPool)
        {
            if (!arm.IsActive && !arm.gameObject.activeSelf)
                return arm;
        }
        return null;
    }

    private void OnArmRetracted(ShadowArm arm)
    {
        activeArms.Remove(arm);
    }

    private void TrySpawnNewHook()
    {
        if (hookPrefab == null || hookRails == null || hookRails.Length == 0) return;
        
        List<HookRail> availableRails = new List<HookRail>();
        
        foreach (var rail in hookRails)
        {
            int hooksOnRail = CountHooksOnRail(rail);
            if (hooksOnRail < 2)
                availableRails.Add(rail);
        }
        
        if (availableRails.Count > 0)
        {
            HookRail rail = availableRails[Random.Range(0, availableRails.Count)];
            SpawnHookOnRail(rail);
        }
    }

    private void SpawnHookOnRail(HookRail rail)
    {
        if (hookPrefab == null || rail == null) return;
        
        GameObject hookObj = Instantiate(hookPrefab, transform);
        CeilingHook hook = hookObj.GetComponent<CeilingHook>();
        
        if (hook != null)
        {
            int startWaypoint = Random.Range(0, rail.WaypointCount);
            hook.SetRail(rail, startWaypoint);
            hook.SetSpeedMultiplier(currentSettings.hookSpeedMultiplier);
            hook.SetCorruptionDuration(currentSettings.corruptionDuration);
            
            hook.OnDestroyed += OnHookDestroyed;
            activeHooks.Add(hook);
        }
    }

    private int CountHooksOnRail(HookRail rail)
    {
        int count = 0;
        foreach (var hook in activeHooks)
        {
            if (hook != null && hook.Rail == rail)
                count++;
        }
        return count;
    }

    private void OnHookDestroyed(CeilingHook hook)
    {
        activeHooks.Remove(hook);
    }

    public List<CeilingHook> GetActiveHooks()
    {
        activeHooks.RemoveAll(h => h == null);
        return new List<CeilingHook>(activeHooks);
    }
}
