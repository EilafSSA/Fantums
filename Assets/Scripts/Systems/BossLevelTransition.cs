using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BossLevelTransition : MonoBehaviour
{
    [Header("=== References ===")]
    [SerializeField] private ShadowBoss boss;

    [Header("=== Destination ===")]
    [SerializeField] private int nextSceneBuildIndex = 2;
    [SerializeField] private string nextSceneName = "Level2";

    [Header("=== Timing ===")]
    [SerializeField] private float watchDeathDuration = 2f;
    [SerializeField] private float barSlideTime = 0.6f;
    [SerializeField] private float starlightInTime = 1.2f;
    [SerializeField] private float holdWhiteTime = 0.4f;
    [Tooltip("Delay before audio triggers after watchDeathDuration ends")]
    [SerializeField] private float audioDelay = 0.5f; 

    [Header("=== Look ===")]
    [SerializeField] private float barHeightPercent = 0.14f;
    [SerializeField] private Color barColor = Color.black;
    [SerializeField] private Color starlightColor = Color.white;

    private bool triggered = false;

    private void Start()
    {
        if (boss == null) boss = FindFirstObjectByType<ShadowBoss>();
        if (boss != null) boss.OnBossDefeated += HandleBossDefeated;
    }

    private void OnDestroy()
    {
        if (boss != null) boss.OnBossDefeated -= HandleBossDefeated;
    }

    private void HandleBossDefeated()
    {
        if (triggered) return;
        triggered = true;

        TransitionRunner runner = TransitionRunner.Create(barColor, starlightColor, barHeightPercent);
        runner.Run(nextSceneBuildIndex, nextSceneName, watchDeathDuration, barSlideTime, starlightInTime, holdWhiteTime, audioDelay);
    }
}

public class TransitionRunner : MonoBehaviour
{
    private Canvas canvas;
    private RectTransform topBar, bottomBar;
    private Image starlight;
    private Color starColor;

    public static TransitionRunner Create(Color barColor, Color starlightColor, float barHeightPercent)
    {
        GameObject canvasGo = new GameObject("BossTransitionCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        DontDestroyOnLoad(canvasGo);
        TransitionRunner runner = canvasGo.AddComponent<TransitionRunner>();
        runner.canvas = canvas;
        runner.starColor = starlightColor;

        float barH = Screen.height * barHeightPercent;
        runner.topBar = MakeImage("TopBar", canvas.transform, barColor);
        runner.topBar.anchorMin = new Vector2(0f, 1f);
        runner.topBar.anchorMax = new Vector2(1f, 1f);
        runner.topBar.sizeDelta = new Vector2(0f, barH);
        runner.topBar.anchoredPosition = new Vector2(0f, barH);

        runner.bottomBar = MakeImage("BottomBar", canvas.transform, barColor);
        runner.bottomBar.anchorMin = new Vector2(0f, 0f);
        runner.bottomBar.anchorMax = new Vector2(1f, 0f);
        runner.bottomBar.sizeDelta = new Vector2(0f, barH);
        runner.bottomBar.anchoredPosition = new Vector2(0f, -barH);

        RectTransform starRect = MakeImage("Starlight", canvas.transform, new Color(starlightColor.r, starlightColor.g, starlightColor.b, 0f));
        starRect.anchorMin = Vector2.zero; starRect.anchorMax = Vector2.one;
        starRect.offsetMin = Vector2.zero; starRect.offsetMax = Vector2.zero;
        runner.starlight = starRect.GetComponent<Image>();

        return runner;
    }

    public void Run(int bIdx, string sName, float wDeath, float bSlide, float sIn, float hWhite, float aDelay)
    {
        StartCoroutine(Routine(bIdx, sName, wDeath, bSlide, sIn, hWhite, aDelay));
    }

    private IEnumerator Routine(int bIdx, string sName, float wDeath, float bSlide, float sIn, float hWhite, float aDelay)
    {
        // 1. Initial visual start
        StartCoroutine(SlideBars(0f, 1f, bSlide));
        yield return new WaitForSecondsRealtime(wDeath);

        // 2. Trigger audio as an independent task so the UI does NOT pause
        StartCoroutine(TriggerAudioDelayed(aDelay));

        // 3. Starlight Fade In (Continues immediately without waiting for audioDelay)
        Color c = starColor;
        float elapsed = 0f;
        while (elapsed < sIn)
        {
            elapsed += Time.unscaledDeltaTime;
            c.a = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / sIn));
            if (starlight != null) starlight.color = c;
            yield return null;
        }

        yield return new WaitForSecondsRealtime(hWhite);

        // 4. Load Scene
        AsyncOperation load = LoadNext(bIdx, sName);
        if (load != null) while (!load.isDone) yield return null;
        
        Destroy(gameObject);
    }

    private IEnumerator TriggerAudioDelayed(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayTransitionSound();
            AudioManager.Instance.PlayVictoryMusic(0.5f, 2.0f);
        }
    }

    private AsyncOperation LoadNext(int bIdx, string sName)
    {
        if (bIdx >= 0 && bIdx < SceneManager.sceneCountInBuildSettings) return SceneManager.LoadSceneAsync(bIdx);
        if (!string.IsNullOrEmpty(sName)) return SceneManager.LoadSceneAsync(sName);
        return null;
    }

    private IEnumerator SlideBars(float from, float to, float dur)
    {
        float barH = topBar.sizeDelta.y;
        float elapsed = 0f;
        while (elapsed < dur)
        {
            elapsed += Time.unscaledDeltaTime;
            float lerped = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, elapsed / dur));
            topBar.anchoredPosition = new Vector2(0f, barH * (1f - lerped));
            bottomBar.anchoredPosition = new Vector2(0f, -barH * (1f - lerped));
            yield return null;
        }
    }

    private static RectTransform MakeImage(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return go.GetComponent<RectTransform>();
    }
}