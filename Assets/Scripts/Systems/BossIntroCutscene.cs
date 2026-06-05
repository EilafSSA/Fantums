using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossIntroCutscene : MonoBehaviour
{
    [Header("=== References ===")]
    [SerializeField] private GameObject peopleSpawnerObject; // Optional reference to the PeopleSpawner in the boss room, used to disable it during the cutscene if assigned.
    [SerializeField] private Transform bossTransform;
    [SerializeField] private Animator bossAnimator;
    [Tooltip("Animator trigger fired on the boss when the camera reaches him.")]
    [SerializeField] private string bossIntroTrigger = "IntroAnim";

    [Header("=== Name Card ===")]
    [SerializeField] private string bossName = "Train Operator";
    [SerializeField] private float nameFontSize = 52f;
    [SerializeField] private Color nameColor = Color.white;

    [Header("=== Timing ===")]
    [SerializeField] private float barSlideTime = 0.35f;
    [SerializeField] private float panToBossTime = 0.8f;
    [SerializeField] private float nameShowDelay = 0.2f;
    [SerializeField] private float nameFadeInTime = 0.3f;
    [SerializeField] private float nameHoldTime = 1.4f;
    [SerializeField] private float bossAnimHoldTime = 0.8f;
    [SerializeField] private float nameFadeOutTime = 0.25f;
    [SerializeField] private float panBackTime = 0.6f;
    [SerializeField] private float barRetractTime = 0.3f;

    [Header("=== Visual ===")]
    [Tooltip("Height of each cinematic bar as a percentage.")]
    [SerializeField] private float barHeightPercent = 0.1f;
    [SerializeField] private Color barColor = Color.black;
    [SerializeField] private float cameraPanSmooth = 5f;

    [Header("=== Boss Music Trigger Settings ===")]
    [SerializeField] private AudioClip bossBattleTheme;

    private Canvas canvas;
    private RectTransform topBar;
    private RectTransform bottomBar;
    private TMP_Text nameText;
    private CanvasGroup nameGroup;

    private bool isPlaying;
    private bool musicTriggered = false; // Prevents overlapping trigger issues if walked over multiple times

    public Coroutine Play(Transform playerTransform)
    {
        if (isPlaying) return null;
        return StartCoroutine(CutsceneRoutine(playerTransform));
    }

    private IEnumerator CutsceneRoutine(Transform playerTransform)
    {
        isPlaying = true;

        // --- AUDIO TRANSITION INJECTION ---
        if (!musicTriggered)
        {
            musicTriggered = true;

            // 1. Kill all standard background level footsteps instantly
            EnemyAudio.StopAllEnemyFootsteps();

            // 2. Command AudioManager to fade out the previous track and fade in the fight theme
            if (AudioManager.Instance != null && bossBattleTheme != null)
            {
                AudioManager.Instance.SwitchToBossMusic(bossBattleTheme, 0.5f, 1.0f);
            }
            else if (bossBattleTheme == null)
            {
                Debug.LogWarning($"BossIntroCutscene on {gameObject.name}: Boss Battle Theme clip field is empty in the Inspector!");
            }
        }

        Rigidbody2D playerRb = playerTransform != null ? playerTransform.GetComponent<Rigidbody2D>() : null;
        PlayerController pc = playerTransform != null ? playerTransform.GetComponent<PlayerController>() : null;
        bool pcWasEnabled = pc != null && pc.enabled;
        if (pc != null) pc.enabled = false;
        if (playerRb != null) playerRb.linearVelocity = Vector2.zero;

        BuildUI();

        yield return SlideBars(0f, 1f, barSlideTime);

        if (CameraFollow.Instance != null && bossTransform != null)
            CameraFollow.Instance.FocusOn(bossTransform, cameraPanSmooth);

        yield return new WaitForSeconds(panToBossTime);

        yield return new WaitForSeconds(nameShowDelay);
        yield return FadeName(0f, 1f, nameFadeInTime);

        if (bossAnimator != null && !string.IsNullOrEmpty(bossIntroTrigger))
            bossAnimator.SetTrigger(bossIntroTrigger);

        yield return new WaitForSeconds(bossAnimHoldTime);

        yield return new WaitForSeconds(nameHoldTime);

        yield return FadeName(1f, 0f, nameFadeOutTime);

        if (CameraFollow.Instance != null)
            CameraFollow.Instance.ReturnToPlayer(cameraPanSmooth);

        yield return new WaitForSeconds(panBackTime);

        yield return SlideBars(1f, 0f, barRetractTime);

        if (pc != null) pc.enabled = pcWasEnabled;
        DestroyUI();
        isPlaying = false;
    }

    private void BuildUI()
    {
        GameObject canvasGo = new GameObject("BossIntroCanvas");
        canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasGo.AddComponent<GraphicRaycaster>();

        float barH = Screen.height * barHeightPercent;

        topBar = MakeBar("TopBar", canvas.transform);
        topBar.anchorMin = new Vector2(0f, 1f);
        topBar.anchorMax = new Vector2(1f, 1f);
        topBar.pivot = new Vector2(0.5f, 1f);
        topBar.sizeDelta = new Vector2(0f, barH);
        topBar.anchoredPosition = new Vector2(0f, barH);

        bottomBar = MakeBar("BottomBar", canvas.transform);
        bottomBar.anchorMin = new Vector2(0f, 0f);
        bottomBar.anchorMax = new Vector2(1f, 0f);
        bottomBar.pivot = new Vector2(0.5f, 0f);
        bottomBar.sizeDelta = new Vector2(0f, barH);
        bottomBar.anchoredPosition = new Vector2(0f, -barH);

        GameObject nameGo = new GameObject("BossNameText");
        nameGo.transform.SetParent(canvas.transform, false);
        nameText = nameGo.AddComponent<TextMeshProUGUI>();
        nameText.text = bossName;
        nameText.fontSize = nameFontSize;
        nameText.color = nameColor;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.fontStyle = FontStyles.Bold;

        RectTransform nameRect = nameText.rectTransform;
        nameRect.anchorMin = new Vector2(0f, 0f);
        nameRect.anchorMax = new Vector2(1f, 0f);
        nameRect.pivot = new Vector2(0.5f, 0f);
        nameRect.sizeDelta = new Vector2(0f, 80f);
        nameRect.anchoredPosition = new Vector2(0f, barH + 20f);

        nameGroup = nameGo.AddComponent<CanvasGroup>();
        nameGroup.alpha = 0f;
    }

    private RectTransform MakeBar(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = barColor;
        img.raycastTarget = false;
        return go.GetComponent<RectTransform>();
    }

    private void DestroyUI()
    {
        if (canvas != null)
            Destroy(canvas.gameObject);
        canvas = null;
        topBar = null;
        bottomBar = null;
        nameText = null;
        nameGroup = null;
    }

    private IEnumerator SlideBars(float from, float to, float duration)
    {
        float barH = topBar.sizeDelta.y;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float lerped = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));
            topBar.anchoredPosition = new Vector2(0f, barH * (1f - lerped));
            bottomBar.anchoredPosition = new Vector2(0f, -barH * (1f - lerped));
            yield return null;
        }
        float final01 = to;
        topBar.anchoredPosition = new Vector2(0f, barH * (1f - final01));
        bottomBar.anchoredPosition = new Vector2(0f, -barH * (1f - final01));
    }

    private IEnumerator FadeName(float from, float to, float duration)
    {
        if (nameGroup == null) yield break;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            nameGroup.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }
        nameGroup.alpha = to;
    }
}