using System.Collections;
using UnityEngine;

public class FadeInSpriteRenderer : MonoBehaviour
{
    public SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component
    public float fadeInDuration = 2f; // Duration of the fade-in animation

    private Color originalColor;
    private bool isFading = false; // Flag to track if the fade is in progress
    private bool delay;
    public float delayDuration = 1.1f;

    void Awake()
    {
        // Ensure the SpriteRenderer component is assigned
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // Save the original color and set the alpha to 0 (completely transparent)
        originalColor = spriteRenderer.color;
        Color transparentColor = originalColor;
        transparentColor.a = 0f;
        spriteRenderer.color = transparentColor;
    }

    private void OnEnable()
    {
        isFading = false;
        Color transparentColor = originalColor;
        transparentColor.a = 0f;
        spriteRenderer.color = transparentColor; // Reset alpha to 0
        FadeInWithDelay(); // Trigger the fade-in when the GameObject is enabled
    }

    public void FadeInWithDelay()
    {
        if (!isFading && gameObject.activeInHierarchy)
        {
            delay = true;
            StartCoroutine(FadeInSprite(fadeInDuration));
        }
    }

    public void FadeIn()
    {
        if (!isFading && gameObject.activeInHierarchy)
        {
            delay = false;
            StartCoroutine(FadeInSprite(fadeInDuration));
        }
    }

    private IEnumerator FadeInSprite(float time)
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
            spriteRenderer.color = newColor;

            yield return null;
        }

        // Ensure the final color is set to the original color
        spriteRenderer.color = originalColor;
        isFading = false; // Mark the fade as complete
    }
}
