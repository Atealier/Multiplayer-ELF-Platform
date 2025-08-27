using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroAnimationManager : MonoBehaviour
{
    public static IntroAnimationManager Instance { get; private set; }

    public GameObject IntroAnimation, IntroVsAnimation;

    public bool isIntroAnimCompleted = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public IEnumerator HandleIntroAnimationAndPlayerLogic()
    {

        GeneralModule.Instance.Canvas.SetActive(false);
        IntroAnimation.SetActive(true);

        // Wait for 10 seconds
        yield return new WaitForSeconds(10f);

        IntroAnimation.SetActive(false);
        IntroVsAnimation.SetActive(true);

        yield return new WaitForSeconds(4f);

        IntroVsAnimation.SetActive(false);
        GeneralModule.Instance.Canvas.SetActive(true);

        isIntroAnimCompleted = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
