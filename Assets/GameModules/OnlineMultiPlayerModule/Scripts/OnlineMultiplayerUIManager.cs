using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public enum PanelType
{
    SaccadeMenu,
    SaccadeGamePlay,
    SaccadeScore,
    SaccadeMenuMultiplayer,
    SaccadeMenuClientOrHost,
    //SaccadeMenuHost,
    //SaccadeScoreBoard,
    //SaccadeMenuClient,
    //SaccadeMenuSpectator
}

public enum GeneralPanelType
{
    SaccadeMenuHost,
    SaccadeMenuClient
}

public class OnlineMultiplayerUIManager : MonoBehaviour
{
    public static OnlineMultiplayerUIManager Instance {get; private set;}
    
    public TextMeshProUGUI[] HostTeam1PlayerName, HostTeam2PlayerName, ClientTeam1PlayerName, ClientTeam2PlayerName;
    public BoolWrapper[] HostTeam1OccupiedPlace, HostTeam2OccupiedPlace, ClientTeam1OccupiedPlace, ClientTeam2OccupiedPlace;
    public Image[] HostTeam1PlayerAvtar, HostTeam2PlayerAvtar, ClientTeam1PlayerAvtar, ClientTeam2PlayerAvtar;
    public Button[] HostTeam1KickButton, HostTeam2KickButton;
    public TMP_InputField[] PlayerNameInput;
    public Image[] PlayerAvtars;
    //public Sprite[] Avtars;
    public int PlayerAvtarNumber;
    public Sprite PlayerAvtar;
    public Button Team1HostLeaveButton, Team1HostJoinButton, Team2HostLeaveButton, Team2HostJoinButton;
    public Button Team1ClientLeaveButton, Team1ClientJoinButton, Team2ClientLeaveButton, Team2ClientJoinButton;
    public TextMeshProUGUI RoomCodeText;
    public GameObject SaccadeMenu, SaccadeMenuAsAClientOrHost, SaccadeMenuAsAHost;
    public Button StartButton, JoinLobbyButton, EnterRoomButton;
    public Button HostLobbyButton;
    public GameObject CodeInputField;
    public Button StartGameButton;
    public GameObject[] Panels;
    public GameObject[] GeneralGamePanels;
    //public TextMeshProUGUI Team1BoardScoreText, Team2BoardScoreText, Team1GameScore, Team2GameScore;
    public TextMeshProUGUI  Team1GameScore, Team2GameScore;
    public Button[] EditButton, SaveButton;
    public GameObject CountDownForHost /*ErrorMsg*/;
    public GameObject SpectModeMenuButton;
    public GameObject IntroAnimation, IntroVsAnimation, AllCanvas;

    public bool PlayerIsInTeamTemporary = false;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

        }
        else
        {
            Destroy(gameObject);
        }
        HostTeam1OccupiedPlace = CreateBoolWrapperArray(10);
        HostTeam2OccupiedPlace = CreateBoolWrapperArray(10);
        ClientTeam1OccupiedPlace = CreateBoolWrapperArray(10);
        ClientTeam2OccupiedPlace = CreateBoolWrapperArray(10);
    }

    private BoolWrapper[] CreateBoolWrapperArray(int size)
    {
        var arr = new BoolWrapper[size];
        for (int i = 0; i < size; i++)
        {
            arr[i] = new BoolWrapper { Value = false };
        }
        return arr;
    }

    private void Start()
    {
        SetRandomAvtar();
        for (int i = 0; i < HostTeam1KickButton.Length; i++)
        {
            int index = i;
            HostTeam1KickButton[i].onClick.AddListener(() => OnKickButtonClicked("Team1", index));
            HostTeam2KickButton[i].onClick.AddListener(() => OnKickButtonClicked("Team2", index));
        }
        // Initialize edit/save buttons
        foreach (Button edit in EditButton) edit.gameObject.SetActive(true);
        foreach (Button save in SaveButton) save.gameObject.SetActive(false);

        string randomPlayerName = GenerateRandomString(4); // Adjust length as needed

        foreach (TMP_InputField playernameinputfield in PlayerNameInput)
        {
            playernameinputfield.text = PlayerPrefs.GetString(OnlineMultiplayerManager.Instance.PlayerNameKey, randomPlayerName);
            playernameinputfield.interactable = false; // Lock input initially
        }

        Player[] players = FindObjectsOfType<Player>();

        foreach (Player player in players)
        {
            if (player.HasStateAuthority)
            {
                player.Name = PlayerNameInput[0].text;
                player.PlayerAvtar = PlayerAvtars[0].sprite;
                player.ChangedPlayerAvtar = !player.ChangedPlayerAvtar;

            }
        }
    }

    public void SetRandomAvtar()
    {
        int AvtarNumber = Random.Range(0, GeneralModule.Instance.Avtars.Count());
        PlayerAvtarNumber = AvtarNumber;

        foreach (Image playerImage in PlayerAvtars)
        {
            playerImage.sprite = GeneralModule.Instance.Avtars[AvtarNumber];
        }
        PlayerAvtar = GeneralModule.Instance.Avtars[AvtarNumber];
        Player[] players = FindObjectsOfType<Player>();
       
        foreach (Player player in players)
        {
            if (player.HasStateAuthority)
            {
                player.PlayerAvtar = PlayerAvtar;
                player.PlayerAvtarIndex = AvtarNumber;
                player.ChangedPlayerAvtar = !player.ChangedPlayerAvtar;
            }
        }
    }

    private string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        char[] stringChars = new char[length];
        for (int i = 0; i < length; i++)
        {
            stringChars[i] = chars[Random.Range(0, chars.Length)];
        }

        return new string(stringChars);
    }

    public void UpdateScores(float team1Score, float team2Score)
    {
        Team1GameScore.text = team1Score.ToString();
        Team2GameScore.text = team2Score.ToString();

        GeneralModule.Instance.Team1BoardScoreText.text = team1Score.ToString();
        GeneralModule.Instance.Team2BoardScoreText.text = team2Score.ToString();
    }
    public void SetRoomCode(string roomCode)
    {
        RoomCodeText.text = roomCode;
    }

    public void ShowPanel(int panelIndex,bool toggle)
    {
        Panels[panelIndex].SetActive(toggle);
    }

    public void ShowGeneralGamePanel(int panelIndex, bool toggle)
    {
        GeneralGamePanels[panelIndex].SetActive(toggle);
    }

    public void ClientButtonClick()
    {
        SaccadeMenu.SetActive(false);
        SaccadeMenuAsAClientOrHost.SetActive(true);
        JoinLobbyButton.gameObject.SetActive(true);
    }

    public void OnKickButtonClicked(string team, int index)
    {
        Debug.Log("Kick button index : " + index);
        Player[] players = FindObjectsOfType<Player>();

        foreach (Player player in players)
        {
            if (team == "Team1" && player.HostPlayerNameTextReference == HostTeam1PlayerName[index])
            {
                player.Rpc_RemovedPlayerByHost();
            }
            
            else if (team == "Team2" && player.HostPlayerNameTextReference == HostTeam2PlayerName[index])
            {
                player.Rpc_RemovedPlayerByHost();

            }
        }
        Invoke("ActiveOrDeactiveKickButt", 1f);
    }

    private void ActiveOrDeactiveKickButt()
    {
        ActiveOrDeactiveKickButton();
    }

    public void ActiveOrDeactiveKickButton()
    {
        for (int i = 0; i < HostTeam1PlayerName.Length; i++)
        {
            if (string.IsNullOrEmpty(HostTeam1PlayerName[i].text))
            {
                HostTeam1KickButton[i].gameObject.SetActive(false);
            }
            else
            {
                if (OnlineMultiplayerManager.Instance.team == "Team1" && OnlineMultiplayerManager.Instance.index == i)
                {
                    continue;
                }
                HostTeam1KickButton[i].gameObject.SetActive(true);
            }
        }

        for (int i = 0; i < HostTeam2PlayerName.Length; i++)
        {
            if (string.IsNullOrEmpty(HostTeam2PlayerName[i].text))
            {
                HostTeam2KickButton[i].gameObject.SetActive(false);
            }
            else
            {
                if (OnlineMultiplayerManager.Instance.team == "Team2" && OnlineMultiplayerManager.Instance.index == i)
                {
                    continue;
                }
                HostTeam2KickButton[i].gameObject.SetActive(true);
            }
        }
    }

    public void JoinLobbyButtonClick()
    {
        JoinLobbyButton.interactable = false;
        CodeInputField.SetActive(true);
        EnterRoomButton.gameObject.SetActive(true);

    }
   
    public void OnClickEditNameButton()
    {
        foreach (TMP_InputField playernameinputfield in PlayerNameInput)
        {
            playernameinputfield.interactable = true; // Enable editing
            playernameinputfield.Select();
        }

        foreach (Button editbutton in EditButton)
        {
            editbutton.gameObject.SetActive(false);
        }
        foreach (Button savebutton in SaveButton)
        {
            savebutton.gameObject.SetActive(true);
        }

    }
    public void OnClickSaveNameButton(TMP_InputField playerNameInputText)
    {
        PlayerPrefs.SetString(OnlineMultiplayerManager.Instance.PlayerNameKey, playerNameInputText.text);
        PlayerPrefs.Save();

        foreach (TMP_InputField playernameinputfield in PlayerNameInput)
        {
            playernameinputfield.text = playerNameInputText.text;
            playernameinputfield.interactable = false;

        }
        Player[] players = FindObjectsOfType<Player>();

        foreach (Player player in players)
        {
            if (player.HasStateAuthority)
            {
                player.Name = playerNameInputText.text;
            }
        }
        foreach (Button editbutton in EditButton)
        {
            editbutton.gameObject.SetActive(true);
        }
        foreach (Button savebutton in SaveButton)
        {
            savebutton.gameObject.SetActive(false);
        }
    }

    public void ClickOnBackButtonFromHostorClientButton()
    {
        JoinLobbyButton.interactable = true;
        CodeInputField.SetActive(false);
        EnterRoomButton.gameObject.SetActive(false);
        Panels[(int)PanelType.SaccadeMenuClientOrHost].SetActive(false);
        Panels[(int)PanelType.SaccadeMenuMultiplayer].SetActive(true);
    }

    public void GoToScoreBoard()
    {      
        //OLD
       /* Team1BoardScoreText.text = FusionManager.Instance.Team1ScoreNetworked.ToString();
        Team2BoardScoreText.text = FusionManager.Instance.Team2ScoreNetworked.ToString();*/

        AnimationManager.Instance.GamePlayAnimation.SetActive(false);
        SpectModeMenuButton.SetActive(false);
        AllCanvas.SetActive(true);
        GeneralGamePanels[(int)GeneralPanelType.SaccadeMenuHost].SetActive(false);
        //Panels[(int)PanelType.SaccadeMenuMultiplayer].SetActive(false);
        //Panels[(int)PanelType.SaccadeMenuSpectator].SetActive(false);

        //Panels[(int)PanelType.SaccadeScoreBoard].SetActive(true);// change it
        GeneralModule.Instance.ScorePanelOpenClose(true, OnlineMultiplayerManager.Instance.Team1ScoreNetworked, OnlineMultiplayerManager.Instance.Team2ScoreNetworked);
    }

    public void GoToMenu()
    {
        Debug.Log("Called go to menu from saccade module");
        if (OnlineMultiplayerManager.Instance.IsThisHostPlayer)
        {
            Player[] players = FindObjectsOfType<Player>();
            foreach (Player player in players)
            {
                if (player.HasStateAuthority)
                {
                    player.IsHostGoToMenu = !player.IsHostGoToMenu;
                }
            }
            return;
        }
        openMenu();
    }

    private void openMenu()
    {
        GeneralModule.Instance.scorePanel.SetActive(false);

        Player[] players = FindObjectsOfType<Player>();

        foreach (Player player in players)
        {
            player.IsGameStart = false;
        }

        OnlineMultiplayerManager.Instance.Team1ScoreNetworked = 0;
        OnlineMultiplayerManager.Instance.Team2ScoreNetworked = 0;
        /*Team1BoardScoreText.text = "0";
        Team2BoardScoreText.text = "0";*/
        Team1GameScore.text = "0";
        Team2GameScore.text = "0";

        if (OnlineMultiplayerManager.Instance.Mode == "Single")
        {
            //Panels[(int)PanelType.SaccadeScoreBoard].SetActive(false);
            Panels[(int)PanelType.SaccadeMenuMultiplayer].SetActive(true);
        }
        else if (OnlineMultiplayerManager.Instance.Mode == "Multiplayer")
        {
            if (OnlineMultiplayerManager.Instance.IsThisHostPlayer)
            {
                
                //Panels[(int)PanelType.SaccadeScoreBoard].SetActive(false);
                //Panels[(int)PanelType.SaccadeMenuSpectator].SetActive(false);
                GeneralGamePanels[(int)GeneralPanelType.SaccadeMenuClient].SetActive(false);
                GeneralGamePanels[(int)GeneralPanelType.SaccadeMenuHost].SetActive(true);
                AnimationManager.Instance.GamePlayAnimation.SetActive(false);
                AllCanvas.SetActive(true);

            }
            else
            {
                GeneralGamePanels[(int)GeneralPanelType.SaccadeMenuHost].SetActive(false);
               // Panels[(int)PanelType.SaccadeScoreBoard].SetActive(false);
                GeneralGamePanels[(int)GeneralPanelType.SaccadeMenuClient].SetActive(true);
            }

            OnlineMultiplayerManager.Instance.GameOver();
        }


    }

    public void GoToMenuByHost()
    {
        openMenu();
    }
    public void startGameButtonClick(string mode)
    {
        OnlineMultiplayerManager.Instance.Mode = mode;

        if (mode == "Single")
        {
            CloseAllPanelAndActiveSpecific();
        }
        else if (mode == "Multiplayer")
        {
           
            if (OnlineMultiplayerManager.Instance.EnoughPlayersInTeamToStartGame())
            {
                Player[] players = FindObjectsOfType<Player>();

                foreach (Player player in players)
                {
                    if (player.HasStateAuthority)
                    {
                        player.Rpc_IntroAnimation();
                    }
                }
            }
            else
            {
                Debug.Log("Not Enough Player In a Team");
            }

        }

    }

    public void CloseAllPanelAndActiveSpecific()
    {
        foreach (GameObject panel in Panels)
        {
            panel.SetActive(false);

        }
        Panels[(int)PanelType.SaccadeGamePlay].SetActive(true);

    }

    public void LeaveSessionByHost()
    {
        if (OnlineMultiplayerManager.Instance.IsThisHostPlayer)
        {
            Player[] players = FindObjectsOfType<Player>();

            foreach (Player player in players)
            {
                if (!player.HasStateAuthority)
                {
                    player.Rpc_LobbyClosedByHost();
                }

            }
            Invoke(nameof(LeaveSession), 2f);
        }
    }

    public void LeaveSession()
    {                
        TeamPlayersManager.Instance.TotalPlayersJoinedAnyOneTeam = 0;
        TeamPlayersManager.Instance.Team1TotalPlayers = 0;
        TeamPlayersManager.Instance.Team2TotalPlayers = 0;

        Team1GameScore.text = "0";
        Team2GameScore.text = "0";
       /* Team1BoardScoreText.text = "0";
        Team2BoardScoreText.text = "0";*/

        for (int i = 0; i < HostTeam1PlayerName.Length; i++)
        {
            HostTeam1PlayerName[i].text = "";
            HostTeam2PlayerName[i].text = "";
            ClientTeam1PlayerName[i].text = "";
            ClientTeam2PlayerName[i].text = "";

            HostTeam1OccupiedPlace[i].Value = false;
            HostTeam2OccupiedPlace[i].Value = false;
            ClientTeam1OccupiedPlace[i].Value = false;
            ClientTeam2OccupiedPlace[i].Value = false;
        }

        for (int i = 0; i < HostTeam1PlayerAvtar.Length; i++)
        {
            HostTeam1PlayerAvtar[i].sprite = null;
            HostTeam2PlayerAvtar[i].sprite = null;
            ClientTeam1PlayerAvtar[i].sprite = null;
            ClientTeam2PlayerAvtar[i].sprite = null;
        }


        Team1HostLeaveButton.gameObject.SetActive(false);
        Team1HostJoinButton.gameObject.SetActive(false);
        Team2HostLeaveButton.gameObject.SetActive(false);
        Team2HostJoinButton.gameObject.SetActive(false);
        Team1ClientLeaveButton.gameObject.SetActive(false);
        Team1ClientJoinButton.gameObject.SetActive(false);
        Team2ClientLeaveButton.gameObject.SetActive(false);
        Team2ClientJoinButton.gameObject.SetActive(false);
        OnlineMultiplayerManager.Instance.team = "";
        OnlineMultiplayerManager.Instance.index = -1;

        OnlineMultiplayerManager.Instance.ConnectAndJoinLobby();
        if (OnlineMultiplayerManager.Instance.IsThisHostPlayer)
        {
            Panels[(int)PanelType.SaccadeGamePlay].SetActive(false);
            GeneralGamePanels[(int)GeneralPanelType.SaccadeMenuHost].SetActive(false);
            //Panels[(int)PanelType.SaccadeScoreBoard].SetActive(false);
            Panels[(int)PanelType.SaccadeMenuMultiplayer].SetActive(true);
        }
        else
        {
            Panels[(int)PanelType.SaccadeGamePlay].SetActive(false);
            GeneralGamePanels[(int)GeneralPanelType.SaccadeMenuClient].SetActive(false);
            //Panels[(int)PanelType.SaccadeScoreBoard].SetActive(false);
            Panels[(int)PanelType.SaccadeMenuMultiplayer].SetActive(true);
        }

        OnlineMultiplayerManager.Instance.IsThisHostPlayer = false;
    }
    public void ClickOnSwitchTeam()
    {
        OnlineMultiplayerManager.Instance.OwnPlayerNetworkObject.GetComponent<Player>().SwitchTeam();
    }

    public void OnClickTeam1JoinButton()
    {
        PlayerIsInTeamTemporary = true;
        OnClickJoinOrSwitchTeam(1);


    }

    public void OnClickTeam1LeaveButton()
    {
        PlayerIsInTeamTemporary = false;
        ResetAllPlayerVales();

    }

    public void OnClickTeam2JoinButton()
    {
        PlayerIsInTeamTemporary = true;
        OnClickJoinOrSwitchTeam(2);

    }

    private void ResetAllPlayerVales()
    {
        Player[] players = FindObjectsOfType<Player>();

        foreach (Player player in players)
        {
            if (player.HasStateAuthority)
            {
                player.ResetValues = !player.ResetValues;
            }
        }
    }

    public void OnClickTeam2LeaveButton()
    {
        PlayerIsInTeamTemporary = false;
        ResetAllPlayerVales();

    }

    private void OnClickJoinOrSwitchTeam(int teamNumber)
    {
        Player[] players = FindObjectsOfType<Player>();

        foreach (Player player in players)
        {
            if (player.playerIsInTeam)
            {
                if (player.HasStateAuthority)
                {
                    Debug.Log("Called swtich team from click on join or switch team");
                    player.PlayerIsInTeamTemporary = true;
                    player.SwitchTeam();
                }

            }
            else
            {
                if (player.HasStateAuthority) // Ensure this only executes on the State Authority
                {
                    Debug.Log("Called Joined team from click on join or switch team");
                    player.Rpc_JoinPlayerInAnyTeam(false, teamNumber);
                }

            }

        }
    }
}
