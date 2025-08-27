using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ToggleImages : MonoBehaviour
{
    public List<GameObject> gameObjects; // List of GameObjects to toggle
    public GameObject gameTransitionScreen; // GameObject for transition effect
    public float transitionSpeed = 1f; // Speed of the transition (1 is default)

    private int currentIndex = 0; // Index to keep track of the current active GameObject
    private SpriteRenderer transitionSprite; // SpriteRenderer to control the alpha

    public TextMeshProUGUI zoomText;


    void Start()
    {
        // Initialize the SpriteRenderer from gameTransitionScreen
        transitionSprite = gameTransitionScreen.GetComponent<SpriteRenderer>();

        // Make sure there is at least one GameObject in the list
        if (gameObjects.Count > 0)
        {
            // Set the first GameObject as active at the start
            UpdateActiveGameObject();
        }
        currentIndex = GetCurrentActiveIndex();
    }

    public void OnButtonClickReversed()
    {
        StartCoroutine(TransitionAndCycle(false));
    }

    public void OnButtonClick()
    {
        StartCoroutine(TransitionAndCycle(true));
    }

    // New Coroutine to handle transitions and cycling
    private IEnumerator TransitionAndCycle(bool forward)
    {
        // Fade in the transition screen from 0 to 255 alpha
        yield return StartCoroutine(FadeInTransitionScreen());

        // Find the index of the currently active GameObject
        currentIndex = GetCurrentActiveIndex();

        // Deactivate the currently active GameObject
        gameObjects[currentIndex].SetActive(false);

        // Move to the next GameObject in the list
        if (forward)
        {
            currentIndex = (currentIndex + 1) % gameObjects.Count;
        }
        else
        {
            currentIndex = (currentIndex - 1 + gameObjects.Count) % gameObjects.Count;
        }

        // Activate the next GameObject
        UpdateActiveGameObject();
        CycleScales();

        // Fade out the transition screen from 255 to 0 alpha
        yield return StartCoroutine(FadeOutTransitionScreen());
    }

    private IEnumerator FadeInTransitionScreen()
    {
        Color color = transitionSprite.color;

        // Gradually increase alpha from 0 to 1 (0 to 255 in byte terms)
        for (float alpha = 0; alpha <= 1; alpha += Time.deltaTime * transitionSpeed)
        {
            color.a = alpha;
            transitionSprite.color = color;
            yield return null;
        }

        // Ensure alpha is set to fully opaque (1)
        color.a = 1;
        transitionSprite.color = color;
    }

    private IEnumerator FadeOutTransitionScreen()
    {
        Color color = transitionSprite.color;

        // Gradually decrease alpha from 1 to 0 (255 to 0 in byte terms)
        for (float alpha = 1; alpha >= 0; alpha -= Time.deltaTime * transitionSpeed)
        {
            color.a = alpha;
            transitionSprite.color = color;
            yield return null;
        }

        // Ensure alpha is set to fully transparent (0)
        color.a = 0;
        transitionSprite.color = color;
    }

    void UpdateActiveGameObject()
    {
        if (gameObjects.Count > 0)
        {
            //gameObjects[currentIndex].SetActive(true);
        }
    }

    // Method to find the index of the currently active GameObject
    private int GetCurrentActiveIndex()
    {
        for (int i = 0; i < gameObjects.Count; i++)
        {
            if (gameObjects[i].activeSelf)
            {
                return i;
            }
        }
        return 0; // Default to 0 if none are active
    }

    // List to store your game objects for scaling
    public List<GameObject> gameObjects2 = new List<GameObject>();

    // Different scale options
    private Vector3[] scaleOptions = new Vector3[]
    {
        new Vector3(92f, 92f, 1f), // Scale 1
        new Vector3(47f, 47f, 2f), // Scale 2
        new Vector3(35f, 35f, 3f), // Scale 3
        new Vector3(23f, 23f, 4f)  // Scale 4
    };

    private int currentScaleIndex = 0;

    // Method to cycle and apply scales to all GameObjects
    public void CycleScales()
    {
        // Cycle to the next scale
        currentScaleIndex = (currentScaleIndex + 1) % scaleOptions.Length;

        // Apply the new scale to all game objects in the list
        ApplyScaleToAll();
    }

    // Method to scale up (increase scale)
    public void ScaleUp()
    {
        if (currentScaleIndex < scaleOptions.Length - 1)
        {
            currentScaleIndex++;
            ApplyScaleToAll();
        }
    }

    // Method to scale down (decrease scale)
    public void ScaleDown()
    {
        if (currentScaleIndex > 0)
        {
            currentScaleIndex--;
            ApplyScaleToAll();
        }
    }

    // Helper method to apply the current scale to all game objects
    private void ApplyScaleToAll()
    {
        foreach (GameObject obj in gameObjects2)
        {
            obj.transform.localScale = scaleOptions[currentScaleIndex];
        }
    }

    private void Update()
    {
        if (zoomText != null)
        {
            // Check the index and update the zoomText accordingly
            switch (currentScaleIndex)
            {
                case 0:
                    zoomText.text = "20/80\n6/24";
                    break;
                case 1:
                    zoomText.text = "20/40\n6/12";
                    break;
                case 2:
                    zoomText.text = "20/30\n6/9";
                    break;
                case 3:
                    zoomText.text = "20/20\n6/6";
                    break;
                default:
                    zoomText.text = "";
                    break;
            }
        }
    }
}
