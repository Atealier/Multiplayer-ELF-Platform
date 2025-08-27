using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class loadedOnEnable : MonoBehaviour
{
    public float targetX = 5f;  // Target distance to move in the x direction (local space)
    public float targetY = 3f;  // Target distance to move in the y direction (local space)
    public float duration = 2f; // Time in seconds to complete the movement

    private Vector3 startLocalPosition;
    private Vector3 targetLocalPosition;
    public bool isMoving = false;  // Flag to track if currently moving
    private bool isReversed = false; // Flag to track if movement is reversed

    private bool delay;
    public float delayduration = 1.1f;

    public bool hidden = true;

    void Awake()
    {
        // Ensure startLocalPosition is set to the GameObject's current local position when it is first created.
        startLocalPosition = transform.localPosition;
    }

    private void OnEnable()
    {
        transform.localPosition = startLocalPosition; // Now, this will correctly reset to the initial position.
        isMoving = false;
        ReloadWithDelay();
    }

    void Start()
    {
        hidden = true;
        // You can still set the target position in Start
        targetLocalPosition = new Vector3(startLocalPosition.x + targetX, startLocalPosition.y + targetY, startLocalPosition.z);
    }

    public void LoadWithDelay()
    {
        if (!isMoving && gameObject.activeInHierarchy) // Prevent new movement until the current movement is finished
        {
            hidden = !hidden;
            delay = true;
            StartCoroutine(MoveOverTime(duration));  // Start the movement
        }
    }

    public void ReloadWithDelay()
    {
        if (!isMoving && gameObject.activeInHierarchy) // Prevent new movement until the current movement is finished
        {
            hidden = !hidden;
            delay = true;
            isReversed = false; // Ensure the movement starts from startLocalPosition to targetLocalPosition
            StartCoroutine(MoveOverTime(duration));  // Start the movement
        }
    }

    public void Load()
    {
        if (!isMoving && gameObject.activeInHierarchy) // Prevent new movement until the current movement is finished
        {
            hidden = !hidden;
            delay = false;
            StartCoroutine(MoveOverTime(duration));  // Start the movement
        }
    }

    IEnumerator MoveOverTime(float time)
    {
        isMoving = true;

        if (delay)
        {
            yield return new WaitForSeconds(delayduration);
        }

        float elapsedTime = 0f;

        // Choose the correct start and target positions based on the current direction
        Vector3 currentStart = isReversed ? targetLocalPosition : startLocalPosition;
        Vector3 currentTarget = isReversed ? startLocalPosition : targetLocalPosition;

        while (elapsedTime < time)
        {
            float t = elapsedTime / time;
            t = EaseInOutQuad(t); // Apply easing function
            transform.localPosition = Vector3.Lerp(currentStart, currentTarget, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the final position is set correctly at the end of the movement
        transform.localPosition = currentTarget;
        isReversed = !isReversed;  // Toggle the direction

        isMoving = false; // Mark movement as complete
    }

    // Easing function: EaseInOutQuad
    float EaseInOutQuad(float t)
    {
        return t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;
    }
}
