using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class stretch : MonoBehaviour
{
    private float targetScaleX = 6.15f; // Target scale for the X axis
    private float duration = 0.5f; // Duration of the stretch
    private bool isStretching = false; // To prevent multiple stretch calls

    // Call this function to start stretching
    public void Stretch()
    {
        if (!isStretching)
        {
            StartCoroutine(StretchX());
        }
    }

    // Coroutine to gradually stretch the scale on the X axis with easing
    private IEnumerator StretchX()
    {
        isStretching = true;
        Vector3 initialScale = transform.localScale;
        Vector3 targetScale = new Vector3(targetScaleX, initialScale.y, initialScale.z);
        float elapsedTime = 0f;

        // Set the initial scale to 0 on the X axis
        transform.localScale = new Vector3(0, initialScale.y, initialScale.z);

        while (elapsedTime < duration)
        {
            // Calculate the normalized time (0 to 1)
            float t = elapsedTime / duration;

            // Use Mathf.SmoothStep to ease-in and ease-out (speed up, then slow down)
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
        isStretching = false;
    }
}
