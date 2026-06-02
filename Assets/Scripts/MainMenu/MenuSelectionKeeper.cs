using UnityEngine;
using UnityEngine.EventSystems;

public class MenuSelectionKeeper : MonoBehaviour
{
    [SerializeField] private GameObject firstSelected;

    private void Start()
    {
        if (GetComponent<SelectionHighlighter>() == null)
            gameObject.AddComponent<SelectionHighlighter>();

        Select(firstSelected);
    }

    private void Update()
    {
        EventSystem es = EventSystem.current;
        if (es == null) return;

        if (es.currentSelectedGameObject == null || !es.currentSelectedGameObject.activeInHierarchy)
            Select(firstSelected);
    }

    public void SetFirstSelected(GameObject go)
    {
        firstSelected = go;
        Select(go);
    }

    private void Select(GameObject go)
    {
        if (go == null || EventSystem.current == null) return;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(go);
    }
}
