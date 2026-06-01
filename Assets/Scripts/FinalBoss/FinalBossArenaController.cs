using System.Collections;
using UnityEngine;
public class FinalBossArenaController : MonoBehaviour
{
    //ADDED BY EILAF:
    [Header("=== Reference ===")]
    [SerializeField] public FinalBossMainBody mainbody;

    [Header("=== Boss ===")]
    [SerializeField] private Transform boss;
    [SerializeField] private BossIntroCutscene introCutscene;
    [Tooltip("How much of the screen height (from the top) the boss hovers down from.")]
    [Range(0.02f, 0.95f)]
    [SerializeField] private float bossViewportYFromTop = 0.5f;
    [Tooltip("Horizontal viewport position (0=left, 1=right, 0.5=center).")]
    [Range(0f, 1f)]
    [SerializeField] private float bossViewportX = 0.85f;
    [SerializeField] private float bossFollowLerp = 6f;

    [Header("=== Arms ===")]
    [SerializeField] private FinalBossArm leftArm;
    [SerializeField] private FinalBossArm rightArm;
    [SerializeField] private float attackInterval = 3f;
    [SerializeField] private float sweepAttackDuration = 2f;
    [SerializeField] private float smashAttackDuration = 0.5f;
    [SerializeField] private float dualSweepDuration = 1.5f;

    [Header("=== Death Animation ===")]
    //[SerializeField] private float deathShakeDuration = 1.5f;
    //[SerializeField] private float deathFadeDuration = 1f;
    [SerializeField] private Color deathColor = Color.red;

    [Header("=== Arena Bounds ===")]
    [SerializeField] private float arenaCenterX = 0f;
    [SerializeField] private float arenaWidth = 18f;
    [SerializeField] private float arenaBottomY = 0f;
    [SerializeField] private float arenaTopY = 40f;

    private Animator anim; //addedbyEilaf
    private Camera mainCam;
    private bool fightActive = false;
    private bool bossDefeated = false;
    private bool introFinished = false;
    private Transform lastPlayer;
    private Coroutine fightRoutine;

    private void Start()
    {
        anim = GetComponent<Animator>(); //addedbyEilaf
        mainCam = Camera.main;

        if (leftArm != null) leftArm.OnDeath += OnArmDied;
        if (rightArm != null) rightArm.OnDeath += OnArmDied;
        
        PlayerHealth.OnPlayerDied += OnPlayerDied;

        if (boss != null) boss.gameObject.SetActive(false);
        if (leftArm != null) leftArm.gameObject.SetActive(false);
        if (rightArm != null) rightArm.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (leftArm != null) leftArm.OnDeath -= OnArmDied;
        if (rightArm != null) rightArm.OnDeath -= OnArmDied;
        
        PlayerHealth.OnPlayerDied -= OnPlayerDied;
    }

    private void OnPlayerDied()
    {
        if (bossDefeated) return;
        ResetArena();
    }

    private void ResetArena()
    {
        fightActive = false;
        introFinished = false;
        if (fightRoutine != null) StopCoroutine(fightRoutine);

        if (leftArm != null)
        {
            leftArm.ResetArm();
            leftArm.gameObject.SetActive(false);
        }
        if (rightArm != null)
        {
            rightArm.ResetArm();
            rightArm.gameObject.SetActive(false);
        }
        
        if (boss != null)
        {
            boss.gameObject.SetActive(false);
            boss.rotation = Quaternion.identity;
            SpriteRenderer bossSr = boss.GetComponent<SpriteRenderer>();
            if (bossSr != null)
            {
                Color col = bossSr.color;
                col.a = 1f;
                bossSr.color = col;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (fightActive || bossDefeated) return;

        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc == null) pc = other.GetComponentInParent<PlayerController>();

        if (pc != null)
        {
            fightActive = true;
            lastPlayer = pc.transform;

            if (boss != null)
            {

                boss.position = new Vector3(arenaCenterX + (arenaWidth * 0.35f), (arenaTopY + arenaBottomY) * 0.5f, boss.position.z);
                boss.gameObject.SetActive(true);
            }
            if (leftArm != null) leftArm.gameObject.SetActive(true);
            if (rightArm != null) rightArm.gameObject.SetActive(true);
            
            fightRoutine = StartCoroutine(FightRoutine());
        }
    }

    private void LateUpdate()
    {
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null || boss == null || !fightActive) return;

        if (leftArm != null && !leftArm.IsDead)
            leftArm.SetIdlePosition(boss.position + new Vector3(0.5f, -2.3f, 0f));
        if (rightArm != null && !rightArm.IsDead)
            rightArm.SetIdlePosition(boss.position + new Vector3(-3.5f, -2.3f, 0f));

        if (!introFinished) return;

        Vector3 viewportPos = new Vector3(bossViewportX, 1f - bossViewportYFromTop, Mathf.Abs(mainCam.transform.position.z));
        Vector3 worldPos = mainCam.ViewportToWorldPoint(viewportPos);

        float halfWidth = arenaWidth * 0.5f;
        worldPos.x = Mathf.Clamp(worldPos.x, arenaCenterX - halfWidth, arenaCenterX + halfWidth);
        worldPos.z = boss.position.z;

        boss.position = Vector3.Lerp(boss.position, worldPos, 1f - Mathf.Exp(-bossFollowLerp * Time.deltaTime));
    }

    private IEnumerator FightRoutine()
    {
        if (introCutscene != null && lastPlayer != null)
        {
            yield return introCutscene.Play(lastPlayer);
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }

        introFinished = true;

        while (fightActive && (!leftArm.IsDead || !rightArm.IsDead))
        {
            yield return new WaitForSeconds(attackInterval);
            
            if (!fightActive) break;

            int attackType = Random.Range(1, 4);
            bool useLeft = Random.value > 0.5f;
            
            if (leftArm.IsDead) useLeft = false;
            if (rightArm.IsDead) useLeft = true;
            
            float floorY = arenaBottomY + 0.5f;
            float sweepY = arenaBottomY + 0.5f;
            float arenaLeft = arenaCenterX - arenaWidth * 0.45f;
            float arenaRight = arenaCenterX + arenaWidth * 0.45f;

            if (attackType == 1)
            {
                if (useLeft && !leftArm.IsDead)
                    leftArm.DoAttack1(arenaLeft, arenaRight, sweepY, sweepAttackDuration);
                else if (!rightArm.IsDead)
                    rightArm.DoAttack1(arenaRight, arenaLeft, sweepY, sweepAttackDuration);
            }
            else if (attackType == 2)
            {

                float targetX = lastPlayer != null ? lastPlayer.position.x : arenaCenterX;
                float startY = sweepY + 4f;
                
                if (useLeft && !leftArm.IsDead)
                    leftArm.DoAttack2(targetX, startY, floorY, smashAttackDuration);
                else if (!rightArm.IsDead)
                    rightArm.DoAttack2(targetX, startY, floorY, smashAttackDuration);
            }
            else if (attackType == 3)
            {
                
                if (!leftArm.IsDead && !rightArm.IsDead)
                {
                    float meetX = lastPlayer != null ? lastPlayer.position.x : arenaCenterX;
                    leftArm.DoAttack3(arenaLeft, meetX - 1f, sweepY, dualSweepDuration);
                    rightArm.DoAttack3(arenaRight, meetX + 1f, sweepY, dualSweepDuration);
                }
                else
                {

                    if (useLeft && !leftArm.IsDead)
                        leftArm.DoAttack1(arenaLeft, arenaRight, sweepY, sweepAttackDuration);
                    else if (!rightArm.IsDead)
                        rightArm.DoAttack1(arenaRight, arenaLeft, sweepY, sweepAttackDuration);
                }
            }

            yield return new WaitForSeconds(3.5f); 
        }
    }

    private void OnArmDied(FinalBossArm arm)
    {
        anim.SetTrigger("Hurt");
        if (leftArm.IsDead && rightArm.IsDead)
        {
            bossDefeated = true;
            fightActive = false;
            StartCoroutine(BossDefeatSequence());
        }
    }

    private IEnumerator BossDefeatSequence()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(5000);
        }

        if (boss != null)
        {
            SpriteRenderer bossSr = boss.GetComponent<SpriteRenderer>();
            Color originalColor = bossSr != null ? bossSr.color : Color.white;
            Vector3 originalScale = boss.localScale;
            Vector3 basePos = boss.position;

            float elapsed = 0f;

            yield return new WaitForSeconds(0.5f);
            mainbody.bodyDeath();

            boss.position = basePos;

            yield return new WaitForSeconds(2f);
            elapsed = 0f;
            boss.gameObject.SetActive(false);
        }

        yield return null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.8f, 0.2f, 0.8f, 0.4f);
        Vector3 center = new Vector3(arenaCenterX, (arenaTopY + arenaBottomY) * 0.5f, 0f);
        Vector3 size = new Vector3(arenaWidth, arenaTopY - arenaBottomY, 0f);
        Gizmos.DrawWireCube(center, size);
    }
}
