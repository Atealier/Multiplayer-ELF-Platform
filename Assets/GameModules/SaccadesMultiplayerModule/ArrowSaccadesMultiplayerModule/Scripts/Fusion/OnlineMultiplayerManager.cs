using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class OnlineMultiplayerManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static OnlineMultiplayerManager Instance { get; private set; }
    public static NetworkRunner networkRunner;
    
    [HideInInspector] public Dictionary<PlayerRef, Player> playerDictionary = new Dictionary<PlayerRef, Player>();
    [HideInInspector] public string Mode;
    [HideInInspector] public bool IsThisHostPlayer = false;
    [SerializeField] GameObject PlayerPrefab;
    public Button JoinBtn, HostBtn;

    public float Team1ScoreNetworked;
    public float Team2ScoreNetworked;

    public GameObject networkRunnerObj;
    private List<SessionInfo> SessionInfoList;
    public NetworkObject OwnPlayerNetworkObject;

    public bool PlayerIsInTeamTemporary = true;

    public string PlayerNameKey = "PlayerName"; // Key to store the player name in PlayerPrefs

    public string team;
    public int index = -1;
    
    public GameObject Team1CharacterGameObject;
    public GameObject Team2CharacterGameObject;

    public Sprite PlayerAvtar;
    
    public int[] playerPossibleEvenNumber = { 2, 4, 6, 8, 10, 12, 14, 16, 18, 20 };
    public int[] playerPossibleOddNumber = { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19 };

    public bool disableAllCanvasForShowingAnimations = false;

    public bool isScoreShowed = false;
    public bool isConnected;

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

    
    private void Update()
    {

    }

    #region Fusion2 Callbacks
    public void OnConnectedToServer(NetworkRunner runner)
    {

    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.LocalPlayer == player)
        {
            OwnPlayerNetworkObject = runner.Spawn(PlayerPrefab, Vector3.zero, Quaternion.identity);
        }
        OnlineMultiplayerUIManager.Instance.StartGameButton.interactable = EnoughPlayersInTeamToStartGame() ? true : false;
    }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (playerDictionary.TryGetValue(player, out Player playerScript))
            {
                Debug.Log("Before remove Player dictionary count" + playerDictionary.Values.Count());
                playerDictionary.Remove(player);
                Debug.Log("After remove Player dictionary count" + playerDictionary.Values.Count());
                //bool hostExists = false;
                //foreach (Player player1 in playerDictionary.Values)
                //{
                //    if(player1.IsThisHostPlayer && player1 != playerScript)
                //    {
                //        hostExists = true;
                //    }
                //}

                 bool hostExists = playerDictionary.Values.Any(p =>p.IsThisHostPlayer && p != playerScript);
            Debug.Log("Host exists " + hostExists);
                if (!hostExists)
                {
                    LeaveSessionHostLeave();
                    playerDictionary.Clear();
                    return;
                }

                if (playerScript.localPlayerJoiningNumber % 2 == 0)
                {
                    TeamPlayersManager.Instance.Team2TotalPlayers--;
                    int index = Array.IndexOf(GeneralModule.Instance.playerPossibleEvenNumber, playerScript.localPlayerJoiningNumber);
                    OnlineMultiplayerUIManager.Instance.HostTeam2OccupiedPlace[index].Value = false;
                    OnlineMultiplayerUIManager.Instance.ClientTeam2OccupiedPlace[index].Value = false;
                }
                else
                {
                    TeamPlayersManager.Instance.Team1TotalPlayers--;
                    int index = Array.IndexOf(GeneralModule.Instance.playerPossibleOddNumber, playerScript.localPlayerJoiningNumber);
                    OnlineMultiplayerUIManager.Instance.HostTeam1OccupiedPlace[index].Value = false;
                    OnlineMultiplayerUIManager.Instance.ClientTeam1OccupiedPlace[index].Value = false;
                }

                Debug.Log("Set false reference");
                playerScript.HostPlayerNameTextReference.text = "";
                playerScript.ClientPlayerNameTextReference.text = "";

                playerScript.HostPlayerOccupiedPlaceReference.Value = false;
                playerScript.ClientPlayerOccupiedPlaceReference.Value = false;

                playerScript.HostPlayerAvtarReference.sprite = null;
                playerScript.ClientPlayerAvtarReference.sprite = null;

                Debug.LogWarning("Player removed " + playerScript.localPlayerJoiningNumber);
                FindObjectOfType<PlayerAvtarInSpectMode>()?.RemoveAvatar(playerScript.localPlayerJoiningNumber);

            }

            TeamPlayersManager.Instance.TotalPlayersJoinedAnyOneTeam--;
            TeamPlayersManager.Instance.SetScoreValueForEachPlayer();
            OnlineMultiplayerUIManager.Instance.ActiveOrDeactiveKickButton();
            OnlineMultiplayerUIManager.Instance.StartGameButton.interactable = EnoughPlayersInTeamToStartGame() ? true : false;

            if ((TeamPlayersManager.Instance.Team1TotalPlayers == 0 || TeamPlayersManager.Instance.Team2TotalPlayers == 0) && playerDictionary.Values.Any(p => p.IsGameStart))
            {
                Debug.LogWarning("LAST PLAYER REMOVED ");
                GeneralModule.Instance.CurrentSaccadesModuleHandler.LastPlayerRemoved();
                
            }
        }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        SessionInfoList.Clear();
        SessionInfoList = sessionList;

    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log("On shut down");
        isConnected = false;
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }

    #endregion

    public void OnChangeTeam1Score()
    {
        Debug.Log("CHANGE SCORE : " + Team1ScoreNetworked.ToString());
        OnlineMultiplayerUIManager.Instance.UpdateScores(Team1ScoreNetworked, Team2ScoreNetworked);

        if (IsThisHostPlayer && !OnlineMultiplayerUIManager.Instance.PlayerIsInTeamTemporary)
        {
            AnimationManager.Instance.UpdateCharacterAnimations(Team1ScoreNetworked, Team2ScoreNetworked);
            AnimationManager.Instance.UpdateTugOfWar(Team1ScoreNetworked, Team2ScoreNetworked);
        }
    }

    public void OnChangeTeam2Score()
    {
        Debug.Log("Change score : " + Team2ScoreNetworked.ToString());
        OnlineMultiplayerUIManager.Instance.UpdateScores(Team1ScoreNetworked, Team2ScoreNetworked);
        if (IsThisHostPlayer && !OnlineMultiplayerUIManager.Instance.PlayerIsInTeamTemporary)
        {
            AnimationManager.Instance.UpdateCharacterAnimations(Team1ScoreNetworked, Team2ScoreNetworked);
            AnimationManager.Instance.UpdateTugOfWar(Team1ScoreNetworked, Team2ScoreNetworked);
        }
    }

    #region UI Handlers

    public async void HostButtonClick()
    {
        if (!isConnected)
        {
            await ConnectAndJoinLobby();
        }

        if (!isConnected)
        {
            // Still not connected after trying — just return
            return;
        }

        IsThisHostPlayer = true;
        if (OnlineMultiplayerUIManager.Instance == null)
        {
            Debug.Log("UI manager instance null");
        }
        OnlineMultiplayerUIManager.Instance.ShowPanel((int)PanelType.SaccadeMenuMultiplayer, false);
        OnlineMultiplayerUIManager.Instance.ShowGeneralGamePanel((int)GeneralPanelType.SaccadeMenuHost, true);
        OnlineMultiplayerUIManager.Instance.JoinLobbyButton.gameObject.SetActive(false);
        OnlineMultiplayerUIManager.Instance.StartGameButton.interactable = EnoughPlayersInTeamToStartGame();
        string roomCode = GenerateRandomCode(4);
        OnlineMultiplayerUIManager.Instance.SetRoomCode(roomCode);
        StartGame(GameMode.Shared, roomCode);

    }
    public void HostLobyButtonClick()
    {
        OnlineMultiplayerUIManager.Instance.StartGameButton.interactable = EnoughPlayersInTeamToStartGame();
        string roomCode = GenerateRandomCode(4);
        OnlineMultiplayerUIManager.Instance.SetRoomCode(roomCode);
        OnlineMultiplayerUIManager.Instance.ShowPanel(0, false);
        OnlineMultiplayerUIManager.Instance.ShowPanel(1, false);
        OnlineMultiplayerUIManager.Instance.ShowPanel(2, true); // SaccadeMenuAsAHost
        StartGame(GameMode.Shared, roomCode);

    }
    public async void EnterRoomButtonClick()
    {
        if(!isConnected)
        {
            await ConnectAndJoinLobby();
        }

        if (!isConnected)
        {
            // Still not connected after trying — just return
            return;
        }

        GameObject codeInputField = OnlineMultiplayerUIManager.Instance.CodeInputField;
        if (!string.IsNullOrEmpty(codeInputField.GetComponent<TMP_InputField>().text))
        {
            if (SessionInfoList.Count > 0)
            {
                foreach (SessionInfo session in SessionInfoList)
                {
                    Debug.Log("Session name : " + session.Name);
                    if (session.Name.ToLower() == codeInputField.GetComponent<TMP_InputField>().text.ToLower())
                    {
                        StartGame(GameMode.Shared, session.Name);
                        Debug.Log("Room joined successfully!");

                        codeInputField.GetComponent<TMP_InputField>().text = "";
                        codeInputField.SetActive(false);
                        OnlineMultiplayerUIManager.Instance.JoinLobbyButton.interactable = true;
                        OnlineMultiplayerUIManager.Instance.CodeInputField.SetActive(false);
                        OnlineMultiplayerUIManager.Instance.EnterRoomButton.gameObject.SetActive(false);
                        OnlineMultiplayerUIManager.Instance.Panels[(int)PanelType.SaccadeMenuClientOrHost].SetActive(false);
                        OnlineMultiplayerUIManager.Instance.GeneralGamePanels[(int)GeneralPanelType.SaccadeMenuClient].SetActive(true);
                        break;

                    }
                    else
                    {
//                        OnlineMultiplayerUIManager.Instance.ErrorMsg.SetActive(true);
                        Invoke("DeactiveErrorMsg", 2f);
                        Debug.Log("This room does not exists please enter right code ");
                    }
                }
            }
            else
            {
            //    OnlineMultiplayerUIManager.Instance.ErrorMsg.SetActive(true);
                Invoke("DeactiveErrorMsg", 2f);
                Debug.Log("No any room available for this code ");
            }

        }
        else
        {
           // OnlineMultiplayerUIManager.Instance.ErrorMsg.SetActive(true);
            Invoke("DeactiveErrorMsg", 2f);
            Debug.Log("Room name is null");
        }

    }

    public void LeaveSessionHostLeave()
    {
        Player[] players = FindObjectsOfType<Player>();


        foreach (Player player in players)
        {
            if (player.HasStateAuthority)
            {
                IsThisHostPlayer = false;
                player.Rpc_LobbyClosedByHost();
            }
        }

    }

    #endregion

    async void Start()
    {
        Debug.Log("IN START");
        //JoinBtn.interactable = false;
        //HostBtn.interactable = false;
        SessionInfoList = new List<SessionInfo>();

        await ConnectAndJoinLobby();

    }
    void DeactiveErrorMsg()
    {
       // OnlineMultiplayerUIManager.Instance.ErrorMsg.SetActive(false);
    }
    #region Other Methods


    public bool EnoughPlayersInTeamToStartGame()
    {
        if (TeamPlayersManager.Instance.Team1TotalPlayers > 0 && TeamPlayersManager.Instance.Team2TotalPlayers > 0)
        {
            return true;
        }
        else
        {
            return false;
        }

    }
     
    public IEnumerator HandleIntroAnimationAndPlayerLogic()
    {
        // Wait for the inner coroutine to complete
        yield return StartCoroutine(IntroAnimationManager.Instance.HandleIntroAnimationAndPlayerLogic());
        
        if (IntroAnimationManager.Instance.isIntroAnimCompleted)
        {
            if (IsThisHostPlayer)
            {
                Player[] players = FindObjectsOfType<Player>();
                foreach (Player player in players)
                {
                    if (player.HasStateAuthority)
                    {
                        GeneralModule.Instance.CurrentSaccadesModuleHandler.SetSaccadeScoreValue(0);
                        
                        player.IsGameStart = true;
                    }
                }
            }
        }
    }


    public void GameOver()
    {
        Player[] players = FindObjectsOfType<Player>();

        //team1WinParticle.SetActive(false);
        //team2WinParticle.SetActive(false);

        foreach (Player player in players)
        {
            if (player.HasStateAuthority)
            {
                player.IsGameStart = false;
                player.Score = 0;
            }
        }
    }

    private string GenerateRandomCode(int length)
    {
        const string chars = "0123456789";
        char[] codeChars = new char[length];
        System.Random random = new System.Random();

        for (int i = 0; i < length; i++)
        {
            codeChars[i] = chars[random.Next(chars.Length)];
        }

        return new string(codeChars);
    }


    public void ResetConnection()
    {
        if (networkRunner != null)
        {
            if (networkRunnerObj != null)
            {
                Destroy(networkRunnerObj); // Explicitly destroy only the runner GameObject
            }

            networkRunner = null;
            networkRunnerObj = null;
        }
    }

    public async Task ConnectAndJoinLobby()
    {
        //if (isConnected) return;

        if (JoinBtn != null)
        JoinBtn.interactable = false;

        if (HostBtn != null)
        HostBtn.interactable = false;
        await Task.Delay(2000);

        playerDictionary.Clear();

        if (networkRunner != null)
        {
             Debug.Log("Called ConnectAndJoinLobby");
            // Only shutdown the NetworkRunner without destroying other objects
            await networkRunner.Shutdown(false);

            if (networkRunnerObj != null)
            {
                Destroy(networkRunnerObj); // Explicitly destroy only the runner GameObject
            }
            isConnected = false;
            networkRunner = null;
            networkRunnerObj = null;
        }

        // Create a new NetworkRunner GameObject
        networkRunnerObj = new GameObject("NetworkRunner");
        networkRunner = networkRunnerObj.AddComponent<NetworkRunner>();
        networkRunner.ProvideInput = true;
        networkRunner.AddCallbacks(this);

        // Attempt to join the session lobby asynchronously
        var result = await networkRunner.JoinSessionLobby(SessionLobby.Shared);

        if (result.Ok)
        {
           
            Debug.Log("Successfully joined the session lobby.");
            isConnected = true;
            TeamPlayersManager.Instance.Team1TotalPlayers = 0;
            TeamPlayersManager.Instance.Team2TotalPlayers = 0;
            TeamPlayersManager.Instance.TotalPlayersJoinedAnyOneTeam = 0;
            // Optionally, update UI or proceed with further steps
        }
        else
        {
            Debug.LogError($"Failed to join session lobby: {result.ShutdownReason}" + " " + GeneralModule.Instance.CurrentSaccadesModuleErrorMsg.Count());
            foreach(GameObject errorMsg in GeneralModule.Instance.CurrentSaccadesModuleErrorMsg)
            {
                errorMsg.SetActive(true);
            }
            StartCoroutine(HideErrorMessagesAfterDelay(3f));
        }
        JoinBtn.interactable = true;
        HostBtn.interactable = true;


    }

    private IEnumerator HideErrorMessagesAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (GameObject errorMsg in GeneralModule.Instance.CurrentSaccadesModuleErrorMsg)
        {
            errorMsg.SetActive(false);
        }
    }

    async void StartGame(GameMode mode, string roomCode)
    {
        isScoreShowed = false;

        string roomcode = roomCode;

        if (string.IsNullOrEmpty(roomcode))
        {
            roomcode = roomcode + UnityEngine.Random.Range(0, 1000);
        }

        // Create the NetworkSceneInfo from the current scene
        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();

        if (scene.IsValid)
        {
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);

        }

        // Start or join (depends on gamemode) a session with a specific name
        await networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = roomcode,
            Scene = scene,
            SceneManager = networkRunnerObj.AddComponent<NetworkSceneManagerDefault>(),

        });

    }

    public void OpenScoreBoard()
    {
        Player[] players = FindObjectsOfType<Player>();

        if (!isScoreShowed)
        {
            isScoreShowed = true;

            int t1Score = Mathf.RoundToInt(Team1ScoreNetworked);
            int t2Score = Mathf.RoundToInt(Team2ScoreNetworked);

            Player hostPlayer = null;

            foreach (Player player in players)
            {
                if (player.Object != null && player.Object.IsValid && player.IsThisHostPlayer)
                {
                    hostPlayer = player;
                }

                if (!player.HasStateAuthority) continue;

                bool amIInTeam1 = player.PlayerJoiningNumber % 2 != 0;
                bool amIInTeam2 = player.PlayerJoiningNumber % 2 == 0;
                bool iWon = (amIInTeam1 && t1Score > t2Score) || (amIInTeam2 && t2Score > t1Score);
                bool isDraw = t1Score == t2Score;

                if (player.IsThisHostPlayer && player.PlayerIsInTeamTemporary)
                {
                    // ✅ Host is playing → immediate
                    if (isDraw) GameSoundManager.Instance.PlayVictorySound();
                    else if (iWon) GameSoundManager.Instance.PlayVictorySound();
                    else GameSoundManager.Instance.PlayChargingSound1();

                    player.IsGameStart = false;
                    player.Rpc_ShowResultAnimation();

                    if (Mode == "Single")
                    {
                        OnlineMultiplayerUIManager.Instance.Panels[(int)PanelType.SaccadeGamePlay].SetActive(false);
                        GeneralModule.Instance.ScorePanelOpenClose(true,
                            float.Parse(OnlineMultiplayerUIManager.Instance.Team1GameScore.text),
                            float.Parse(OnlineMultiplayerUIManager.Instance.Team2GameScore.text));
                    }
                    else if (Mode == "Multiplayer")
                    {
                        OnlineMultiplayerUIManager.Instance.Panels[(int)PanelType.SaccadeGamePlay].SetActive(false);
                        GeneralModule.Instance.ScorePanelOpenClose(true, Team1ScoreNetworked, Team2ScoreNetworked);
                    }
                }
                else
                {
                    // ✅ Normal playing client
                    if (isDraw) GameSoundManager.Instance.PlayVictorySound();
                    else if (iWon) GameSoundManager.Instance.PlayVictorySound();
                    else GameSoundManager.Instance.PlayChargingSound1();

                    player.IsGameStart = false;

                    if (Mode == "Single")
                    {
                        OnlineMultiplayerUIManager.Instance.Panels[(int)PanelType.SaccadeGamePlay].SetActive(false);
                        GeneralModule.Instance.ScorePanelOpenClose(true,
                            float.Parse(OnlineMultiplayerUIManager.Instance.Team1GameScore.text),
                            float.Parse(OnlineMultiplayerUIManager.Instance.Team2GameScore.text));
                    }
                    else if (Mode == "Multiplayer")
                    {
                        OnlineMultiplayerUIManager.Instance.Panels[(int)PanelType.SaccadeGamePlay].SetActive(false);
                        GeneralModule.Instance.ScorePanelOpenClose(true, Team1ScoreNetworked, Team2ScoreNetworked);
                    }

                    // ✅ Client manually triggers delayed scoreboard for spectating host
                    if (hostPlayer != null && !hostPlayer.PlayerIsInTeamTemporary)
                    {
                        hostPlayer.Rpc_ShowSpectatorResultAfterDelay();
                    }
                }
            }
        }
    }

    public void ConsecutiveCorrectAnswerAnimation(string Team)
    {
        if(IsThisHostPlayer && !PlayerIsInTeamTemporary)
        {
            if(Team == "Team1")
            {   
                AnimationManager.Instance.Team1Character.UpdateAnimationBasedConsecutiveAnswers();
            }
            else
            {
                AnimationManager.Instance.Team2Character.UpdateAnimationBasedConsecutiveAnswers();
            }
        }    
    }

    public void ConsecutiveHalfCorrectAnswerAnimation(string Team)
    {
        if (IsThisHostPlayer && !PlayerIsInTeamTemporary)
        {
            if (Team == "Team1")
            {
              
                AnimationManager.Instance.Team1Character.UpdateAnimationBasedHalfConsecutiveAnswers();
            }
            else
            {
              
                AnimationManager.Instance.Team2Character.UpdateAnimationBasedHalfConsecutiveAnswers();
            }
        }
    }

    public void ConsecutiveCorrectAnswer()
    {
        Player[] players = FindObjectsOfType<Player>();

        foreach (Player player in players)
        {
            if (player.HasStateAuthority)
            {          
                player.Consecutive = !player.Consecutive;
            }
        }
    }



    public void ConsecutiveHalfCorrectAnswer()
    {
        Player[] players = FindObjectsOfType<Player>();
        foreach (Player player in players)
        {
            
            if (player.HasStateAuthority)
            {
                player.HalfConsecutive = !player.HalfConsecutive;
            }
        }
    }

    public void CloseAllPanelAndActiveSpect()
    {
        foreach (GameObject panel in OnlineMultiplayerUIManager.Instance.Panels)
        {
            panel.SetActive(false);

        }
       
      //  OnlineMultiplayerUIManager.Instance.Panels[(int)PanelType.SaccadeMenuSpectator].SetActive(true);

    }

    public void OnChangeSingleplayerScore(int localPlayeScore, int botPlayerScore)
    {
        Debug.Log("Called single player score change");

        OnlineMultiplayerUIManager.Instance.Team1GameScore.text = localPlayeScore.ToString();
        OnlineMultiplayerUIManager.Instance.Team2GameScore.text = botPlayerScore.ToString();
    }
    public void OnChangeMultiplayerScore(int localPlayeScore)
    {
        Debug.Log("Called multiplayer score change");

        Player[] players = FindObjectsOfType<Player>();

        foreach (Player player in players)
        {
            if (player.HasStateAuthority)
            {
                Debug.Log("Player score first time : " + player.Score);
                player.Score = localPlayeScore;
            }
        }
    }
   

    #endregion
    private async void OnApplicationQuit()
    {
        if (networkRunner != null && networkRunner.IsRunning)
        {
            await networkRunner.Shutdown();
        }
    }

    private async void OnDisable()
    {
        if (networkRunner != null)
        {
            // Only shutdown the NetworkRunner without destroying other objects
            await networkRunner.Shutdown(false);

            if (networkRunnerObj != null)
            {
                Destroy(networkRunnerObj); // Explicitly destroy only the runner GameObject
            }
            isConnected = false;
            networkRunner = null;
            networkRunnerObj = null;
        }

    }

    
}
