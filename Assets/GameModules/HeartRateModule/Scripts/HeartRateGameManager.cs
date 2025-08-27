using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeartRateGameManager : MonoBehaviour
{
    public bool isPlaying;
    public GameObject HeartRateScore;
    public int HeartRateScoreValue = 0;
    public float countdownTime;
    private Coroutine countdownCoroutine;
    private Coroutine inputTimeoutCoroutine; // Added coroutine for input timeout
    public GameObject HeartRateTimeLeft;
    public GameObject HearRateGamePlay;
    public HeartRateReceiver heartRateReceiver;
    public GameObject ScoreBoard;

    // Start is called before the first frame update
    void Start()
    {

    }

    void Update()
    {
        if (!isPlaying)
            return;

        HeartRateTimeLeft.GetComponent<TextMeshProUGUI>().text = countdownTime.ToString();
        //saccadecurrentscorevalue.GetComponent<TextMeshProUGUI>().text = saccadeScoreValue.ToString();
        
        // // Update the slider value
        // if (scoreDifference >= 50)
        // {
        //     progressSlider.value = progressSlider.maxValue; // Fully moved to Team 1's side
        // }
        // else if (scoreDifference <= -50)
        // {
        //     progressSlider.value = progressSlider.minValue; // Fully moved to Team 2's side
        // }
        // else
        // {
        //     // Calculate a normalized value for the slider (0 to 1) where 0.5 is neutral
        //     float normalizedValue = 0.5f + ((float)scoreDifference / 100f);
        //     progressSlider.value = Mathf.Clamp(normalizedValue * progressSlider.maxValue, progressSlider.minValue, progressSlider.maxValue);
        // }


        // If the input timeout coroutine is null, start the timeout for player input
        if (inputTimeoutCoroutine == null)
        {
            inputTimeoutCoroutine = StartCoroutine(InputTimeoutCoroutine());
        }

    }

    private IEnumerator InputTimeoutCoroutine()
    {
        Debug.Log("count down " + countdownTime);
        while (countdownTime > 0) // Check if isPlaying is true while waiting for input
        {
            countdownTime--;
            yield return new WaitForSeconds(1); // Wait for 1 second

        }

        // Reset the coroutine reference when isPlaying is false
        inputTimeoutCoroutine = null;
        isPlaying = false;
        Debug.Log("Game time over " + countdownTime);
        GeneralModule.Instance.checkWinner(heartRateReceiver.teamScores["Team1"], heartRateReceiver.teamScores["Team2"]);
        PlayerAvtarInSpectMode.Instance.ScoreBoardButton.SetActive(true);
        heartRateReceiver.isGameRunning = false;
        //heartRateReceiver.ResetGame();
        //HearRateGamePlay.SetActive(false);
        //ScoreBoard.SetActive(true);
    }

    public void StartCountdown()
    {
        isPlaying = true;

    }

}
