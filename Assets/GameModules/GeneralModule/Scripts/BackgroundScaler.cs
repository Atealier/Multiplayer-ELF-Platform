using UnityEngine;

public class BackgroundScaler : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;
    private float lastAspectRatio;
    private float lastOrthographicSize;

    void Start()
    {
        // Get the SpriteRenderer component from this GameObject
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer not found on " + gameObject.name);
            return;
        }

        // Get the main camera and ensure it’s orthographic
        mainCamera = Camera.main;
        if (mainCamera == null || !mainCamera.orthographic)
        {
            Debug.LogError("Main camera is not set or not orthographic.");
            return;
        }

        // Initial scale adjustment
        AdjustScale();

        // Store initial values to detect changes
        lastAspectRatio = mainCamera.aspect;
        lastOrthographicSize = mainCamera.orthographicSize;
    }

    void Update()
    {
        // Check if the aspect ratio or orthographic size has changed
        if (mainCamera.aspect != lastAspectRatio || mainCamera.orthographicSize != lastOrthographicSize)
        {
            AdjustScale();
            lastAspectRatio = mainCamera.aspect;
            lastOrthographicSize = mainCamera.orthographicSize;
        }
    }

    private void AdjustScale()
    {
        // Calculate the camera’s view width in world units
        float viewWidth = 2 * mainCamera.orthographicSize * mainCamera.aspect;

        // Get the sprite’s original width (before scaling)
        float originalWidth = spriteRenderer.sprite.bounds.size.x;

        // Calculate the scale factor to match the screen width
        float scaleFactor = viewWidth / originalWidth;

        // Apply the scale uniformly to maintain aspect ratio
        transform.localScale = new Vector3(scaleFactor, transform.localScale.y, transform.localScale.z);
    }
}