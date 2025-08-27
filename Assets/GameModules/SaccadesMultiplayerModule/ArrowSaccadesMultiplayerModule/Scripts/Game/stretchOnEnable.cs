using System.Collections;
using UnityEngine;

public class stretchOnEnable : MonoBehaviour
{
    public float targetScaleX = 1280.0f; // Target scale for the X axis
    public float duration = 0.5f; // Duration of the stretch
    private Coroutine currentStretchCoroutine; // Reference to the running coroutine
    public float delay; // Delay within the stretch process
    public float initialDelay; // New delay before initiating the stretch process

    private void OnEnable()
    {
        Stretch();
    }

    // Call this function to start stretching
    public void Stretch()
    {
        // If there's an existing stretch coroutine, stop it before starting a new one
        if (currentStretchCoroutine != null)
        {
            StopCoroutine(currentStretchCoroutine);
        }

        // Start a new coroutine for the initial delay and the stretch process
        currentStretchCoroutine = StartCoroutine(StretchWithInitialDelay());
    }

    // Coroutine to handle the initial delay before starting the stretch coroutine
    private IEnumerator StretchWithInitialDelay()
    {
        // Set the initial scale to 0 on the X axis before the initial delay
        Vector3 initialScale = transform.localScale;
        transform.localScale = new Vector3(0, initialScale.y, initialScale.z);

        // Wait for the initial delay
        yield return new WaitForSeconds(initialDelay);

        // Start the stretch process after the initial delay
        currentStretchCoroutine = StartCoroutine(StretchX());
    }

    // Coroutine to gradually stretch the scale on the X axis with easing
    private IEnumerator StretchX()
    {
        Vector3 initialScale = transform.localScale;
        Vector3 targetScale = new Vector3(targetScaleX, initialScale.y, initialScale.z);
        float elapsedTime = 0f;

        // Wait for the specified delay within the stretch process
        yield return new WaitForSeconds(delay);

        while (elapsedTime < duration)
        {
            // Calculate the normalized time (0 to 1)
            float t = elapsedTime / duration;

            // Use Mathf.SmoothStep to ease-in and ease-out (accelerate, then decelerate)
            float easedT = Mathf.SmoothStep(0, 1, t);

            // Calculate the current scale based on eased time
            float currentScaleX = Mathf.Lerp(0, targetScaleX, easedT);
            transform.localScale = new Vector3(currentScaleX, initialScale.y, initialScale.z);

            // Increase the elapsed time
            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // Ensure the final scale is exactly the target scale
        transform.localScale = targetScale;

        // Clear the reference to the completed coroutine
        currentStretchCoroutine = null;
    }
}
