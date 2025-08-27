using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class DynamicParticleRadius : MonoBehaviour
{
    [Header("References")]
    public RectTransform avatarRect;               // UI avatar
    public Transform targetWorldTransform;         // World-space character
    public RectTransform canvasRect;               // Canvas (Screen Space - Overlay or Camera)
    public RectTransform rotatingChildRect;        // Child to rotate (e.g. ParticleToPlayer)

    [Header("Settings")]
    public float scaleMultiplier = 0.5f;           // Adjusts particle size relative to avatar
    public float rotationOffset = 0f;              // Per-character adjustment (set in Inspector)

    [Header("Debug")]
    public float angle;                            // For monitoring angle

    private ParticleSystem ps;
    private ParticleSystem.ShapeModule shape;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        shape = ps.shape;
    }

    void Update()
    {
        if (avatarRect == null || targetWorldTransform == null || canvasRect == null || rotatingChildRect == null)
            return;

        // === Update particle radius to match avatar size on screen ===
        float canvasScale = canvasRect.lossyScale.x; // Handles Canvas Scaler scaling
        float avatarSize = Mathf.Max(avatarRect.rect.width, avatarRect.rect.height) * canvasScale;
        shape.radius = avatarSize * scaleMultiplier;

        // === Convert world position to local canvas space ===
        Vector3 screenPos = Camera.main.WorldToScreenPoint(targetWorldTransform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, null, out Vector2 targetLocalPos);

        // === Get particle system UI position
        Vector2 origin = ((RectTransform)transform).localPosition;

        // === Calculate direction and angle
        Vector2 direction = targetLocalPos - origin;
        angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // === Apply custom offset
        angle += rotationOffset;

        // === Rotate particle child
        rotatingChildRect.localRotation = Quaternion.Euler(0, 0, angle);
    }
}










