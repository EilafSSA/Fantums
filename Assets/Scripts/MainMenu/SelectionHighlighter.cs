using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectionHighlighter : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private float outlineThickness = 4f;

    private Outline currentOutline;
    private GameObject outlineTarget;
    private static bool globalInitialBlock = true;

    private void Awake()
    {
        globalInitialBlock = true;
        Invoke(nameof(ReleaseInitialBlock), 0.1f);
    }

    private void ReleaseInitialBlock()
    {
        globalInitialBlock = false;
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (!globalInitialBlock && UIAudioManager.Instance != null)
        {
            UIAudioManager.Instance.PlayHover();
        }

        outlineTarget = FindOutlineTarget();
        if (outlineTarget == null) return;

        currentOutline = outlineTarget.GetComponent<Outline>();
        if (currentOutline == null)
            currentOutline = outlineTarget.AddComponent<Outline>();

        currentOutline.effectColor = highlightColor;
        currentOutline.effectDistance = new Vector2(outlineThickness, outlineThickness);
        currentOutline.enabled = true;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        ClearOutline();
    }

    private GameObject FindOutlineTarget()
    {
        if (GetComponent<Graphic>() != null)
            return gameObject;

        var graphic = GetComponentInChildren<Graphic>();
        if (graphic != null)
            return graphic.gameObject;

        return null;
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
