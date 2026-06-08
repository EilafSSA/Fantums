using UnityEngine;

public class OptionsMenu : MonoBehaviour
{
    [Header("=== Panels ===")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject mainPanel;

    [Header("=== Rebind Rows ===")]
    [SerializeField] private RebindActionUI[] rebindRows;

    private void OnEnable()
    {
        RefreshAllRows();
    }

    public void OpenOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(true);
        if (mainPanel != null) mainPanel.SetActive(false);
    }

    public void CloseOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true);

        if (InputManager.Instance != null)
            InputManager.Instance.SaveRebinds();
    }

    public void ResetToDefaults()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.ResetAllRebinds();

        RefreshAllRows();
    }

    private void RefreshAllRows()
    {
        if (rebindRows == null) return;
        foreach (var row in rebindRows)
        {
            if (row != null) row.RefreshDisplay();
        }
    }
}
