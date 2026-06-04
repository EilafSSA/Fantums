using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectionHighlighter : MonoBehaviour
{
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private float outlineThickness = 4f;

    private GameObject current;
    private Outline currentOutline;

    private void Update()
    {
        EventSystem es = EventSystem.current;
        if (es == null) return;

        GameObject selected = es.currentSelectedGameObject;
        if (selected == current) return;

        ClearOutline();

        current = selected;
        if (current == null) return;

        currentOutline = current.GetComponent<Outline>();
        if (currentOutline == null)
            currentOutline = current.AddComponent<Outline>();

        currentOutline.effectColor = highlightColor;
        currentOutline.effectDistance = new Vector2(outlineThickness, outlineThickness);
        currentOutline.enabled = true;
    }

    private void ClearOutline()
    {
        if (currentOutline != null)
            currentOutline.enabled = false;
        currentOutline = null;
    }

    private void OnDisable()
    {
        ClearOutline();
        current = null;
    }
}
