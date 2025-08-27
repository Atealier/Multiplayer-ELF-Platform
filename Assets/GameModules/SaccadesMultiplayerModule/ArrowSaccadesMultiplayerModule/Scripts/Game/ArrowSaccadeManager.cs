using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ArrowSaccadeManager : MonoBehaviour, ISaccadesModuleHandler
{
    public static ArrowSaccadeManager Instance { get; set; }

    private bool isPlaying;
    private int direction;
    public GameObject arrows; // Reference to the parent GameObject containing arrow children
    private int previousIndex = -1; // Store the index of the previously selected child

    public GameObject saccadegameplay;
    public GameObject saccademenu;
    public GameObject saccadetimeleft;
    public GameObject saccadecurrentscorevalue;

    public GameObject saccadeScore;
    public int saccadeScoreValue = 0;
    public int botScoreValue = 0;

    private float countdownTime;
    private Coroutine countdownCoroutine;
    private Coroutine inputTimeoutCoroutine; // Added coroutine for input timeout

    private int consecutiveCorrectAnswerCount = 0;

    public Slider progressSlider;

    [Header("Avatar Effect Thresholds (Configurable)")]
    public int AvtarBorderHighlightThreshold;
    public int AvtarRoundParticleThreshold;
    public int AvtarFlowParticleThreshold;

    public int GetAvtarBorderHighlightThreshold() => AvtarBorderHighlightThreshold;
    public int GetAvtarRoundParticleThreshold() => AvtarRoundParticleThreshold;
    public int GetAvtarFlowParticleThreshold() => AvtarFlowParticleThreshold;
    public void SetSaccadeScoreValue(int scoreValue)
    {
        saccadeScoreValue = scoreValue;
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {


    }

    void Update()
    {
        if (saccadeScore.activeInHierarchy)
        {
            if (progressSlider.value > 50)
            {
                saccadeScore.GetComponent<TextMeshProUGUI>().text = "Score: " + saccadeScoreValue.ToString();
            }
            else
            {
                saccadeScore.GetComponent<TextMeshProUGUI>().text = "Score: " + saccadeScoreValue.ToString();
            }
        }

        if (saccademenu.activeInHierarchy)
        {
            countdownTime = 60;
            saccadeScoreValue = 0;
            progressSlider.value = 50;
        }

        if (saccadegameplay.activeInHierarchy)
        {
            saccadetimeleft.GetComponent<TextMeshProUGUI>().text = countdownTime.ToString();
                                              
            if (isPlaying)
            {

                if (OnlineMultiplayerManager.Instance.Mode == "Single")
                {
                    OnlineMultiplayerManager.Instance.OnChangeSingleplayerScore(saccadeScoreValue, botScoreValue);
                }
                else if (OnlineMultiplayerManager.Instance.Mode == "Multiplayer")
                {
                        Debug.Log("SCORE IN GAME : " + saccadeScoreValue);
                        OnlineMultiplayerManager.Instance.OnChangeMultiplayerScore(saccadeScoreValue);

                        float team1Score = OnlineMultiplayerManager.Instance.Team1ScoreNetworked;
                        float team2Score = OnlineMultiplayerManager.Instance.Team2ScoreNetworked;

                        float scoreDifference = team2Score - team1Score;

                        // Update the slider value
                        if (scoreDifference >= 50)
                        {
                            progressSlider.value = progressSlider.maxValue; // Fully moved to Team 1's side
                        }
                        else if (scoreDifference <= -50)
                        {
                            progressSlider.value = progressSlider.minValue; // Fully moved to Team 2's side
                        }
                        else
                        {
                            // Calculate a normalized value for the slider (0 to 1) where 0.5 is neutral
                            float normalizedValue = 0.5f + ((float)scoreDifference / 100f);
                            progressSlider.value = Mathf.Clamp(normalizedValue * progressSlider.maxValue, progressSlider.minValue, progressSlider.maxValue);
                        }
                    
                }

                // If the input timeout coroutine is null, start the timeout for player input
                if (inputTimeoutCoroutine == null)
                {
                    inputTimeoutCoroutine = StartCoroutine(InputTimeoutCoroutine());
                }

                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    if (direction == 2) { correct(); } else { wrong(); }
                    RotateRandomArrow();

                    // Restart the input timeout timer after input is detected
                    RestartInputTimeout();
                }

                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    if (direction == 4) { correct(); } else { wrong(); }
                    RotateRandomArrow();

                    // Restart the input timeout timer after input is detected
                    RestartInputTimeout();
                }
            }
        }
        else
        {
            // Cancel the countdown and input timeout if saccadegameplay is not active
            if (countdownCoroutine != null)
            {
                saccadetimeleft.GetComponent<TextMeshProUGUI>().text = "";
               // saccadecurrentscorevalue.GetComponent<TextMeshProUGUI>().text = "";
                StopCoroutine(countdownCoroutine);
                countdownCoroutine = null; // Reset the coroutine reference
                Debug.Log("Countdown stopped.");
                isPlaying = false;
            }

            // Stop the input timeout coroutine if gameplay is not active
            if (inputTimeoutCoroutine != null)
            {
                StopCoroutine(inputTimeoutCoroutine);
                inputTimeoutCoroutine = null;
            }
        }
    }

  /*   public void correct()
     {
         saccadeScoreValue++;
         progressSlider.value += 1;
         progressSlider.value = Mathf.Clamp(progressSlider.value, progressSlider.minValue, progressSlider.maxValue);


         GameSoundManager.Instance.PlayCorrectAnswerSound();


         consecutiveCorrectAnswerCount++;
         consecutiveCorrectAnswerCount++;

         // Update networked property
         Player[] Players = FindObjectsOfType<Player>();

         foreach(Player localPlayer in Players)
         {
             if (localPlayer != null && localPlayer.HasStateAuthority)
             {
                 localPlayer.ConsecutiveCorrectAnswers = consecutiveCorrectAnswerCount;
             }
         }
         Debug.Log(consecutiveCorrectAnswerCount + "Consecutive Correct Answer");

         if(consecutiveCorrectAnswerCount == consecutiveCorrectAnswerHalfChain)
         {
            Debug.LogWarning("IN THE CHECKING ANS :" + consecutiveCorrectAnswerCount);
             FusionManager.Instance.ConsecutiveHalfCorrectAnswer();
         }
         if (consecutiveCorrectAnswerCount == consecutiveCorrectAnswerChain)
         {
            Debug.LogWarning("IN THE CHECKING ANS :" + consecutiveCorrectAnswerCount);
            FusionManager.Instance.ConsecutiveCorrectAnswer();
             consecutiveCorrectAnswerCount = 0;
         }     
     }*/

    public void correct()
    {
        saccadeScoreValue++;
        progressSlider.value += 1;
        progressSlider.value = Mathf.Clamp(progressSlider.value, progressSlider.minValue, progressSlider.maxValue);

        GameSoundManager.Instance.PlayCorrectAnswerSound();

        consecutiveCorrectAnswerCount++;

        // Update networked property
        Player[] Players = FindObjectsOfType<Player>();
        foreach (Player localPlayer in Players)
        {
            if (localPlayer != null && localPlayer.HasStateAuthority)
            {
                localPlayer.ConsecutiveCorrectAnswers = consecutiveCorrectAnswerCount;
            }
        }

        Debug.Log(consecutiveCorrectAnswerCount + " Consecutive Correct Answer");

      
        if (consecutiveCorrectAnswerCount >= AvtarRoundParticleThreshold && consecutiveCorrectAnswerCount < AvtarFlowParticleThreshold)
        {
            OnlineMultiplayerManager.Instance.ConsecutiveHalfCorrectAnswer();
        }

        if (consecutiveCorrectAnswerCount >= AvtarFlowParticleThreshold)
        {
            OnlineMultiplayerManager.Instance.ConsecutiveCorrectAnswer();
           // consecutiveCorrectAnswerCount = 0;
        }
    }

    public void wrong()
    {
        consecutiveCorrectAnswerCount = 0;
        

        // Update networked property
        Player[] Players = FindObjectsOfType<Player>();

        foreach (Player localPlayer in Players)
        {
            if (localPlayer != null && localPlayer.HasStateAuthority)
            {
                Debug.LogWarning("IN WRONG ANSWER");
                localPlayer.ConsecutiveCorrectAnswers = 0;
            }
        }


        if (OnlineMultiplayerManager.Instance.Mode == "Single")
        {
            botScoreValue++;
        }
        else if(OnlineMultiplayerManager.Instance.Mode == "Multiplayer")
        {
            if (saccadeScoreValue > 0)
            {
                //saccadeScoreValue--;
            }
        }
                
        progressSlider.value -= 1;
        progressSlider.value = Mathf.Clamp(progressSlider.value, progressSlider.minValue, progressSlider.maxValue);
    }

    public void DisableAllArrows()
    {
        // Loop through each child of the arrows GameObject
        for (int i = 0; i < arrows.transform.childCount; i++)
        {
            // Get the child Transform
            Transform child = arrows.transform.GetChild(i);

            // Get the SpriteRenderer component of the child
            SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();

            // If the child has a SpriteRenderer component, disable it
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }
        }
    }

    public void RotateRandomArrow()
    {
        // If there was a previous arrow selected, hide its SpriteRenderer
        if (previousIndex != -1)
        {
            Transform previousArrow = arrows.transform.GetChild(previousIndex);
            SpriteRenderer previousSpriteRenderer = previousArrow.GetComponent<SpriteRenderer>();
            if (previousSpriteRenderer != null)
            {
                previousSpriteRenderer.enabled = false; // Hide the previous arrow
            }
        }

        // Randomly choose one child of the arrows GameObject, excluding the previous one
        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, arrows.transform.childCount);
        } while (randomIndex == previousIndex);

        // Update the previous index to the current one
        previousIndex = randomIndex;

        // Get the selected arrow
        Transform selectedArrow = arrows.transform.GetChild(randomIndex);

        // Randomly choose one of the four possible Z-axis rotations (90, 270)
        int[] possibleRotations = { 90, 270 };
        int randomRotation = possibleRotations[Random.Range(0, possibleRotations.Length)];

        // Apply the rotation to the selected arrow
        selectedArrow.rotation = Quaternion.Euler(0, 0, randomRotation);

        if (randomRotation == 90) { direction = 2; }
        if (randomRotation == 270) { direction = 4; }

        // Ensure the SpriteRenderer component is enabled
        SpriteRenderer spriteRenderer = selectedArrow.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true; // Show the selected arrow
        }
        else
        {
            Debug.LogWarning("No SpriteRenderer component found on the selected child.");
        }
    }

    public void StartCountdown()
    {
        countdownTime = 60;  // change time
        progressSlider.value = 50;
        saccadeScoreValue = 0;
        botScoreValue = 0;
        isPlaying = true;
        countdownCoroutine = StartCoroutine(CountdownCoroutine());
    }

    private IEnumerator CountdownCoroutine()
    {
        Debug.Log("start count down for host also");
        while (countdownTime > 0)
        {
            yield return new WaitForSeconds(1);
            countdownTime--;

            //if (progressSlider.value == 100)
            //{
            //    break;
            //}
            // Optionally update a UI element to display the remaining time

            //Debug.Log("Time remaining: " + countdownTime);
        }

        isPlaying = false;
        // Handle what happens when the timer reaches zero
        Debug.LogWarning("Countdown finished!");
        GameSoundManager.Instance.StopBackgroundMusic();
        botScoreValue = 0;
        saccadeScoreValue = 0;
        countdownCoroutine = null; // Reset the coroutine reference
        gameObject.GetComponent<ToggleImages>().OnButtonClick();



        Debug.LogWarning("IT'S COUNTDOWN FINISHED");
        OnlineMultiplayerManager.Instance.OpenScoreBoard();
    }
                    
    public void LastPlayerRemoved()
    {
        isPlaying = false;
        // Handle what happens when the timer reaches zero
        Debug.Log("Countdown finished!");
        botScoreValue = 0;
        saccadeScoreValue = 0;
        countdownCoroutine = null; // Reset the coroutine reference
        gameObject.GetComponent<ToggleImages>().OnButtonClick();

        OnlineMultiplayerManager.Instance.OpenScoreBoard();
    }

    // Coroutine that checks for input every 1 second
    private IEnumerator InputTimeoutCoroutine()
    {
        while (isPlaying) // Check if isPlaying is true while waiting for input
        {
            yield return new WaitForSeconds(1); // Wait for 1 second

            // If no input was detected, call RotateRandomArrow() and wrong()
            RotateRandomArrow();

            if(OnlineMultiplayerManager.Instance.Mode == "Single")
            {
                wrong();

            }

            // Continue checking for the next timeout
        }

        // Reset the coroutine reference when isPlaying is false
        inputTimeoutCoroutine = null;
    }

    // Method to restart the input timeout timer
    private void RestartInputTimeout()
    {
        if (inputTimeoutCoroutine != null)
        {
            StopCoroutine(inputTimeoutCoroutine);
        }

        // Start a new timeout only if isPlaying is true
        if (isPlaying)
        {
            inputTimeoutCoroutine = StartCoroutine(InputTimeoutCoroutine());
        }
    }

   
}
