using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public enum GameModule
{
    OnlineMultiplayer,
    HeartRate
}

public enum OnlineMultiPlayerGameModule
{
    None,
    ArrowSaccades,
}

public class GeneralModule : MonoBehaviour
{
    public static GeneralModule Instance { get; private set; }

    public GameObject[] GameModulesUI;
    public GameObject[] GameModules;

    public GameObject[] OnlineMultiplayerGameModulesUI;
    public GameObject[] OnlineMultiplayerGameModules;

    public Sprite[] Avtars;

    public GameModule CurrentGameModule;

    public OnlineMultiPlayerGameModule CurrentOnlineMultiplayerGameModule;
    public ISaccadesModuleHandler CurrentSaccadesModuleHandler;
    public GameObject[] CurrentSaccadesModuleErrorMsg;

    public float dealyForScore = 5f;
    public GameObject scorePanel;
    public Button backButton;

    public TextMeshProUGUI Team1BoardScoreText, Team2BoardScoreText;

    public int[] playerPossibleEvenNumber = { 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32 };
    public int[] playerPossibleOddNumber = { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25, 27, 29, 31 };

    public GameObject Canvas;

    private void Awake()
    {
        // Singleton assignment
        if (Instance == null)
        {
            Debug.Log("General module instance created");
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("Multiple instances of GeneralGameModule found! Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    public void SetCurrentSelctedMode(GameModule gameModule)
    {
        CurrentGameModule = gameModule;

        int selectedIndex = (int)gameModule;

        for (int i = 0; i < GameModulesUI.Length; i++)
        {
            GameModulesUI[i].SetActive(i == selectedIndex);
            GameModules[i].SetActive(i == selectedIndex);
        }

        if (CurrentGameModule != GameModule.OnlineMultiplayer && backButton != null)
        {
            backButton.onClick.RemoveAllListeners(); // Remove all existing listeners
            IBackButtonHandler handler = GameModules[selectedIndex].GetComponent<IBackButtonHandler>();
            if (handler != null)
            {
                //Debug.Log("Set back button listner  of interface");
                backButton.onClick.AddListener(handler.OnBackButtonPressed); // Set new listener
            }
            else
            {
                //Debug.LogWarning($"No IBackButtonHandler found on GameModules[{selectedIndex}]");
            }
        }

    }

    public void SetCurrentSelctedOnlineMultiplayerMode(OnlineMultiPlayerGameModule onlineMultiPlayerGameModule, GameObject[] gamePanels,GameObject[] errorMsg,Button hostButton,Button joinButton)
    {
        CurrentOnlineMultiplayerGameModule = onlineMultiPlayerGameModule;
        CurrentSaccadesModuleErrorMsg = errorMsg;

        OnlineMultiplayerUIManager.Instance.Panels = gamePanels;
        //joinButton.interactable = false;
        //hostButton.interactable = false;
        OnlineMultiplayerManager.Instance.JoinBtn = joinButton;
        OnlineMultiplayerManager.Instance.HostBtn = hostButton;

        int selectedIndex = (int)onlineMultiPlayerGameModule;

        for (int i = 0; i < OnlineMultiplayerGameModulesUI.Length; i++)
        {
            OnlineMultiplayerGameModulesUI[i].SetActive(i == selectedIndex);
            OnlineMultiplayerGameModules[i].SetActive(i == selectedIndex);
        }
        CurrentSaccadesModuleHandler = OnlineMultiplayerGameModules[selectedIndex].GetComponent<ISaccadesModuleHandler>();

        if (OnlineMultiplayerUIManager.Instance.StartGameButton != null)
        {
            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners(); // Remove all existing listeners
                IBackButtonHandler handler = OnlineMultiplayerGameModules[selectedIndex].GetComponent<IBackButtonHandler>();
                Debug.Log(OnlineMultiplayerGameModules[selectedIndex].name + " Game object name");
                if (handler != null)
                {
                    Debug.Log("Set back button listner  of interface");
                    backButton.onClick.AddListener(handler.OnBackButtonPressed); // Set new listener
                }
                else
                {
                    Debug.LogWarning($"No IBackButtonHandler found on OnlineMultiplayerGameModules[{selectedIndex}]");
                }
            }

            //OnlineMultiplayerUIManager.Instance.StartGameButton.onClick.RemoveAllListeners(); // Remove all existing listeners
            //IStartButtonHandler handler = OnlineMultiplayerGameModules[selectedIndex].GetComponent<IStartButtonHandler>();
            //if (handler != null)
            //{
            //    Debug.Log("Set start button listner  of interface");
            //  //  backButton.onClick.AddListener(handler.OnStartGameButtonPressed); // Set new listener
            //}
            //else
            //{
            //    Debug.LogWarning($"No OnStartGameButtonPressed found on GameModules[{selectedIndex}]");
            //}
        }

    }

    public void DisableCurrentSelectedModule()
    {
        if(CurrentOnlineMultiplayerGameModule != OnlineMultiPlayerGameModule.None)
        {
            OnlineMultiplayerGameModulesUI[(int)CurrentOnlineMultiplayerGameModule].SetActive(false);
            OnlineMultiplayerGameModules[(int)CurrentOnlineMultiplayerGameModule].SetActive(false);
        }

        GameModulesUI[(int)CurrentGameModule].SetActive(false);
        GameModules[(int)CurrentGameModule].SetActive(false);
    }

    public void ScorePanelOpenClose(bool isOpen, float Team1Score, float Team2Score)
    {
        scorePanel.SetActive(isOpen);
        Team1BoardScoreText.text = Team1Score.ToString();
        Team2BoardScoreText.text = Team2Score.ToString();
        OnlineMultiplayerManager.Instance.isScoreShowed = false;
    }

    public void checkWinner(float team1Score, float team2Score)
    {
        Debug.Log("Check winner");
        if (Mathf.RoundToInt(team1Score) > Mathf.RoundToInt(team2Score))
        {
            AnimationManager.Instance.Team1Character.UpdateAnimationBasedWinner("win", "Team1");
            AnimationManager.Instance.Team2Character.UpdateAnimationBasedWinner("lose", "Team1");
            Debug.Log("Team 1 Win");
        }
        else if (Mathf.RoundToInt(team1Score) < Mathf.RoundToInt(team2Score))
        {
            AnimationManager.Instance.Team1Character.UpdateAnimationBasedWinner("lose", "Team2");
            AnimationManager.Instance.Team2Character.UpdateAnimationBasedWinner("win", "Team2");
            Debug.Log("Team 2 Win");
        }
        else
        {
            AnimationManager.Instance.Team1Character.UpdateAnimationBasedWinner("win", "Draw");
            AnimationManager.Instance.Team2Character.UpdateAnimationBasedWinner("win", "Draw");
            Debug.Log("Draw");
        }
    }

    public void OpenScoreBoard()
    {
        AnimationManager.Instance.GamePlayAnimation.SetActive(false);
        Canvas.SetActive(true);
        scorePanel.SetActive(true);
    }

    
}
