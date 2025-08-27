using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class countdownmanagerforhost : MonoBehaviour
{
    public float scaleSpeed = 3f;  // Speed of scaling
    public float waitTime = 0.3f;  // Time to wait between scale up and down

    private void OnEnable()
    {
        // Set all children to (0, 0, 0) scale first
        foreach (Transform child in transform)
        {
            child.localScale = Vector3.zero;
        }

        StartScalingChildren();
    }

    public void StartScalingChildren()
    {
        StartCoroutine(ScaleChildren());
    }

    private IEnumerator ScaleChildren()
    {
        GameSoundManager.Instance.PlayTimerSound();
        GameSoundManager.Instance.PlayBackgroundMusic();


        // Loop through all children of this GameObject
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);

            // Gradually scale the child up to (1, 1, 1) with acceleration
            yield return StartCoroutine(ScaleObject(child, Vector3.zero, Vector3.one));

            // Wait for 0.2 seconds
            yield return new WaitForSeconds(waitTime);

            // Gradually scale the child down to (0, 0, 0) with deceleration
            yield return StartCoroutine(ScaleObject(child, Vector3.one, Vector3.zero));

            // Wait for 0.2 seconds
            yield return new WaitForSeconds(waitTime);
        }

        // Optionally restart the process to loop through all children again
        //StartCoroutine(ScaleChildren());

        AnimationManager.Instance.Team1Character.StartPullingAnimation();
        AnimationManager.Instance.Team2Character.StartPullingAnimation();


       
    }

    private IEnumerator ScaleObject(Transform obj, Vector3 startScale, Vector3 endScale)
    {
        float progress = 0f;
        while (progress < 1f)
        {
            // Gradually accelerate the scale using SmoothStep for a more polished effect
            progress += Time.deltaTime * scaleSpeed;
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress); // Smooth acceleration and deceleration
            obj.localScale = Vector3.Lerp(startScale, endScale, easedProgress);
            yield return null;
        }

        // Ensure it reaches the final scale exactly
        obj.localScale = endScale;
    }
}
