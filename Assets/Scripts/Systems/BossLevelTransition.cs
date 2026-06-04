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

    [Header("=== Look ===")]
    [SerializeField] private float barHeightPercent = 0.14f;
    [SerializeField] private Color barColor = Color.black;
    [SerializeField] private Color starlightColor = Color.white;

    private bool triggered = false;

    private void Start()
    {
        if (boss == null)
            boss = FindFirstObjectByType<ShadowBoss>();

        if (boss != null)
            boss.OnBossDefeated += HandleBossDefeated;
        else
            Debug.LogWarning("[BossLevelTransition] No ShadowBoss found to listen to.");
    }

    private void OnDestroy()
    {
        if (boss != null)
            boss.OnBossDefeated -= HandleBossDefeated;
    }

    private void HandleBossDefeated()
    {
        if (triggered) return;
        triggered = true;

        TransitionRunner runner = TransitionRunner.Create(barColor, starlightColor, barHeightPercent);
        runner.Run(nextSceneBuildIndex, nextSceneName, watchDeathDuration, barSlideTime, starlightInTime, holdWhiteTime);
    }
}

public class TransitionRunner : MonoBehaviour
{
    private Canvas canvas;
    private RectTransform topBar;
    private RectTransform bottomBar;
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

        canvasGo.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad(canvasGo);

        TransitionRunner runner = canvasGo.AddComponent<TransitionRunner>();
        runner.canvas = canvas;
        runner.starColor = starlightColor;

        float barH = Screen.height * barHeightPercent;

        runner.topBar = MakeImage("TopBar", canvas.transform, barColor);
        runner.topBar.anchorMin = new Vector2(0f, 1f);
        runner.topBar.anchorMax = new Vector2(1f, 1f);
        runner.topBar.pivot = new Vector2(0.5f, 1f);
        runner.topBar.sizeDelta = new Vector2(0f, barH);
        runner.topBar.anchoredPosition = new Vector2(0f, barH);

        runner.bottomBar = MakeImage("BottomBar", canvas.transform, barColor);
        runner.bottomBar.anchorMin = new Vector2(0f, 0f);
        runner.bottomBar.anchorMax = new Vector2(1f, 0f);
        runner.bottomBar.pivot = new Vector2(0.5f, 0f);
        runner.bottomBar.sizeDelta = new Vector2(0f, barH);
        runner.bottomBar.anchoredPosition = new Vector2(0f, -barH);

        RectTransform starRect = MakeImage("Starlight", canvas.transform,
            new Color(starlightColor.r, starlightColor.g, starlightColor.b, 0f));
        starRect.anchorMin = Vector2.zero;
        starRect.anchorMax = Vector2.one;
        starRect.offsetMin = Vector2.zero;
        starRect.offsetMax = Vector2.zero;
        runner.starlight = starRect.GetComponent<Image>();

        return runner;
    }

    public void Run(int buildIndex, string sceneName, float watchDeath, float barSlide, float starlightIn, float holdWhite)
    {
        StartCoroutine(Routine(buildIndex, sceneName, watchDeath, barSlide, starlightIn, holdWhite));
    }

    private IEnumerator Routine(int buildIndex, string sceneName, float watchDeath, float barSlide, float starlightIn, float holdWhite)
    {
        StartCoroutine(SlideBars(0f, 1f, barSlide));
        yield return new WaitForSecondsRealtime(watchDeath);

        Color c = starColor;
        float elapsed = 0f;
        while (elapsed < starlightIn)
        {
            elapsed += Time.unscaledDeltaTime;
            c.a = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / starlightIn));
            starlight.color = c;
            yield return null;
        }
        c.a = 1f;
        starlight.color = c;

        yield return new WaitForSecondsRealtime(holdWhite);

        AsyncOperation load = LoadNext(buildIndex, sceneName);
        if (load != null)
        {
            while (!load.isDone)
                yield return null;
        }

        yield return null;

        elapsed = 0f;
        while (elapsed < starlightIn)
        {
            elapsed += Time.unscaledDeltaTime;
            c.a = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / starlightIn));
            if (starlight != null) starlight.color = c;
            yield return null;
        }

        Destroy(gameObject);
    }

    private AsyncOperation LoadNext(int buildIndex, string sceneName)
    {
        if (buildIndex >= 0 && buildIndex < SceneManager.sceneCountInBuildSettings)
            return SceneManager.LoadSceneAsync(buildIndex);
        if (!string.IsNullOrEmpty(sceneName))
            return SceneManager.LoadSceneAsync(sceneName);
        Debug.LogError("[TransitionRunner] No valid next scene set.");
        return null;
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
        topBar.anchoredPosition = new Vector2(0f, barH * (1f - to));
        bottomBar.anchoredPosition = new Vector2(0f, -barH * (1f - to));
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
