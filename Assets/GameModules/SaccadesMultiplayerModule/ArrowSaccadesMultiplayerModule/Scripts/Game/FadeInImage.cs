using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeInImage : MonoBehaviour
{
    public Image uiImage; // Reference to the Image component
    public float fadeInDuration = 0.8f; // Duration of the fade-in animation

    private Color originalColor;
    private bool isFading = false; // Flag to track if the fade is in progress
    private bool delay;
    public float delayDuration = 0.7f;

    void Awake()
    {
        // Ensure the Image component is assigned
        if (uiImage == null)
        {
            uiImage = GetComponent<Image>();
        }

        // Save the original color and set the alpha to 0 (completely transparent)
        originalColor = uiImage.color;
        Color transparentColor = originalColor;
        transparentColor.a = 0f;
        uiImage.color = transparentColor;
    }

    private void OnEnable()
    {
        isFading = false;
        Color transparentColor = originalColor;
        transparentColor.a = 0f;
        uiImage.color = transparentColor; // Reset alpha to 0
        FadeInWithDelay(); // Trigger the fade-in when the GameObject is enabled
    }

    public void FadeInWithDelay()
    {
        if (!isFading && gameObject.activeInHierarchy)
        {
            delay = true;
            StartCoroutine(FadeInImageCoroutine(fadeInDuration));
        }
    }

    public void FadeIn()
    {
        if (!isFading && gameObject.activeInHierarchy)
        {
            delay = false;
            StartCoroutine(FadeInImageCoroutine(fadeInDuration));
        }
    }

    private IEnumerator FadeInImageCoroutine(float time)
    {
        isFading = true;

        if (delay)
        {
            yield return new WaitForSeconds(delayDuration);
        }

        float elapsedTime = 0f;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / time);

            // Lerp the alpha from 0 to the original alpha
            Color newColor = originalColor;
            newColor.a = Mathf.Lerp(0f, originalColor.a, t);
            uiImage.color = newColor;

            yield return null;
        }

        // Ensure the final color is set to the original color
        uiImage.color = originalColor;
        isFading = false; // Mark the fade as complete
    }
}
