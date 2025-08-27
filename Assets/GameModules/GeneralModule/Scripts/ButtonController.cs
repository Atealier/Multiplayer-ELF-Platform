using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    public Button btn; // Assign in Inspector

    private void OnEnable()
    {
        if (btn != null)
        {
            btn.interactable = false; // Disable first
            StartCoroutine(EnableButtonAfterDelay(4f)); // Enable after 3 sec
        }
    }

    private IEnumerator EnableButtonAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        btn.interactable = true;
    }
}
