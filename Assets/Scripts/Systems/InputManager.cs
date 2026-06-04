using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-100)]
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [Header("=== Input Actions ===")]
    [SerializeField] private InputActionAsset actions;

    private const string RebindsKey = "rebinds";

    public InputAction Move { get; private set; }
    public InputAction Jump { get; private set; }
    public InputAction Attack { get; private set; }
    public InputAction Sprint { get; private set; }
    public InputAction Cling { get; private set; }
    public InputAction Interact { get; private set; }
    public InputAction Crouch { get; private set; }

    public InputActionAsset Actions => actions;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;

        InputActionAsset asset = Resources.Load<InputActionAsset>("InputSystem_Actions");
        GameObject go = new GameObject("InputManager");
        InputManager mgr = go.AddComponent<InputManager>();
        if (asset != null && mgr.actions == null)
            mgr.actions = asset;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (actions == null)
            actions = InputSystem.actions;

        if (actions == null)
            actions = Resources.Load<InputActionAsset>("InputSystem_Actions");

        if (actions == null)
        {
            Debug.LogError("[InputManager] No InputActionAsset found.");
            return;
        }

        CacheActions();
        LoadRebinds();
    }

    private void OnEnable()
    {
        if (actions != null)
            actions.FindActionMap("Player", true).Enable();
    }

    private void OnDisable()
    {
        if (actions != null)
        {
            var map = actions.FindActionMap("Player", false);
            if (map != null) map.Disable();
        }
    }

    private void CacheActions()
    {
        var player = actions.FindActionMap("Player", true);
        Move = player.FindAction("Move", true);
        Jump = player.FindAction("Jump", true);
        Attack = player.FindAction("Attack", true);
        Sprint = player.FindAction("Sprint", true);
        Cling = player.FindAction("Cling", true);
        Interact = player.FindAction("Interact", false);
        Crouch = player.FindAction("Crouch", false);

        player.Enable();
    }

    public void SaveRebinds()
    {
        if (actions == null) return;
        string json = actions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(RebindsKey, json);
        PlayerPrefs.Save();
    }

    public void LoadRebinds()
    {
        if (actions == null) return;
        string json = PlayerPrefs.GetString(RebindsKey, string.Empty);
        if (!string.IsNullOrEmpty(json))
            actions.LoadBindingOverridesFromJson(json);
    }

    public void ResetAllRebinds()
    {
        if (actions == null) return;
        actions.RemoveAllBindingOverrides();
        PlayerPrefs.DeleteKey(RebindsKey);
        PlayerPrefs.Save();
    }
}
