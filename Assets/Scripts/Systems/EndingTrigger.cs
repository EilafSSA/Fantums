using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndingTrigger : MonoBehaviour
{
    [Header("=== References ===")]
    public PlayerController playeranim;

    [Header("=== Waypoints (FIXED!) ===")]
    [SerializeField] private List<EndingWaypoint> customWaypoints = new List<EndingWaypoint>();

    [Header("=== Cinematic Bars ===")]
    [SerializeField] private bool showCinematicBars = true;
    [SerializeField] private float barHeightPercent = 0.12f;
    [SerializeField] private float barSlideTime = 0.5f;
    [SerializeField] private Color barColor = Color.black;

    [Header("=== Ending Music Settings ===")]
    [SerializeField] private AudioClip endingThemeMusic;
    [Tooltip("How long it takes for the heavy boss music to fade to complete silence.")]
    [SerializeField] private float bossMusicFadeOutTime = 2.0f; 
    [Tooltip("How long it takes for the new emotional ending theme to reach full volume.")]
    [SerializeField] private float endingMusicFadeInTime = 2.5f;

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

        // Validate the player or the entering object to trigger the final sequence
        if (player != null || other.CompareTag("Player"))
        {
            // Fallback assignment just in case the manual reference was left blank
            if (player == null) player = other.GetComponent<PlayerController>();
            if (player == null) return; 

            triggered = true;

            // --- FIXED WAYPOINT CONVERSION SYSTEM ---
            List<EndingWaypoint> worldWaypoints = new List<EndingWaypoint>();
            if (customWaypoints != null && customWaypoints.Count > 0)
            {
                foreach (var wp in customWaypoints)
                {
                    EndingWaypoint newWp = new EndingWaypoint();
                    newWp.waypointName = wp.waypointName;
                    // Fixes the relative stacking math bug by grabbing the trigger origin cleanly
                    newWp.position = (Vector2)transform.position + wp.position; 
                    newWp.speed = wp.speed;
                    newWp.pauseDuration = wp.pauseDuration;
                    newWp.playRunningAnim = wp.playRunningAnim;
                    
                    worldWaypoints.Add(newWp);
                }
            }

            // --- CINEMATIC MUSIC CROSSFADE INJECTION ---
            if (AudioManager.Instance != null && endingThemeMusic != null)
            {
                // Uses the customized long fade values to switch from boss tension to the ending theme
                AudioManager.Instance.SwitchMusic(endingThemeMusic, bossMusicFadeOutTime, endingMusicFadeInTime);
            }
            else if (endingThemeMusic == null)
            {
                Debug.LogWarning($"EndingTrigger on {gameObject.name} is missing its Ending Theme Music asset!");
            }

            // --- CINEMATIC BARS ---
            if (showCinematicBars)
            {
                BuildUI();
                StartCoroutine(SlideBars(0f, 1f, barSlideTime));
            }

            // Send the corrected path list directly to the character engine
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