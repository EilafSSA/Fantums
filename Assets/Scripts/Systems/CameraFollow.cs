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

    private void Awake()
    {
        Instance = this;
    }

    //call this from PlayerHealth (or anywhere) to trigger a shake (for like the enemies? or maybe like idk but yes.)
    public void TriggerShake()
    {
        shakeTimer = shakeDuration;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = target.position + offset;

        if (useBounds)
        {
            desiredPos.x = Mathf.Clamp(desiredPos.x, minX, maxX);
            desiredPos.y = Mathf.Clamp(desiredPos.y, minY, maxY);
        }

        Vector3 smoothed = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);

        // apply shake offset on top of normal follow
        if (shakeTimer > 0f)
        {
            smoothed += (Vector3)Random.insideUnitCircle * shakeMagnitude;
            shakeTimer -= Time.deltaTime;
        }

        transform.position = smoothed;
    }
}