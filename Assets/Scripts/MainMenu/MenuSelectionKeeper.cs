using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class MenuSelectionKeeper : MonoBehaviour
{
    [SerializeField] private GameObject firstSelected;

    private Canvas menuCanvas;
    private GameObject lastActivePanel;
    private bool hasFixedHighlighters;

    private void Start()
    {
        menuCanvas = FindFirstObjectByType<Canvas>();
        AddHighlightersToAll();

        lastActivePanel = GetCurrentActivePanel();
        FixNavigationForPanel(lastActivePanel);
        Select(firstSelected);
    }

    private void Update()
    {
        EventSystem es = EventSystem.current;
        if (es == null) return;

        GameObject activePanel = GetCurrentActivePanel();
        if (activePanel != lastActivePanel)
        {
            lastActivePanel = activePanel;
            FixNavigationForPanel(activePanel);
            SelectFirstIn(activePanel);
            return;
        }

        if (es.currentSelectedGameObject == null || !es.currentSelectedGameObject.activeInHierarchy)
        {
            GameObject target = GetBestSelectable();
            if (target != null) Select(target);
        }
    }

    private void AddHighlightersToAll()
    {
        if (hasFixedHighlighters || menuCanvas == null) return;
        hasFixedHighlighters = true;

        foreach (var sel in menuCanvas.GetComponentsInChildren<Selectable>(true))
        {
            if (sel.GetComponent<SelectionHighlighter>() == null)
                sel.gameObject.AddComponent<SelectionHighlighter>();
        }
    }

    private void FixNavigationForPanel(GameObject panel)
    {
        if (panel == null) return;

        var selectables = panel.GetComponentsInChildren<Selectable>(false);
        var active = new List<Selectable>();
        foreach (var s in selectables)
        {
            if (s.interactable && s.gameObject.activeInHierarchy)
                active.Add(s);
        }

        if (active.Count == 0) return;

        var rows = new List<List<Selectable>>();
        float lastY = float.MaxValue;

        foreach (var s in active)
        {
            float y = GetWorldY(s);
            bool addedToRow = false;

            for (int r = 0; r < rows.Count; r++)
            {
                float rowY = GetWorldY(rows[r][0]);
                if (Mathf.Abs(y - rowY) < 30f)
                {
                    rows[r].Add(s);
                    addedToRow = true;
                    break;
                }
            }

            if (!addedToRow)
            {
                var newRow = new List<Selectable>();
                newRow.Add(s);
                rows.Add(newRow);
            }
        }

        rows.Sort((a, b) => GetWorldY(b[0]).CompareTo(GetWorldY(a[0])));

        foreach (var row in rows)
        {
            row.Sort((a, b) => GetWorldX(a).CompareTo(GetWorldX(b)));
        }

        for (int r = 0; r < rows.Count; r++)
        {
            var row = rows[r];

            for (int c = 0; c < row.Count; c++)
            {
                Selectable sel = row[c];

                if (sel is Slider)
                {
                    Navigation nav = new Navigation { mode = Navigation.Mode.Vertical };
                    sel.navigation = nav;
                    continue;
                }

                Navigation n = new Navigation { mode = Navigation.Mode.Explicit };

                if (row.Count > 1)
                {
                    if (c > 0) n.selectOnLeft = row[c - 1];
                    if (c < row.Count - 1) n.selectOnRight = row[c + 1];
                }

                if (r > 0)
                {
                    var aboveRow = rows[r - 1];
                    int idx = Mathf.Clamp(c, 0, aboveRow.Count - 1);
                    n.selectOnUp = aboveRow[idx];
                }

                if (r < rows.Count - 1)
                {
                    var belowRow = rows[r + 1];
                    int idx = Mathf.Clamp(c, 0, belowRow.Count - 1);
                    n.selectOnDown = belowRow[idx];
                }

                sel.navigation = n;
            }
        }
    }

    private float GetWorldY(Selectable s)
    {
        RectTransform rt = s.transform as RectTransform;
        if (rt == null) return 0f;
        return rt.position.y;
    }

    private float GetWorldX(Selectable s)
    {
        RectTransform rt = s.transform as RectTransform;
        if (rt == null) return 0f;
        return rt.position.x;
    }

    private void SelectFirstIn(GameObject panel)
    {
        if (panel == null) return;

        foreach (var s in panel.GetComponentsInChildren<Selectable>(false))
        {
            if (s.interactable && s.gameObject.activeInHierarchy)
            {
                Select(s.gameObject);
                return;
            }
        }
    }

    private GameObject GetCurrentActivePanel()
    {
        if (menuCanvas == null) return null;

        Transform ct = menuCanvas.transform;
        GameObject result = null;

        for (int i = 0; i < ct.childCount; i++)
        {
            GameObject child = ct.GetChild(i).gameObject;
            if (!child.activeInHierarchy) continue;
            if (child.GetComponentInChildren<Selectable>(false) == null) continue;
            result = child;
        }

        return result;
    }

    private GameObject GetBestSelectable()
    {
        if (lastActivePanel != null)
        {
            foreach (var s in lastActivePanel.GetComponentsInChildren<Selectable>(false))
            {
                if (s.interactable && s.gameObject.activeInHierarchy)
                    return s.gameObject;
            }
        }

        if (firstSelected != null && firstSelected.activeInHierarchy)
            return firstSelected;

        var all = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (var b in all)
        {
            if (b.interactable && b.gameObject.activeInHierarchy)
                return b.gameObject;
        }

        return null;
    }

    public void SetFirstSelected(GameObject go)
    {
        firstSelected = go;
        Select(go);
    }

    private void Select(GameObject go)
    {
        if (go == null || EventSystem.current == null) return;
        if (!go.activeInHierarchy) return;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(go);
    }
}
