using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class RebindActionUI : MonoBehaviour
{
    [Header("=== Action ===")]
    [SerializeField] private string actionName = "Jump";
    [SerializeField] private int bindingIndex = 0;
    [SerializeField] private string bindingGroup = "Gamepad";
    [SerializeField] private string displayName = "";

    [Header("=== UI ===")]
    [SerializeField] private TMP_Text actionLabel;
    [SerializeField] private TMP_Text bindingLabel;
    [SerializeField] private Button rebindButton;
    [SerializeField] private GameObject waitingOverlay;

    private InputAction action;
    private InputActionRebindingExtensions.RebindingOperation rebindOperation;

    public void Configure(string action, int bindingIndex, string bindingGroup,
        TMP_Text actionLabel, TMP_Text bindingLabel, Button rebindButton, GameObject waitingOverlay,
        string displayName = "")
    {
        this.actionName = action;
        this.bindingIndex = bindingIndex;
        this.bindingGroup = bindingGroup;
        this.displayName = displayName;
        this.actionLabel = actionLabel;
        this.bindingLabel = bindingLabel;
        this.rebindButton = rebindButton;
        this.waitingOverlay = waitingOverlay;

        if (isActiveAndEnabled)
        {
            ResolveAction();
            RefreshDisplay();
            if (this.rebindButton != null)
            {
                this.rebindButton.onClick.RemoveListener(StartRebind);
                this.rebindButton.onClick.AddListener(StartRebind);
            }
        }
    }

    private void OnEnable()
    {
        ResolveAction();
        RefreshDisplay();

        if (rebindButton != null)
        {
            rebindButton.onClick.RemoveListener(StartRebind);
            rebindButton.onClick.AddListener(StartRebind);
        }
    }

    private void OnDisable()
    {
        if (rebindButton != null)
            rebindButton.onClick.RemoveListener(StartRebind);

        CleanUpOperation();
    }

    private void ResolveAction()
    {
        if (InputManager.Instance == null || InputManager.Instance.Actions == null)
        {
            action = null;
            return;
        }

        var map = InputManager.Instance.Actions.FindActionMap("Player", false);
        if (map == null) { action = null; return; }

        action = map.FindAction(actionName, false);

        if (actionLabel != null)
        {
            if (!string.IsNullOrEmpty(displayName))
                actionLabel.text = displayName;
            else
                actionLabel.text = string.IsNullOrEmpty(actionName) ? "?" : actionName;
        }
    }

    public void RefreshDisplay()
    {
        if (waitingOverlay != null)
            waitingOverlay.SetActive(false);

        if (action == null)
        {
            if (bindingLabel != null) bindingLabel.text = "—";
            return;
        }

        if (bindingLabel != null && bindingIndex >= 0 && bindingIndex < action.bindings.Count)
        {
            bindingLabel.text = action.GetBindingDisplayString(
                bindingIndex,
                InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
        }
    }

    public void StartRebind()
    {
        if (action == null)
        {
            ResolveAction();
            if (action == null) return;
        }

        if (bindingIndex < 0 || bindingIndex >= action.bindings.Count)
            return;

        if (waitingOverlay != null)
            waitingOverlay.SetActive(true);

        if (bindingLabel != null)
            bindingLabel.text = "Press a button...";

        action.Disable();

        rebindOperation = action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("<Mouse>/position")
            .WithControlsExcluding("<Mouse>/delta")
            .WithControlsExcluding("<Gamepad>/start")
            .WithCancelingThrough("<Keyboard>/escape")
            .WithCancelingThrough("<Gamepad>/select")
            .OnComplete(op => OnRebindFinished())
            .OnCancel(op => OnRebindFinished());

        if (!string.IsNullOrEmpty(bindingGroup))
            rebindOperation.WithControlsHavingToMatchPath("<Gamepad>");

        rebindOperation.Start();
    }

    private void OnRebindFinished()
    {
        CleanUpOperation();

        action.Enable();

        if (InputManager.Instance != null)
            InputManager.Instance.SaveRebinds();

        RefreshDisplay();
    }

    private void CleanUpOperation()
    {
        if (rebindOperation != null)
        {
            rebindOperation.Dispose();
            rebindOperation = null;
        }

        if (waitingOverlay != null)
            waitingOverlay.SetActive(false);
    }
}
