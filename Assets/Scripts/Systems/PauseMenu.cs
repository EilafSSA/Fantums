using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PauseMenu : MonoBehaviour
{
    [Header("=== Settings ===")]
    [SerializeField] private int mainMenuBuildIndex = 0;

    [Header("=== Rebind actions to expose ===")]
    [SerializeField] private string[] rebindActions = { "Jump", "Attack", "Sprint", "Interact", "Cling" };
    [SerializeField] private int[] rebindBindingIndices = { 1, 0, 1, 1, 0 };
    [SerializeField] private string[] rebindDisplayNames = { "Jump", "Attack", "Dash", "Interact", "Cling" };

    private bool isOpen;
    private GameObject root;
    private GameObject pausePanel;
    private GameObject optionsPanel;
    private readonly List<RebindActionUI> rows = new List<RebindActionUI>();

    private Button resumeButton;
    private Button optionsBackButton;
    private Button optionsFirstButton;

    private void Update()
    {
        if (TogglePressed())
        {
            if (isOpen) Resume();
            else OpenPause();
        }

        if (isOpen)
        {
            EventSystem es = EventSystem.current;
            if (es != null && (es.currentSelectedGameObject == null || !es.currentSelectedGameObject.activeInHierarchy))
                SelectDefault();
        }
    }

    private bool TogglePressed()
    {
        bool kb = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        bool pad = Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame;
        return kb || pad;
    }

    private void OpenPause()
    {
        if (root == null) BuildUI();

        isOpen = true;
        root.SetActive(true);
        pausePanel.SetActive(true);
        optionsPanel.SetActive(false);
        Time.timeScale = 0f;
        SelectDefault();
    }

    public void Resume()
    {
        isOpen = false;
        Time.timeScale = 1f;
        if (root != null) root.SetActive(false);
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }

    public void OpenOptions()
    {
        pausePanel.SetActive(false);
        optionsPanel.SetActive(true);
        foreach (var r in rows) if (r != null) r.RefreshDisplay();
        SelectDefault();
    }

    public void BackToPause()
    {
        optionsPanel.SetActive(false);
        pausePanel.SetActive(true);
        if (InputManager.Instance != null) InputManager.Instance.SaveRebinds();
        SelectDefault();
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuBuildIndex);
    }

    private void SelectDefault()
    {
        if (EventSystem.current == null) return;

        GameObject target = null;
        if (optionsPanel != null && optionsPanel.activeInHierarchy)
        {
            if (optionsFirstButton != null)
                target = optionsFirstButton.gameObject;
            else if (optionsBackButton != null)
                target = optionsBackButton.gameObject;
        }
        else if (resumeButton != null)
            target = resumeButton.gameObject;

        if (target != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(target);
        }
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;

        EventSystem existing = FindFirstObjectByType<EventSystem>();
        if (existing != null) return;

        GameObject es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<InputSystemUIInputModule>();
    }

    private void BuildUI()
    {
        EnsureEventSystem();

        root = new GameObject("PauseCanvas");
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 900;
        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        root.AddComponent<GraphicRaycaster>();
        root.AddComponent<SelectionHighlighter>();

        GameObject dim = NewUI("Dim", root.transform);
        Stretch(dim.GetComponent<RectTransform>());
        dim.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.7f);

        pausePanel = NewUI("PausePanel", root.transform);
        Stretch(pausePanel.GetComponent<RectTransform>());

        Label(pausePanel.transform, "PAUSED", 60, new Vector2(0f, 200f), new Vector2(600f, 90f));
        resumeButton = MakeButton(pausePanel.transform, "Resume", new Vector2(0f, 80f));
        Button optionsBtn = MakeButton(pausePanel.transform, "Options", new Vector2(0f, 0f));
        Button quitBtn = MakeButton(pausePanel.transform, "Quit to Menu", new Vector2(0f, -80f));

        resumeButton.onClick.AddListener(Resume);
        optionsBtn.onClick.AddListener(OpenOptions);
        quitBtn.onClick.AddListener(QuitToMenu);

        LinkVertical(new[] { resumeButton, optionsBtn, quitBtn });

        optionsPanel = NewUI("OptionsPanel", root.transform);
        Stretch(optionsPanel.GetComponent<RectTransform>());
        optionsPanel.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.08f, 0.96f);

        Label(optionsPanel.transform, "CONTROLLER OPTIONS", 42, new Vector2(0f, 320f), new Vector2(900f, 70f));

        var rowButtons = new List<Button>();
        int count = Mathf.Min(rebindActions.Length, rebindBindingIndices.Length);
        for (int i = 0; i < count; i++)
        {
            string display = (rebindDisplayNames != null && i < rebindDisplayNames.Length) ? rebindDisplayNames[i] : rebindActions[i];
            RebindActionUI row = MakeRebindRow(optionsPanel.transform, rebindActions[i], rebindBindingIndices[i], display, 200f - i * 80f, out Button rowBtn);
            if (row != null) rows.Add(row);
            if (rowBtn != null) rowButtons.Add(rowBtn);
        }

        optionsFirstButton = rowButtons.Count > 0 ? rowButtons[0] : null;

        Button resetBtn = MakeButton(optionsPanel.transform, "Reset to Defaults", new Vector2(-160f, -320f));
        optionsBackButton = MakeButton(optionsPanel.transform, "Back", new Vector2(160f, -320f));
        resetBtn.onClick.AddListener(() =>
        {
            if (InputManager.Instance != null) InputManager.Instance.ResetAllRebinds();
            foreach (var r in rows) if (r != null) r.RefreshDisplay();
        });
        optionsBackButton.onClick.AddListener(BackToPause);

        LinkVertical(rowButtons.ToArray());

        Button lastRowBtn = rowButtons.Count > 0 ? rowButtons[rowButtons.Count - 1] : null;

        Navigation resetNav = new Navigation { mode = Navigation.Mode.Explicit };
        resetNav.selectOnUp = lastRowBtn;
        resetNav.selectOnRight = optionsBackButton;
        resetBtn.navigation = resetNav;

        Navigation backNav = new Navigation { mode = Navigation.Mode.Explicit };
        backNav.selectOnUp = lastRowBtn;
        backNav.selectOnLeft = resetBtn;
        optionsBackButton.navigation = backNav;

        if (lastRowBtn != null)
        {
            Navigation lastNav = lastRowBtn.navigation;
            lastNav.selectOnDown = resetBtn;
            lastRowBtn.navigation = lastNav;
        }

        optionsPanel.SetActive(false);
        root.SetActive(false);
    }

    private static void LinkVertical(Button[] buttons)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;
            Navigation nav = new Navigation { mode = Navigation.Mode.Explicit };
            if (i > 0) nav.selectOnUp = buttons[i - 1];
            if (i < buttons.Length - 1) nav.selectOnDown = buttons[i + 1];
            buttons[i].navigation = nav;
        }
    }

    private RebindActionUI MakeRebindRow(Transform parent, string action, int bindingIndex, string display, float y, out Button rebindBtn)
    {
        GameObject row = NewUI("Row_" + action, parent);
        RectTransform rt = row.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(820f, 70f);
        rt.anchoredPosition = new Vector2(0f, y);

        TMP_Text actionLabel = LabelRaw(row.transform, display, 28, new Vector2(-280f, 0f), new Vector2(320f, 60f), TextAlignmentOptions.Left);
        TMP_Text bindingLabel = LabelRaw(row.transform, "—", 26, new Vector2(60f, 0f), new Vector2(260f, 60f), TextAlignmentOptions.Center);

        GameObject btnGo = NewUI("RebindButton", row.transform);
        RectTransform brt = btnGo.GetComponent<RectTransform>();
        brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 0.5f);
        brt.pivot = new Vector2(0.5f, 0.5f);
        brt.sizeDelta = new Vector2(180f, 56f);
        brt.anchoredPosition = new Vector2(300f, 0f);
        btnGo.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.28f, 1f);
        rebindBtn = btnGo.AddComponent<Button>();
        ApplyYellowColors(rebindBtn);
        LabelRaw(btnGo.transform, "Rebind", 22, Vector2.zero, new Vector2(180f, 56f), TextAlignmentOptions.Center);

        GameObject overlay = NewUI("WaitingOverlay", row.transform);
        Stretch(overlay.GetComponent<RectTransform>());
        Image ov = overlay.AddComponent<Image>();
        ov.color = new Color(0f, 0f, 0f, 0.6f);
        ov.raycastTarget = false;
        overlay.SetActive(false);

        RebindActionUI rb = row.AddComponent<RebindActionUI>();
        rb.Configure(action, bindingIndex, "Gamepad", actionLabel, bindingLabel, rebindBtn, overlay, display);
        return rb;
    }

    private static GameObject NewUI(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    private static void Label(Transform parent, string text, float size, Vector2 pos, Vector2 sd)
    {
        LabelRaw(parent, text, size, pos, sd, TextAlignmentOptions.Center);
    }

    private static TMP_Text LabelRaw(Transform parent, string text, float size, Vector2 pos, Vector2 sd, TextAlignmentOptions align)
    {
        GameObject go = NewUI("Label", parent);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = sd;
        rt.anchoredPosition = pos;
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size; tmp.alignment = align; tmp.color = Color.white;
        return tmp;
    }

    private static Button MakeButton(Transform parent, string label, Vector2 pos)
    {
        GameObject go = NewUI("Button_" + label, parent);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(300f, 60f);
        rt.anchoredPosition = pos;
        go.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.28f, 1f);
        Button btn = go.AddComponent<Button>();
        ApplyYellowColors(btn);
        LabelRaw(go.transform, label, 26, Vector2.zero, new Vector2(300f, 60f), TextAlignmentOptions.Center);
        return btn;
    }

    private static void ApplyYellowColors(Button btn)
    {
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.2f, 0.2f, 0.28f, 1f);
        cb.highlightedColor = new Color(1f, 0.9f, 0.2f, 1f);
        cb.selectedColor = new Color(1f, 0.9f, 0.2f, 1f);
        cb.pressedColor = new Color(0.9f, 0.75f, 0.1f, 1f);
        cb.fadeDuration = 0.05f;
        btn.colors = cb;
    }
}
