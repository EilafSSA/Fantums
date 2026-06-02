using UnityEngine;
using UnityEngine.InputSystem;

public class DoorTransition : MonoBehaviour
{
    [Header("=== Destination ===")]
    [SerializeField] private Transform teleportTarget;

    [Header("=== Input ===")]
    [SerializeField] private InputActionReference interactAction;

    private bool playerInRange;
    private GameObject playerObject;

    private void OnEnable()
    {
        if (interactAction != null && interactAction.action != null)
            interactAction.action.Enable();
    }

    private bool EnterPressed()
    {
        if (interactAction != null && interactAction.action != null)
            return interactAction.action.WasPressedThisFrame();

        if (InputManager.Instance != null && InputManager.Instance.Interact != null)
            return InputManager.Instance.Interact.WasPressedThisFrame();

        return Keyboard.current != null && Keyboard.current.yKey.wasPressedThisFrame;
    }

    private void Update()
    {
        if (playerInRange && EnterPressed())
        {
            if (teleportTarget == null)
            {
                Debug.LogError("DoorTransition: No teleport target assigned!");
                return;
            }

            if (playerObject == null) return;

            Rigidbody2D rb = playerObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            playerObject.transform.position = teleportTarget.position;
            Debug.Log("Player teleported!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"DoorTrigger hit by: {other.gameObject.name} | Tag: {other.tag}");

        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerObject = other.gameObject;
            Debug.Log("Press Y to enter door.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerObject = null;
        }
    }
}
