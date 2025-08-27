using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;


public class HeartRateUIManager : MonoBehaviour
{
    public static HeartRateUIManager Instance { get; private set; }
    // Lobby UI elements (16 players per team)
    //public TMP_InputField[] Team1PlayerNames = new TMP_InputField[16];
    //public TMP_InputField[] Team2PlayerNames = new TMP_InputField[16];
    public Image[] Team1PlayerAvatars = new Image[16];
    public Image[] Team2PlayerAvatars = new Image[16];
    public Button[] Team1SwitchButtons = new Button[16];
    public Button[] Team2SwitchButtons = new Button[16];
    public TMP_InputField[] Team1NameInputs = new TMP_InputField[16];
    public TMP_InputField[] Team2NameInputs = new TMP_InputField[16];
    public Button[] Team1EditButtons = new Button[16];
    public Button[] Team2EditButtons = new Button[16];
    public Button[] Team1SaveButtons = new Button[16];
    public Button[] Team2SaveButtons = new Button[16];

    // General UI elements
    public Button StartButton;
    public Button CloseLobbyButton;
    public GameObject LobbyPanel;
    public GameObject GameplayPanel;
    public TextMeshProUGUI Team1ScoreText;
    public TextMeshProUGUI Team2ScoreText;

    private HeartRateReceiver heartRateReceiver;
    private Sprite defaultAvatar; // Placeholder, replace with actual sprite

    // New fields for game length slider and text
    public Slider gameLengthSlider;
    public TextMeshProUGUI timeLengthText;
    public HeartRateGameManager heartRateGameManager;
    private int gameLengthMinValue = 1;
    public int GameLenghtMaxValue = 5;

    public GameObject HeartRateModuleUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        heartRateReceiver = FindObjectOfType<HeartRateReceiver>();
        defaultAvatar = Resources.Load<Sprite>("Square"); // Replace with your default avatar sprite

        // Initialize UI
        for (int i = 0; i < 16; i++)
        {
            int index = i;
            Team1SwitchButtons[i].onClick.AddListener(() => OnSwitchTeamButtonClicked("Team1", index));
            Team2SwitchButtons[i].onClick.AddListener(() => OnSwitchTeamButtonClicked("Team2", index));
            Team1EditButtons[i].onClick.AddListener(() => OnEditNameButtonClicked("Team1", index));
            Team2EditButtons[i].onClick.AddListener(() => OnEditNameButtonClicked("Team2", index));
            Team1SaveButtons[i].onClick.AddListener(() => OnSaveNameButtonClicked("Team1", index));
            Team2SaveButtons[i].onClick.AddListener(() => OnSaveNameButtonClicked("Team2", index));

            Team1NameInputs[i].interactable = false;
            Team2NameInputs[i].interactable = false;
            Team1EditButtons[i].gameObject.SetActive(false);
            Team2EditButtons[i].gameObject.SetActive(false);
            Team1SaveButtons[i].gameObject.SetActive(false);
            Team2SaveButtons[i].gameObject.SetActive(false);
        }

        StartButton.onClick.AddListener(OnStartButtonClicked);
        CloseLobbyButton.onClick.AddListener(() => Application.Quit()); // Placeholder, adjust as needed
        ShowGameplayPanel(false);

        gameLengthSlider.minValue = gameLengthMinValue;
        gameLengthSlider.maxValue = GameLenghtMaxValue;
        gameLengthSlider.value = GameLenghtMaxValue;
        gameLengthSlider.onValueChanged.AddListener(UpdateGameLength);

        UpdateGameLength(gameLengthSlider.value);
        UpdateStartButtonInteractability();
        
        // Connect to MQTT when HeartRate module is activated
        if (heartRateReceiver != null)
        {
//            heartRateReceiver.ConnectToMQTT();
        }
    }

    public void OnStartButtonClicked()
    {
        StartCoroutine(heartRateReceiver.StartGame());
    }

    // Method to update game length and UI text
    private void UpdateGameLength(float value)
    {
        if (heartRateGameManager != null)
        {
            heartRateGameManager.countdownTime = value * 60; // Convert minutes to seconds
        }
        timeLengthText.text = $"TIME LENGTH: {(int)value} MIN";
    }

    public void ResetUI()
    {
                // Reset UI
        for (int i = 0; i < 16; i++)
        {
            // Team1PlayerNames[i].text = "";
            // Team2PlayerNames[i].text = "";
            Team1PlayerAvatars[i].sprite = null;
            Team2PlayerAvatars[i].sprite = null;
            Team1SwitchButtons[i].gameObject.SetActive(false);
            Team2SwitchButtons[i].gameObject.SetActive(false);
            Team1EditButtons[i].gameObject.SetActive(false);
            Team2EditButtons[i].gameObject.SetActive(false);
            Team1NameInputs[i].text = "";
            Team2NameInputs[i].text = "";
        }

    }


    public void UpdatePlayerUI(string id, bool remove = false)
    {
        var teamPlayers = heartRateReceiver.GetTeamPlayers();
        var players = heartRateReceiver.GetPlayers();

        ResetUI();

        // Update Team1
        for (int i = 0; i < teamPlayers["Team1"].Count && i < 16; i++)
        {
            string playerId = teamPlayers["Team1"][i];
            PlayerData player = players[playerId];
            // Team1PlayerNames[i].text = player.name;
            Team1PlayerAvatars[i].sprite = player.avatar ?? defaultAvatar;
            Team1SwitchButtons[i].gameObject.SetActive(true);
            Team1NameInputs[i].text = player.name;
            Team1EditButtons[i].gameObject.SetActive(true);
        }

        // Update Team2
        for (int i = 0; i < teamPlayers["Team2"].Count && i < 16; i++)
        {
            string playerId = teamPlayers["Team2"][i];
            PlayerData player = players[playerId];
            // Team2PlayerNames[i].text = player.name;
            Team2PlayerAvatars[i].sprite = player.avatar ?? defaultAvatar;
            Team2SwitchButtons[i].gameObject.SetActive(true);
            Team2NameInputs[i].text = player.name;
            Team2EditButtons[i].gameObject.SetActive(true);
        }

        // Update switch button interactability based on team sizes
        int team1Count = teamPlayers["Team1"].Count;
        int team2Count = teamPlayers["Team2"].Count;

        // If Team2 is full (16 players), disable Team1's switch buttons
        if (team2Count >= 16)
        {
            for (int i = 0; i < team1Count && i < 16; i++)
            {
                Team1SwitchButtons[i].gameObject.SetActive(false);
            }
        }
        //else
        //{
        //    for (int i = 0; i < team1Count && i < 16; i++)
        //    {
        //        Team1SwitchButtons[i].gameObject.SetActive(false);
        //    }
        //}

        // If Team1 is full (16 players), disable Team2's switch buttons
        if (team1Count >= 16)
        {
            for (int i = 0; i < team2Count && i < 16; i++)
            {
                Team2SwitchButtons[i].gameObject.SetActive(false);
            }
        }
        //else
        //{
        //    for (int i = 0; i < team2Count && i < 16; i++)
        //    {
        //        Team2SwitchButtons[i].gameObject.SetActive(false);
        //    }
        //}

        UpdateStartButtonInteractability();
    }

    private void UpdateStartButtonInteractability()
    {
        var teamPlayers = heartRateReceiver.GetTeamPlayers();
        bool hasEnoughPlayers = teamPlayers["Team1"].Count > 0 && teamPlayers["Team2"].Count > 0;
        StartButton.interactable = hasEnoughPlayers;
    }

    void OnSwitchTeamButtonClicked(string currentTeam, int index)
    {
        var teamPlayers = heartRateReceiver.GetTeamPlayers();
        if (currentTeam == "Team1")
            Team1EditButtons[index].gameObject.SetActive(false);
        else
            Team2EditButtons[index].gameObject.SetActive(false);

        if (index < teamPlayers[currentTeam].Count)
        {
            string id = teamPlayers[currentTeam][index];
            heartRateReceiver.SwitchTeam(id);
            UpdateStartButtonInteractability();
        }
    }

    void OnEditNameButtonClicked(string team, int index)
    {
        if (team == "Team1")
        {
            Team1NameInputs[index].interactable = true;
            Team1EditButtons[index].gameObject.SetActive(false);
            Team1SaveButtons[index].gameObject.SetActive(true);
            Team1SwitchButtons[index].gameObject.SetActive(false);
            Team1NameInputs[index].Select();
        }
        else
        {
            Team2NameInputs[index].interactable = true;
            Team2EditButtons[index].gameObject.SetActive(false);
            Team2SaveButtons[index].gameObject.SetActive(true);
            Team2SwitchButtons[index].gameObject.SetActive(false);
            Team2NameInputs[index].Select();
        }
    }

    void OnSaveNameButtonClicked(string team, int index)
    {
        var teamPlayers = heartRateReceiver.GetTeamPlayers();
        if (index < teamPlayers[team].Count)
        {
            string id = teamPlayers[team][index];
            string newName = team == "Team1" ? Team1NameInputs[index].text : Team2NameInputs[index].text;
            heartRateReceiver.SetPlayerName(id, newName);

            if (team == "Team1")
            {
                Team1NameInputs[index].interactable = false;
                Team1EditButtons[index].gameObject.SetActive(true);
                Team1SaveButtons[index].gameObject.SetActive(false);
                Team1SwitchButtons[index].gameObject.SetActive(true);
            }
            else
            {
                Team2NameInputs[index].interactable = false;
                Team2EditButtons[index].gameObject.SetActive(true);
                Team2SaveButtons[index].gameObject.SetActive(false);
                Team2SwitchButtons[index].gameObject.SetActive(true);
            }
        }
    }

    //public void UpdateScores(int team1Score, int team2Score)
    //{
    //    Team1ScoreText.text = team1Score.ToString();
    //    Team2ScoreText.text = team2Score.ToString();
    //}

    public void ShowGameplayPanel(bool show)
    {
        LobbyPanel.SetActive(!show);
        GameplayPanel.SetActive(show);
    }

    public void GoToMenu()
    {
        Debug.Log("Called Go to menu from heart rate module");
        GeneralModule.Instance.scorePanel.SetActive(false);
        HeartRateModuleUI.SetActive(true);
        heartRateReceiver.ResetGame();

    }
}
