using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectionHighlighter : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private float outlineThickness = 4f;

    private Outline currentOutline;
    private static bool globalInitialBlock = true;

    private void Awake()
    {
        // Reset the boot blocker when the scene first loads
        globalInitialBlock = true;
        Invoke(nameof(ReleaseInitialBlock), 0.1f);
    }

    private void ReleaseInitialBlock()
    {
        globalInitialBlock = false;
    }

    // Unity Event System automatically executes this EXACTLY ONCE when this specific object is highlighted
    public void OnSelect(BaseEventData eventData)
    {
        // 1. Trigger the Hover sound safely (blocking the first frame boot noise)
        if (!globalInitialBlock && UIAudioManager.Instance != null)
        {
            UIAudioManager.Instance.PlayHover();
        }

        // 2. Visual Outline generation
        currentOutline = GetComponent<Outline>();
        if (currentOutline == null)
            currentOutline = gameObject.AddComponent<Outline>();

        currentOutline.effectColor = highlightColor;
        currentOutline.effectDistance = new Vector2(outlineThickness, outlineThickness);
        currentOutline.enabled = true;
    }

    // Unity Event System automatically executes this EXACTLY ONCE when selection moves away
    public void OnDeselect(BaseEventData eventData)
    {
        ClearOutline();
    }

    private void ClearOutline()
    {
        if (currentOutline != null)
            currentOutline.enabled = false;
    }

    private void OnDisable()
    {
        ClearOutline();
    }
}