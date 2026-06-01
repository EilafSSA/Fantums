using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndingTrigger : MonoBehaviour
{
    [Header("=== References ===")]
    public PlayerController playeranim;

    [Header("=== Waypoints (doesn't flippinggg work idk how to fix it) ===")]
    [SerializeField] private List<EndingWaypoint> customWaypoints = new List<EndingWaypoint>();

    [Header("=== Cinematic Bars ===")]
    [SerializeField] private bool showCinematicBars = true;
    [SerializeField] private float barHeightPercent = 0.12f;
    [SerializeField] private float barSlideTime = 0.5f;
    [SerializeField] private Color barColor = Color.black;

    private bool triggered = false;
    private Canvas canvas;
    private RectTransform topBar;
    private RectTransform bottomBar;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;

        PlayerController player = playeranim;
        if (player == null)
        {
            player = other.GetComponent<PlayerController>();
            if (player == null)
                player = other.GetComponentInParent<PlayerController>();
        }

        if (player != null && customWaypoints != null && customWaypoints.Count > 0)
        {
            triggered = true;

            List<EndingWaypoint> worldWaypoints = new List<EndingWaypoint>();
            foreach (var wp in customWaypoints)
            {
                worldWaypoints.Add(new EndingWaypoint
                {
                    waypointName = wp.waypointName,
                    position = (Vector2)transform.position + wp.position,
                    speed = wp.speed,
                    pauseDuration = wp.pauseDuration,
                    playRunningAnim = wp.playRunningAnim
                });
            }

            if (showCinematicBars)
            {
                BuildUI();
                StartCoroutine(SlideBars(0f, 1f, barSlideTime));
            }

            player.StartEndingCutscene(worldWaypoints);
        }
    }

    private void BuildUI()
    {
        GameObject canvasGo = new GameObject("EndingCinematicCanvas");
        canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
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

    private System.Collections.IEnumerator SlideBars(float from, float to, float duration)
    {
        float barH = topBar.sizeDelta.y;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float lerped = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));
            topBar.anchoredPosition = new Vector2(0f, barH * (1f - lerped));
            bottomBar.anchoredPosition = new Vector2(0f, -barH * (1f - lerped));
            yield return null;
        }
        topBar.anchoredPosition = new Vector2(0f, barH * (1f - to));
        bottomBar.anchoredPosition = new Vector2(0f, -barH * (1f - to));
    }

    private void OnDrawGizmosSelected()
    {
        if (customWaypoints == null || customWaypoints.Count == 0) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < customWaypoints.Count; i++)
        {
            Vector3 pos = transform.position + new Vector3(customWaypoints[i].position.x, customWaypoints[i].position.y, 0f);
            Gizmos.DrawWireSphere(pos, 0.4f);

            if (i > 0)
            {
                Vector3 prevPos = transform.position + new Vector3(customWaypoints[i - 1].position.x, customWaypoints[i - 1].position.y, 0f);
                Gizmos.DrawLine(prevPos, pos);
            }
            else
            {
                Gizmos.DrawLine(transform.position, pos);
            }
        }
    }
}