using System.Collections;
using UnityEngine;
using TMPro;

public class FadeInTextMeshPro : MonoBehaviour
{
    public TextMeshProUGUI uiText; // Reference to the TextMeshProUGUI component
    public float fadeInDuration = 0.8f; // Duration of the fade-in animation

    private Color originalColor;
    private bool isFading = false; // Flag to track if the fade is in progress
    private bool delay;
    public float delayDuration = 0.7f;

    void Awake()
    {
        // Ensure the TextMeshPro component is assigned
        if (uiText == null)
        {
            uiText = GetComponent<TextMeshProUGUI>();
        }

        // Save the original color and set the alpha to 0 (completely transparent)
        originalColor = uiText.color;
        Color transparentColor = originalColor;
        transparentColor.a = 0f;
        uiText.color = transparentColor;
    }

    private void OnEnable()
    {
        isFading = false;
        Color transparentColor = originalColor;
        transparentColor.a = 0f;
        uiText.color = transparentColor; // Reset alpha to 0
        FadeInWithDelay(); // Trigger the fade-in when the GameObject is enabled
    }

    public void FadeInWithDelay()
    {
        if (!isFading && gameObject.activeInHierarchy)
        {
            delay = true;
            StartCoroutine(FadeInText(fadeInDuration));
        }
    }

    public void FadeIn()
    {
        if (!isFading && gameObject.activeInHierarchy)
        {
            delay = false;
            StartCoroutine(FadeInText(fadeInDuration));
        }
    }

    private IEnumerator FadeInText(float time)
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
            uiText.color = newColor;

            yield return null;
        }

        // Ensure the final color is set to the original color
        uiText.color = originalColor;
        isFading = false; // Mark the fade as complete
    }
}
