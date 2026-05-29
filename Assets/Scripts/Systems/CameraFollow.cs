using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 8f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 1f, -10f);

    [Header("=== Camera Bounds (ehh temp doe) ===")]
    [SerializeField] private bool useBounds;
    [SerializeField] private float minX = -14f;
    [SerializeField] private float maxX = 21f;
    [SerializeField] private float minY = -2f;
    [SerializeField] private float maxY = 8f;

    [Header("=== Shake ===")]
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeMagnitude = 0.15f;

    public static CameraFollow Instance { get; private set; }

    private float shakeTimer;

    // cutscene override
    private Transform overrideTarget;
    private float overrideSmooth;
    private bool isOverriding;

    public Transform Target => target;

    private void Awake()
    {
        Instance = this;
    }

    //call this from PlayerHealth (or anywhere) to trigger a shake
    public void TriggerShake()
    {
        shakeTimer = shakeDuration;
    }

    /// <summary>Smoothly pan to a different target (used by boss cutscenes etc).</summary>
    public void FocusOn(Transform focusTarget, float smooth = 0f)
    {
        overrideTarget = focusTarget;
        overrideSmooth = smooth > 0f ? smooth : smoothSpeed;
        isOverriding = true;
    }

    /// <summary>Return the camera back to the player.</summary>
    public void ReturnToPlayer(float smooth = 0f)
    {
        overrideTarget = null;
        overrideSmooth = smooth > 0f ? smooth : smoothSpeed;
        isOverriding = false;
    }

    private void LateUpdate()
    {
        Transform activeTarget = isOverriding && overrideTarget != null ? overrideTarget : target;
        if (activeTarget == null) return;

        float activeSmooth = isOverriding ? overrideSmooth : smoothSpeed;

        Vector3 desiredPos = activeTarget.position + offset;

        if (useBounds && !isOverriding)
        {
            desiredPos.x = Mathf.Clamp(desiredPos.x, minX, maxX);
            desiredPos.y = Mathf.Clamp(desiredPos.y, minY, maxY);
        }

        Vector3 smoothed = Vector3.Lerp(transform.position, desiredPos, activeSmooth * Time.deltaTime);

        // apply shake offset on top of normal follow
        if (shakeTimer > 0f)
        {
            smoothed += (Vector3)Random.insideUnitCircle * shakeMagnitude;
            shakeTimer -= Time.deltaTime;
        }

        transform.position = smoothed;
    }
}