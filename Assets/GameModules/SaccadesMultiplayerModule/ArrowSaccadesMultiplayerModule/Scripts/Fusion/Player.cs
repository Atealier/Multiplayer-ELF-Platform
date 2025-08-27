    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Fusion;
    using TMPro;
    using System;
    using UnityEngine.UI;

    public class Player : NetworkBehaviour,IPlayerLeft, IAvatarEntity
    {
        public bool playerIsInTeam;
        
        public int localPlayerJoiningNumber;

        [HideInInspector]
        public TextMeshProUGUI HostPlayerNameTextReference, ClientPlayerNameTextReference;
        public BoolWrapper HostPlayerOccupiedPlaceReference, ClientPlayerOccupiedPlaceReference;
        public Image HostPlayerAvtarReference, ClientPlayerAvtarReference;

        public int[] playerPossibleEvenNumber = { 2, 4, 6, 8, 10, 12, 14, 16, 18, 20 };
        public int[] playerPossibleOddNumber = { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19 };
   
        [Header("For Network Properties")]

        [Networked, OnChangedRender(nameof(StartGamePanel))]
        public bool IsGameStart { get; set; } = false;

        [Networked, OnChangedRender(nameof(onChangeScore))]
        public float Score { get; set; }

        [Networked, OnChangedRender(nameof(OnChangePlayerName))]
        public string Name { get; set; } = null;

        public Sprite PlayerAvtar;
    
        [Networked]
        public int PlayerAvtarIndex { get; set; }

        [Networked, OnChangedRender(nameof(OnChangePlayerAvtar))]
        public bool ChangedPlayerAvtar { get; set; }


        [Networked, OnChangedRender(nameof(OnChangeSwitchTeam))]
        public bool SwitchTeamPropety { get; set; }

        [Networked]
        public int PlayerJoiningNumber { get; set; }
    
        [Networked]
        public int PlayerPositionFix { get; set; } = -1;

        [Networked,OnChangedRender(nameof(OnChageResetValues))]
        public bool ResetValues { get; set; }

        [Networked, OnChangedRender(nameof(GoToMenuByHost))]
        public bool IsHostGoToMenu { get; set; } = false;

        [Networked]
        public bool PlayerIsInTeamTemporary { get; set; } = true;

        [Networked]
        public bool IsThisHostPlayer { get; set; } = false;

        [Networked, OnChangedRender(nameof(ConsecutiveAnimation))]
        public bool Consecutive { get; set; } = false;

        [Networked, OnChangedRender(nameof(HalfConsecutiveAnimation))]
        public bool HalfConsecutive { get; set; } = false;

        bool ManualSwitchTeam = true;

        public bool localPositionSet = false;

        [Networked, OnChangedRender(nameof(OnConsecutiveCorrectAnswersChanged))]
        public int ConsecutiveCorrectAnswers { get; set; }

        #region IavatarMethods

        public int GetAvatarIndex()
        {
            return PlayerAvtarIndex;
        }

        public int GetConsecutiveCorrectAnswers()
        {
            return ConsecutiveCorrectAnswers;
        }

        public string GetEntityName()
        {
            return Name ?? gameObject.name;
        }

        public bool IsHostControlled()
        {
            return IsThisHostPlayer;
        }

        public int GetJoiningNumber()
        {
            return localPlayerJoiningNumber;
        }

        public int initialJoiningNumber()
        {
            return PlayerJoiningNumber;
        }

        public bool ApplyGreenBorder() => ConsecutiveCorrectAnswers >= GeneralModule.Instance.CurrentSaccadesModuleHandler.GetAvtarBorderHighlightThreshold();
        public bool ActivateRoundParticles() => (ConsecutiveCorrectAnswers >= GeneralModule.Instance.CurrentSaccadesModuleHandler.GetAvtarRoundParticleThreshold() && ConsecutiveCorrectAnswers < GeneralModule.Instance.CurrentSaccadesModuleHandler.GetAvtarFlowParticleThreshold());
        public bool ActivateFlowParticles() => ConsecutiveCorrectAnswers >= GeneralModule.Instance.CurrentSaccadesModuleHandler.GetAvtarFlowParticleThreshold();

  
        #endregion IavatarMethods

        #region OnchangeRedrer Callback
        private void StartGamePanel()
        {
           // FusionManager.Instance.isScoreShowed = false;

            if (IsGameStart)
            {
               // GameSoundManager.Instance.PlayBgSound();  // SOUND BG


                if (OnlineMultiplayerManager.Instance.IsThisHostPlayer && !PlayerIsInTeamTemporary)
                {
                    OnlineMultiplayerManager.Instance.Mode = "Multiplayer";
                    OnlineMultiplayerManager.Instance.CloseAllPanelAndActiveSpect();
                    OnlineMultiplayerManager.Instance.disableAllCanvasForShowingAnimations = true;
                    OnlineMultiplayerUIManager.Instance.AllCanvas.SetActive(false);
                    AnimationManager.Instance.GamePlayAnimation.SetActive(true);
                    OnlineMultiplayerManager.Instance.PlayerIsInTeamTemporary = false;
                }
                else
                {
                    Debug.Log("Start game panel called");
                    OnlineMultiplayerManager.Instance.Mode = "Multiplayer";
                    OnlineMultiplayerUIManager.Instance.CloseAllPanelAndActiveSpecific();
                }            
            }
       
        }

        private void ConsecutiveAnimation()
        {
            Player[] players = FindObjectsOfType<Player>();


            if (OnlineMultiplayerManager.Instance.IsThisHostPlayer)
            {
                if (localPlayerJoiningNumber % 2 == 0)
                {
                    OnlineMultiplayerManager.Instance.ConsecutiveCorrectAnswerAnimation("Team2");
                }
                else
                {
                    OnlineMultiplayerManager.Instance.ConsecutiveCorrectAnswerAnimation("Team1");
                }
            }

            bool isHost = OnlineMultiplayerManager.Instance.IsThisHostPlayer;
            bool hostInTemporaryTeam = OnlineMultiplayerManager.Instance.PlayerIsInTeamTemporary;

            bool triggerIsTeam1 = (PlayerJoiningNumber % 2 != 0);

            foreach (Player p in players)
            {
                if (!p.HasStateAuthority) continue; // Skip non-authoritative copies

                bool iAmTeam1 = (p.PlayerJoiningNumber % 2 != 0);
                bool myTeamMatchesTrigger = (iAmTeam1 == triggerIsTeam1);

                bool isThisHost = (p == isHost);
                bool allowSound = (isThisHost && hostInTemporaryTeam) || (!isThisHost);   // Host is allowed only if in temp team  // All other players are allowed


                if (myTeamMatchesTrigger && allowSound)
                {
                    Debug.LogError("Sound played: My team matches trigger and allowed.");
                    GameSoundManager.Instance.PlayFullStreakSound();
                }
                else
                {
                    Debug.LogError("No sound: Team mismatch or not allowed.");
                }
            }

            #region old code
            /* bool triggerIsTeam1 = (PlayerJoiningNumber % 2 != 0);

             foreach (Player p in players)
             {

                 if (!p.HasStateAuthority) continue;                 // skip remote copies

                 bool iAmTeam1 = (p.PlayerJoiningNumber % 2 != 0);

                 if (iAmTeam1 == triggerIsTeam1)
                 {
                     Debug.LogWarning($" full-streak sound (my team matches trigger team)");
                     GameSoundManager.Instance.PlayFullStreakSound();
                 }
                 else
                 {
                     Debug.LogWarning($" opponent: no sound");
                 }

             }*/
            #endregion old code
        }
        private void HalfConsecutiveAnimation()
        {
            Player[] players = FindObjectsOfType<Player>();

            if (OnlineMultiplayerManager.Instance.IsThisHostPlayer)
            {
                if (localPlayerJoiningNumber % 2 == 0)
                {
                    OnlineMultiplayerManager.Instance.ConsecutiveHalfCorrectAnswerAnimation("Team2");
                }
                else
                {
                    OnlineMultiplayerManager.Instance.ConsecutiveHalfCorrectAnswerAnimation("Team1");
                }
            }


            bool isHost = OnlineMultiplayerManager.Instance.IsThisHostPlayer;
            bool hostInTemporaryTeam = OnlineMultiplayerManager.Instance.PlayerIsInTeamTemporary;
             

            bool triggerIsTeam1 = (PlayerJoiningNumber % 2 != 0);

            foreach (Player p in players)
            {
                if (!p.HasStateAuthority) continue; // Skip non-authoritative copies

                bool iAmTeam1 = (p.PlayerJoiningNumber % 2 != 0);
                bool myTeamMatchesTrigger = (iAmTeam1 == triggerIsTeam1);

                bool isThisHost = (p == isHost);
                bool allowSound = (isThisHost && hostInTemporaryTeam) || (!isThisHost);   // Host is allowed only if in temp team  // All other players are allowed


                if (myTeamMatchesTrigger && allowSound)
                {
                    Debug.LogError("Sound played: My team matches trigger and allowed.");
                    GameSoundManager.Instance.PlayHalfStreakSound();
                }
                else
                {
                    Debug.LogError("No sound: Team mismatch or not allowed.");
                }
            }


            #region Old code
            /*   if (FusionManager.Instance.IsThisHostPlayer && FusionManager.Instance.PlayerIsInTeamTemporary)
              {
                  Debug.LogError("HOST AND IN TEMPRARORY");
              }


              bool triggerIsTeam1 = (PlayerJoiningNumber % 2 != 0);


              foreach (Player p in players)
              {
                  if (!p.HasStateAuthority) continue;                 // skip remote copies

                  bool iAmTeam1 = (p.PlayerJoiningNumber % 2 != 0);

                  if (iAmTeam1 == triggerIsTeam1)
                  {
                      Debug.LogWarning($" Half-streak sound (my team matches trigger team)");
                      GameSoundManager.Instance.PlayFullStreakSound();
                  }
                  else
                  {
                      Debug.LogWarning($" opponent: no sound");
                  }
              }*/
            #endregion Old code
        }

        public void OnConsecutiveCorrectAnswersChanged()
        { 
            Debug.Log("OnConsecutiveCorrectAnswersChanged "  + ConsecutiveCorrectAnswers);
            PlayerAvtarInSpectMode spectMode = FindObjectOfType<PlayerAvtarInSpectMode>();
            if (spectMode != null)
            {
                PlayerAvtarInSpectMode.Instance.UpdateAvatarBorder(this);
            }
        }

        private void onChangeScore()
        {
            if(Score != 0)
            {
                if (PlayerJoiningNumber % 2 != 0)
                {
                    OnlineMultiplayerManager.Instance.Team1ScoreNetworked += TeamPlayersManager.Instance.ScoreValueForTeam1;
                    Debug.Log("On change in player script" + TeamPlayersManager.Instance.ScoreValueForTeam1);
                    OnlineMultiplayerManager.Instance.OnChangeTeam1Score();
                }
                else
                {

                    OnlineMultiplayerManager.Instance.Team2ScoreNetworked += TeamPlayersManager.Instance.ScoreValueForTeam2;
                    Debug.Log("On change in player script" + TeamPlayersManager.Instance.ScoreValueForTeam2);
                    OnlineMultiplayerManager.Instance.OnChangeTeam2Score();
                }
            }

        }
        public void OnChangePlayerName()
        {
            HostPlayerNameTextReference.text = Name;
            ClientPlayerNameTextReference.text = Name;
            

            if (OnlineMultiplayerManager.Instance.IsThisHostPlayer)
            {
                OnlineMultiplayerUIManager.Instance.ActiveOrDeactiveKickButton();
            }

        }

        public void OnChangePlayerAvtar()
        {
            Debug.Log("set image in reference");
            if(HostPlayerAvtarReference != null)
            {
                Debug.Log("host reference not null");
            }
            if (HostPlayerAvtarReference != null)
                HostPlayerAvtarReference.sprite = GeneralModule.Instance.Avtars[PlayerAvtarIndex];
            if(ClientPlayerAvtarReference != null)
                ClientPlayerAvtarReference.sprite = GeneralModule.Instance.Avtars[PlayerAvtarIndex];
 
        }

    private IEnumerator WaitForPlayerCountSync()
    {
        bool switchteam = false;
        bool allPlayerJoinedTeam = false;

        while (!switchteam)
        {
            int networkPlayerCount = Runner.SessionInfo.PlayerCount;
            int localPlayerCount = FindObjectsOfType<Player>().Length;

            Debug.Log($"Checking player counts: Network={networkPlayerCount}, Local={localPlayerCount}");

            Player[] players = FindObjectsOfType<Player>();

            foreach (Player player in players)
            {
                if (!player.localPositionSet)
                {
                    allPlayerJoinedTeam = false;
                    break;
                }
                else
                {
                    allPlayerJoinedTeam = true;
                }
            }
            Debug.Log("All player joined team" + allPlayerJoinedTeam);
            if (networkPlayerCount == localPlayerCount && allPlayerJoinedTeam)
            {
                switchteam = true;

            }

            yield return new WaitForSeconds(1);

        }


        if (OnlineMultiplayerManager.Instance.IsThisHostPlayer && IsThisHostPlayer)
        {
            OnlineMultiplayerManager.Instance.PlayerIsInTeamTemporary = true;
        }

        // Determine the current team based on playerJoiningNumber
        bool isCurrentlyOnTeam1 = localPlayerJoiningNumber % 2 != 0;

        // Remove the player from the current team


        if (isCurrentlyOnTeam1)
        {
            TeamPlayersManager.Instance.Team1TotalPlayers--;
            int currentIndex = Array.IndexOf(playerPossibleOddNumber, localPlayerJoiningNumber);

            // Clear UI reference for the player in Team 1
            Debug.Log("Current team2 make null " + currentIndex + " Local player joining number " + localPlayerJoiningNumber);
            if (currentIndex >= 0 && ManualSwitchTeam)
            {
                OnlineMultiplayerUIManager.Instance.HostTeam1PlayerName[currentIndex].text = "";
                OnlineMultiplayerUIManager.Instance.ClientTeam1PlayerName[currentIndex].text = "";
                OnlineMultiplayerUIManager.Instance.HostTeam1OccupiedPlace[currentIndex].Value = false;
                OnlineMultiplayerUIManager.Instance.ClientTeam1OccupiedPlace[currentIndex].Value = false;
                OnlineMultiplayerUIManager.Instance.HostTeam1PlayerAvtar[currentIndex].sprite = null;
                OnlineMultiplayerUIManager.Instance.ClientTeam1PlayerAvtar[currentIndex].sprite = null;
            }
        }
        else
        {
            TeamPlayersManager.Instance.Team2TotalPlayers--;
            int currentIndex = Array.IndexOf(playerPossibleEvenNumber, localPlayerJoiningNumber);

            // Clear UI reference for the player in Team 2
            Debug.Log("Current team2 make null " + currentIndex + " Local player joining number " + localPlayerJoiningNumber);
            if (currentIndex >= 0 && ManualSwitchTeam)
            {
                OnlineMultiplayerUIManager.Instance.HostTeam2PlayerName[currentIndex].text = "";
                OnlineMultiplayerUIManager.Instance.ClientTeam2PlayerName[currentIndex].text = "";
                OnlineMultiplayerUIManager.Instance.HostTeam2OccupiedPlace[currentIndex].Value = false;
                OnlineMultiplayerUIManager.Instance.ClientTeam2OccupiedPlace[currentIndex].Value = false;
                OnlineMultiplayerUIManager.Instance.HostTeam2PlayerAvtar[currentIndex].sprite = null;
                OnlineMultiplayerUIManager.Instance.ClientTeam2PlayerAvtar[currentIndex].sprite = null;
            }
        }

        if (!ManualSwitchTeam)
        {
            ManualSwitchTeam = true;
        }

        // Switch the player's team
        if (isCurrentlyOnTeam1)
        {
            // Assign the player to Team 2

            if (HasStateAuthority)
            {
                OnlineMultiplayerUIManager.Instance.Team1HostJoinButton.gameObject.SetActive(true);
                OnlineMultiplayerUIManager.Instance.Team1HostLeaveButton.gameObject.SetActive(false);

                OnlineMultiplayerUIManager.Instance.Team2HostJoinButton.gameObject.SetActive(false);
                OnlineMultiplayerUIManager.Instance.Team2HostLeaveButton.gameObject.SetActive(true);

                OnlineMultiplayerUIManager.Instance.Team1ClientJoinButton.gameObject.SetActive(true);
                //FusionManager2.Instance.Team1ClientLeaveButton.gameObject.SetActive(false);

                OnlineMultiplayerUIManager.Instance.Team2ClientJoinButton.gameObject.SetActive(false);
                //FusionManager2.Instance.Team2ClientLeaveButton.gameObject.SetActive(true);
            }

            TeamPlayersManager.Instance.Team2TotalPlayers++;

            for (int i = 0; i < TeamPlayersManager.Instance.Team2TotalPlayers; i++)
            {
                // Check if the HostTeam2PlayerName at this index is null
                if (string.IsNullOrEmpty(OnlineMultiplayerUIManager.Instance.HostTeam2PlayerName[i].text) &&
                    !OnlineMultiplayerUIManager.Instance.HostTeam2OccupiedPlace[i].Value)
                {
                    PlayerJoiningNumber = playerPossibleEvenNumber[i];
                    OnlineMultiplayerUIManager.Instance.HostTeam2OccupiedPlace[i].Value = true;
                    OnlineMultiplayerUIManager.Instance.ClientTeam2OccupiedPlace[i].Value = true;
                    localPlayerJoiningNumber = PlayerJoiningNumber;
                    Debug.Log("Assigned Name to index: " + i);
                    break; // Exit the loop after assigning
                }
            }

            Debug.Log("Current team1 : " + PlayerJoiningNumber);

            int newIndex = Array.IndexOf(playerPossibleEvenNumber, localPlayerJoiningNumber);

            Debug.Log("New index for text reference : " + PlayerJoiningNumber);

            PlayerPositionFix = newIndex;

            // Update UI references for Team 2
            if (newIndex >= 0)
            {
                OnlineMultiplayerUIManager.Instance.HostTeam2PlayerName[newIndex].text = Name;

                if (HasStateAuthority)
                {
                    OnlineMultiplayerUIManager.Instance.HostTeam2PlayerAvtar[newIndex].sprite = GeneralModule.Instance.Avtars[OnlineMultiplayerUIManager.Instance.PlayerAvtarNumber];
                    PlayerAvtarIndex = OnlineMultiplayerUIManager.Instance.PlayerAvtarNumber;
                }
                else
                {
                    OnlineMultiplayerUIManager.Instance.HostTeam2PlayerAvtar[newIndex].sprite = GeneralModule.Instance.Avtars[PlayerAvtarIndex];
                }

                HostPlayerNameTextReference = OnlineMultiplayerUIManager.Instance.HostTeam2PlayerName[newIndex];
                HostPlayerOccupiedPlaceReference = OnlineMultiplayerUIManager.Instance.HostTeam2OccupiedPlace[newIndex];
                HostPlayerAvtarReference = OnlineMultiplayerUIManager.Instance.HostTeam2PlayerAvtar[newIndex];

                OnlineMultiplayerUIManager.Instance.ClientTeam2PlayerName[newIndex].text = Name;

                if (HasStateAuthority)
                {
                    OnlineMultiplayerUIManager.Instance.ClientTeam2PlayerAvtar[newIndex].sprite = GeneralModule.Instance.Avtars[OnlineMultiplayerUIManager.Instance.PlayerAvtarNumber];
                    PlayerAvtarIndex = OnlineMultiplayerUIManager.Instance.PlayerAvtarNumber;
                }
                else
                {
                    OnlineMultiplayerUIManager.Instance.ClientTeam2PlayerAvtar[newIndex].sprite = GeneralModule.Instance.Avtars[PlayerAvtarIndex];
                }

                ClientPlayerNameTextReference = OnlineMultiplayerUIManager.Instance.ClientTeam2PlayerName[newIndex];
                ClientPlayerOccupiedPlaceReference = OnlineMultiplayerUIManager.Instance.ClientTeam2OccupiedPlace[newIndex];
                ClientPlayerAvtarReference = OnlineMultiplayerUIManager.Instance.ClientTeam2PlayerAvtar[newIndex];
            }

            TeamPlayersManager.Instance.SetScoreValueForEachPlayer();
            if (OnlineMultiplayerManager.Instance.IsThisHostPlayer && HasStateAuthority)
            {
                OnlineMultiplayerManager.Instance.index = newIndex;

            }
        }
        else
        {
            // Assign the player to Team 1

            if (HasStateAuthority)
            {
                OnlineMultiplayerUIManager.Instance.Team1HostJoinButton.gameObject.SetActive(false);
                OnlineMultiplayerUIManager.Instance.Team1HostLeaveButton.gameObject.SetActive(true);

                OnlineMultiplayerUIManager.Instance.Team2HostJoinButton.gameObject.SetActive(true);
                OnlineMultiplayerUIManager.Instance.Team2HostLeaveButton.gameObject.SetActive(false);

                OnlineMultiplayerUIManager.Instance.Team1ClientJoinButton.gameObject.SetActive(false);
                //FusionManager2.Instance.Team1ClientLeaveButton.gameObject.SetActive(true);

                OnlineMultiplayerUIManager.Instance.Team2ClientJoinButton.gameObject.SetActive(true);
                //FusionManager2.Instance.Team2ClientLeaveButton.gameObject.SetActive(false);
            }

            TeamPlayersManager.Instance.Team1TotalPlayers++;

            for (int i = 0; i < TeamPlayersManager.Instance.Team1TotalPlayers; i++)
            {
                // Check if the HostTeam2PlayerName at this index is null
                if (string.IsNullOrEmpty(OnlineMultiplayerUIManager.Instance.HostTeam1PlayerName[i].text) &&
                    !OnlineMultiplayerUIManager.Instance.HostTeam1OccupiedPlace[i].Value)
                {
                    PlayerJoiningNumber = playerPossibleOddNumber[i];
                    OnlineMultiplayerUIManager.Instance.HostTeam1OccupiedPlace[i].Value = true;
                    OnlineMultiplayerUIManager.Instance.ClientTeam1OccupiedPlace[i].Value = true;
                    localPlayerJoiningNumber = PlayerJoiningNumber;
                    Debug.Log("Assigned Name to index: " + i);
                    break; // Exit the loop after assigning
                }
            }

            Debug.Log("Current team2 : " + PlayerJoiningNumber);

            int newIndex = Array.IndexOf(playerPossibleOddNumber, localPlayerJoiningNumber);

            Debug.Log("New index for text reference : " + PlayerJoiningNumber);

            PlayerPositionFix = newIndex;

            // Update UI references for Team 1
            if (newIndex >= 0)
            {
                OnlineMultiplayerUIManager.Instance.HostTeam1PlayerName[newIndex].text = Name;

                if (HasStateAuthority)
                {
                    OnlineMultiplayerUIManager.Instance.HostTeam1PlayerAvtar[newIndex].sprite = GeneralModule.Instance.Avtars[OnlineMultiplayerUIManager.Instance.PlayerAvtarNumber];
                    PlayerAvtarIndex = OnlineMultiplayerUIManager.Instance.PlayerAvtarNumber;
                }
                else
                {
                    OnlineMultiplayerUIManager.Instance.HostTeam1PlayerAvtar[newIndex].sprite = GeneralModule.Instance.Avtars[PlayerAvtarIndex];
                }

                HostPlayerNameTextReference = OnlineMultiplayerUIManager.Instance.HostTeam1PlayerName[newIndex];
                HostPlayerOccupiedPlaceReference = OnlineMultiplayerUIManager.Instance.HostTeam1OccupiedPlace[newIndex];
                HostPlayerAvtarReference = OnlineMultiplayerUIManager.Instance.HostTeam1PlayerAvtar[newIndex];

                OnlineMultiplayerUIManager.Instance.ClientTeam1PlayerName[newIndex].text = Name;

                if (HasStateAuthority)
                {
                    OnlineMultiplayerUIManager.Instance.ClientTeam1PlayerAvtar[newIndex].sprite = GeneralModule.Instance.Avtars[OnlineMultiplayerUIManager.Instance.PlayerAvtarNumber];
                    PlayerAvtarIndex = OnlineMultiplayerUIManager.Instance.PlayerAvtarNumber;
                }
                else
                {
                    OnlineMultiplayerUIManager.Instance.ClientTeam1PlayerAvtar[newIndex].sprite = GeneralModule.Instance.Avtars[PlayerAvtarIndex];
                }

                ClientPlayerNameTextReference = OnlineMultiplayerUIManager.Instance.ClientTeam1PlayerName[newIndex];
                ClientPlayerOccupiedPlaceReference = OnlineMultiplayerUIManager.Instance.ClientTeam1OccupiedPlace[newIndex];
                ClientPlayerAvtarReference = OnlineMultiplayerUIManager.Instance.ClientTeam1PlayerAvtar[newIndex];
            }

            TeamPlayersManager.Instance.SetScoreValueForEachPlayer();
            if (OnlineMultiplayerManager.Instance.IsThisHostPlayer && HasStateAuthority)
            {
                OnlineMultiplayerManager.Instance.index = newIndex;

            }
        }

        Debug.Log($"Player {Name} switched to {(localPlayerJoiningNumber % 2 == 0 ? "Team 2" : "Team 1")}");

        if (OnlineMultiplayerManager.Instance.IsThisHostPlayer)
        {
            if (HasStateAuthority)
                OnlineMultiplayerManager.Instance.team = localPlayerJoiningNumber % 2 == 0 ? "Team2" : "Team1";

            OnlineMultiplayerUIManager.Instance.ActiveOrDeactiveKickButton();
        }

        OnlineMultiplayerUIManager.Instance.StartGameButton.interactable = OnlineMultiplayerManager.Instance.EnoughPlayersInTeamToStartGame() ? true : false;


    }

    public void OnChangeSwitchTeam()
        {
            StartCoroutine(WaitForPlayerCountSync());
            
        }
        void OnChageResetValues()
        {        
            ResetAllValues();
               
        }

        #endregion

        #region Fusion2 Callbacks

        public override void Spawned()
        {
            Debug.Log("Player Spawned " + initialJoiningNumber());

            if (HasStateAuthority && OnlineMultiplayerManager.Instance.IsThisHostPlayer)
            {
                Debug.Log("Set is this host player in spawned function");
                IsThisHostPlayer = true;
            }

            if (PlayerIsInTeamTemporary)
            {
                if(IsThisHostPlayer)
                {

                    Debug.Log("Yes this is host player spawned");
                }
                JoinPlayerInAnyTeam(true);

            }
        
        }
        #endregion

        #region Other Methods

        public void JoinPlayerInAnyTeam(bool joinFirstTime,int teamNumber = -1)
        {
            Debug.Log("Team number " + teamNumber);
            if (OnlineMultiplayerManager.Instance.IsThisHostPlayer && IsThisHostPlayer)
            {
                OnlineMultiplayerManager.Instance.PlayerIsInTeamTemporary = true;
                PlayerIsInTeamTemporary = true;
            
            }

            TeamPlayersManager.Instance.TotalPlayersJoinedAnyOneTeam++;
            PlayerRef currentPlayerRef = Object.StateAuthority;

            if (!OnlineMultiplayerManager.Instance.playerDictionary.ContainsKey(currentPlayerRef))
            {
                OnlineMultiplayerManager.Instance.playerDictionary.Add(currentPlayerRef, this); // `this` refers to the current Player script

                Debug.Log("Play count in player dictionary : " + OnlineMultiplayerManager.Instance.playerDictionary.Count);
                Debug.Log($"Player {Name} added to the dictionary with PlayerRef {currentPlayerRef}.");
            }

            if(teamNumber == -1)
            {
                if (PlayerJoiningNumber == 0)
                {
                    Debug.Log("Player count of session : " + OnlineMultiplayerManager.networkRunner.SessionInfo.PlayerCount + "Players in team" + TeamPlayersManager.Instance.TotalPlayersJoinedAnyOneTeam);
                    if (joinFirstTime)
                    {
                        PlayerJoiningNumber = OnlineMultiplayerManager.networkRunner.SessionInfo.PlayerCount;
                        if (PlayerJoiningNumber % 2 == 0)
                        {
                            int index = Array.IndexOf(GeneralModule.Instance.playerPossibleEvenNumber, PlayerJoiningNumber);
                            OnlineMultiplayerUIManager.Instance.HostTeam2OccupiedPlace[index].Value = true;
                            OnlineMultiplayerUIManager.Instance.ClientTeam2OccupiedPlace[index].Value = true;

                        }
                        else
                        {
                            int index = Array.IndexOf(GeneralModule.Instance.playerPossibleOddNumber, PlayerJoiningNumber);
                            OnlineMultiplayerUIManager.Instance.HostTeam1OccupiedPlace[index].Value = true;
                            OnlineMultiplayerUIManager.Instance.ClientTeam1OccupiedPlace[index].Value = true;
                        }
                    }
                    else
                    {
                        PlayerJoiningNumber = TeamPlayersManager.Instance.TotalPlayersJoinedAnyOneTeam;
                        if (PlayerJoiningNumber % 2 == 0)
                        {
                            int index = Array.IndexOf(GeneralModule.Instance.playerPossibleEvenNumber, PlayerJoiningNumber);
                            OnlineMultiplayerUIManager.Instance.HostTeam2OccupiedPlace[index].Value = true;
                            OnlineMultiplayerUIManager.Instance.ClientTeam2OccupiedPlace[index].Value = true;

                        }
                        else
                        {
                            int index = Array.IndexOf(GeneralModule.Instance.playerPossibleOddNumber, PlayerJoiningNumber);
                            OnlineMultiplayerUIManager.Instance.HostTeam1OccupiedPlace[index].Value = true;
                            OnlineMultiplayerUIManager.Instance.ClientTeam1OccupiedPlace[index].Value = true;
                        }
                    }

                }
            }
            else
            {
                if(teamNumber == 1)
                {
                    for (int i = 0; i < TeamPlayersManager.Instance.Team1TotalPlayers + 1; i++)
                    {
                        // Check if the HostTeam2PlayerName at this index is null
                        if (string.IsNullOrEmpty(OnlineMultiplayerUIManager.Instance.HostTeam1PlayerName[i].text) &&
                             !OnlineMultiplayerUIManager.Instance.HostTeam1OccupiedPlace[i].Value)
                        {
                            PlayerJoiningNumber = playerPossibleOddNumber[i];
                            OnlineMultiplayerUIManager.Instance.HostTeam1OccupiedPlace[i].Value = true;
                            OnlineMultiplayerUIManager.Instance.ClientTeam1OccupiedPlace[i].Value = true;
                            Debug.Log("Assigned Name to index team1: " + i);
                            break; // Exit the loop after assigning
                        }
                    }
                }
                else if(teamNumber == 2)
                {
                    for (int i = 0; i < TeamPlayersManager.Instance.Team2TotalPlayers + 1; i++)
                    {
                        // Check if the HostTeam2PlayerName at this index is null
                        if (string.IsNullOrEmpty(OnlineMultiplayerUIManager.Instance.HostTeam2PlayerName[i].text) &&
                             !OnlineMultiplayerUIManager.Instance.HostTeam2OccupiedPlace[i].Value)
                        {
                            PlayerJoiningNumber = playerPossibleEvenNumber[i];
                            OnlineMultiplayerUIManager.Instance.HostTeam2OccupiedPlace[i].Value = true;
                            OnlineMultiplayerUIManager.Instance.ClientTeam2OccupiedPlace[i].Value = true;
                            Debug.Log("Assigned Name to index team2: " + i);
                            break; // Exit the loop after assigning
                        }
                    }
                }
            }
               

            localPlayerJoiningNumber = PlayerJoiningNumber;

            Debug.Log("Player join number" + PlayerJoiningNumber);

            if(joinFirstTime)
            {
                if (HasStateAuthority)
                {
                    StartCoroutine(HandleWithDelay(joinFirstTime, teamNumber));

                }
                else
                {
                    setPositionOfPlayer(joinFirstTime,teamNumber);
                }
            }
            else
            {
                if (HasStateAuthority)
                {
                    setPositionOfPlayer(joinFirstTime, teamNumber);

                }
                else
                {
                    StartCoroutine(HandleWithDelay(joinFirstTime, teamNumber));
                }
            }
            OnlineMultiplayerUIManager.Instance.StartGameButton.interactable = OnlineMultiplayerManager.Instance.EnoughPlayersInTeamToStartGame() ? true : false;

        }

        public void ResetAllValues()
        {
            if (localPlayerJoiningNumber % 2 == 0)
            {
                TeamPlayersManager.Instance.Team2TotalPlayers--;
                int index = Array.IndexOf(GeneralModule.Instance.playerPossibleEvenNumber, localPlayerJoiningNumber);
                OnlineMultiplayerUIManager.Instance.HostTeam2OccupiedPlace[index].Value = false;
                OnlineMultiplayerUIManager.Instance.ClientTeam2OccupiedPlace[index].Value = false;
            }
            else
            {
                TeamPlayersManager.Instance.Team1TotalPlayers--;
                int index = Array.IndexOf(GeneralModule.Instance.playerPossibleOddNumber, localPlayerJoiningNumber);
                Debug.Log("Index in reset all value for checking in team 1 player " + index);
                OnlineMultiplayerUIManager.Instance.HostTeam1OccupiedPlace[index].Value = false;
                OnlineMultiplayerUIManager.Instance.ClientTeam1OccupiedPlace[index].Value = false;
            }             

            if (HasStateAuthority)
            {
                OnlineMultiplayerUIManager.Instance.Team1HostJoinButton.gameObject.SetActive(true);
                OnlineMultiplayerUIManager.Instance.Team1HostLeaveButton.gameObject.SetActive(false);

                OnlineMultiplayerUIManager.Instance.Team2HostJoinButton.gameObject.SetActive(true);
                OnlineMultiplayerUIManager.Instance.Team2HostLeaveButton.gameObject.SetActive(false);

                OnlineMultiplayerUIManager.Instance.Team1ClientJoinButton.gameObject.SetActive(true);
                //FusionManager2.Instance.Team1ClientLeaveButton.gameObject.SetActive(false);

                OnlineMultiplayerUIManager.Instance.Team2ClientJoinButton.gameObject.SetActive(true);
                //FusionManager2.Instance.Team2ClientLeaveButton.gameObject.SetActive(false);
            }

            PlayerJoiningNumber = 0;
            localPlayerJoiningNumber = 0;
            PlayerPositionFix = -1;
            playerIsInTeam = false;
            PlayerIsInTeamTemporary = false;
            TeamPlayersManager.Instance.TotalPlayersJoinedAnyOneTeam--;
            TeamPlayersManager.Instance.SetScoreValueForEachPlayer();
       
        
            HostPlayerNameTextReference.text = "";
            HostPlayerOccupiedPlaceReference.Value = false;
            ClientPlayerNameTextReference.text = "";
            ClientPlayerOccupiedPlaceReference.Value = false;

            HostPlayerAvtarReference.sprite = null;
            ClientPlayerAvtarReference.sprite = null;

            HostPlayerAvtarReference = null;
            ClientPlayerAvtarReference = null;

            if (OnlineMultiplayerManager.Instance.IsThisHostPlayer)
            {
                if(HasStateAuthority)
                {
                    OnlineMultiplayerManager.Instance.index = -1;
                    OnlineMultiplayerManager.Instance.team = "";

                }
                OnlineMultiplayerUIManager.Instance.ActiveOrDeactiveKickButton();
            }
            OnlineMultiplayerManager.Instance.PlayerIsInTeamTemporary = false;

            OnlineMultiplayerUIManager.Instance.StartGameButton.interactable = OnlineMultiplayerManager.Instance.EnoughPlayersInTeamToStartGame() ? true : false;


        }

        IEnumerator HandleWithDelay(bool joinFirstTime,int teamNumber = -1)
        {
            // Wait for 5 seconds
            yield return new WaitForSeconds(2f);

            setPositionOfPlayer(joinFirstTime, teamNumber);
        }

        IEnumerator HandleButtonInteraction(params Button[] buttons)
        {
            Debug.Log("Handle button interaction");
            foreach(Button button in buttons)
            {
                button.interactable = false;
            }

            yield return new WaitForSeconds(2f);

            foreach (Button button in buttons)
            {
                button.interactable = true;
            }
        
        }

        void setPositionOfPlayer(bool joinFirstTime, int teamNumber = -1)
        {
            playerIsInTeam = true;

            if (PlayerJoiningNumber % 2 == 0)
            {
                TeamPlayersManager.Instance.Team2TotalPlayers++;
                TeamPlayersManager.Instance.SetScoreValueForEachPlayer();

                if(HasStateAuthority)
                {
                    OnlineMultiplayerUIManager.Instance.Team1HostJoinButton.gameObject.SetActive(true);
                    OnlineMultiplayerUIManager.Instance.Team1HostLeaveButton.gameObject.SetActive(false);

                    OnlineMultiplayerUIManager.Instance.Team2HostJoinButton.gameObject.SetActive(false);
                    OnlineMultiplayerUIManager.Instance.Team2HostLeaveButton.gameObject.SetActive(true);

                    OnlineMultiplayerUIManager.Instance.Team1ClientJoinButton.gameObject.SetActive(true);
                    //FusionManager2.Instance.Team1ClientLeaveButton.gameObject.SetActive(false);

                    OnlineMultiplayerUIManager.Instance.Team2ClientJoinButton.gameObject.SetActive(false);
                    //FusionManager2.Instance.Team2ClientLeaveButton.gameObject.SetActive(true);

                    StartCoroutine(HandleButtonInteraction(OnlineMultiplayerUIManager.Instance.Team1HostJoinButton, OnlineMultiplayerUIManager.Instance.Team1HostLeaveButton,
                    OnlineMultiplayerUIManager.Instance.Team2HostJoinButton, OnlineMultiplayerUIManager.Instance.Team2HostLeaveButton,
                    OnlineMultiplayerUIManager.Instance.Team1ClientJoinButton, OnlineMultiplayerUIManager.Instance.Team2ClientJoinButton));
                }

                int playerJoiningIndex = Array.IndexOf(playerPossibleEvenNumber, PlayerJoiningNumber);

                if (PlayerPositionFix == -1)
                {
                    bool switchTeam = false;
                    // Iterate from the 0th index to the current playerJoiningIndex
                    for (int i = 0; i <= playerJoiningIndex; i++)
                    {
                        // Check if the HostTeam2PlayerName at this index is null
                        if (string.IsNullOrEmpty(OnlineMultiplayerUIManager.Instance.HostTeam2PlayerName[i].text) /*&&
                            (teamNumber != -1 || !OnlineMultiplayerUIManager.Instance.HostTeam2OccupiedPlace[i].Value )*/)
                        {


                            // Set the text for this index
                            OnlineMultiplayerUIManager.Instance.HostTeam2PlayerName[i].text = Name;
                            localPositionSet = true;
                            OnlineMultiplayerUIManager.Instance.HostTeam2OccupiedPlace[i].Value = true;
                        

                            if (HasStateAuthority)
                            {
                                OnlineMultiplayerUIManager.Instance.HostTeam2PlayerAvtar[i].sprite = GeneralModule.Instance.Avtars[PlayerAvtarIndex];
                                PlayerAvtarIndex = OnlineMultiplayerUIManager.Instance.PlayerAvtarNumber;
                            }
                            else
                            {
                                OnlineMultiplayerUIManager.Instance.HostTeam2PlayerAvtar[i].sprite = GeneralModule.Instance.Avtars[PlayerAvtarIndex];
                            }
                                                
                            HostPlayerNameTextReference = OnlineMultiplayerUIManager.Instance.HostTeam2PlayerName[i];
                            HostPlayerOccupiedPlaceReference = OnlineMultiplayerUIManager.Instance.HostTeam2OccupiedPlace[i];
                            HostPlayerOccupiedPlaceReference = OnlineMultiplayerUIManager.Instance.HostTeam2OccupiedPlace[i];
                            HostPlayerAvtarReference = OnlineMultiplayerUIManager.Instance.HostTeam2PlayerAvtar[i];
                            localPlayerJoiningNumber = playerPossibleEvenNumber[i];
                            switchTeam = true;
                            PlayerPositionFix = i;
                            Debug.Log("Assigned Name to index: " + i);
                            break; // Exit the loop after assigning
                        }

                    }

                }
                else
                {
                
                    if(string.IsNullOrEmpty(OnlineMultiplayerUIManager.Instance.HostTeam2PlayerName[PlayerPositionFix].text) /*&&
                        (teamNumber != -1 || !OnlineMultiplayerUIManager.Instance.HostTeam2OccupiedPlace[PlayerPositionFix].Value)*/)
                    {
                        OnlineMultiplayerUIManager.Instance.HostTeam2PlayerName[PlayerPositionFix].text = Name;
                        localPositionSet = true;
                        OnlineMultiplayerUIManager.Instance.HostTeam2OccupiedPlace[PlayerPositionFix].Value = true;
                   
                        if (HasStateAuthority)
                        {
                            OnlineMultiplayerUIManager.Instance.HostTeam2PlayerAvtar[PlayerPositionFix].sprite = GeneralModule.Instance.Avtars[OnlineMultiplayerUIManager.Instance.PlayerAvtarNumber];
                            PlayerAvtarIndex = OnlineMultiplayerUIManager.Instance.PlayerAvtarNumber;
                        }
                        else
                        {
                            OnlineMultiplayerUIManager.Instance.HostTeam2PlayerAvtar[PlayerPositionFix].sprite = GeneralModule.Instance.Avtars[PlayerAvtarIndex];
                        }
                        HostPlayerNameTextReference = OnlineMultiplayerUIManager.Instance.HostTeam2PlayerName[PlayerPositionFix];
                        HostPlayerOccupiedPlaceReference = OnlineMultiplayerUIManager.Instance.HostTeam2OccupiedPlace[PlayerPositionFix];
                        HostPlayerAvtarReference = OnlineMultiplayerUIManager.Instance.HostTeam2PlayerAvtar[PlayerPositionFix];
                        localPlayerJoiningNumber = playerPossibleEvenNumber[PlayerPositionFix];

                    }
                    else
                    {
                        ManualSwitchTeam = false;
                    
                    }
                }

                if(OnlineMultiplayerManager.Instance.IsThisHostPlayer && HasStateAuthority)
                {
                    OnlineMultiplayerManager.Instance.index = playerJoiningIndex;
                    OnlineMultiplayerManager.Instance.team = "Team2";
                }

            }
            else
            {
                Debug.Log("Odd player join");

                TeamPlayersManager.Instance.Team1TotalPlayers++;
                TeamPlayersManager.Instance.SetScoreValueForEachPlayer();

                if(HasStateAuthority)
                {
                    Debug.Log("Odd player HandleButtonInteraction");

                    OnlineMultiplayerUIManager.Instance.Team1HostJoinButton.gameObject.SetActive(false);
                    OnlineMultiplayerUIManager.Instance.Team1HostLeaveButton.gameObject.SetActive(true);

                    OnlineMultiplayerUIManager.Instance.Team2HostJoinButton.gameObject.SetActive(true);
                    OnlineMultiplayerUIManager.Instance.Team2HostLeaveButton.gameObject.SetActive(false);

                    OnlineMultiplayerUIManager.Instance.Team1ClientJoinButton.gameObject.SetActive(false);
                    //FusionManager2.Instance.Team1ClientLeaveButton.gameObject.SetActive(true);

                    OnlineMultiplayerUIManager.Instance.Team2ClientJoinButton.gameObject.SetActive(true);
                    //FusionManager2.Instance.Team2ClientLeaveButton.gameObject.SetActive(false);

                    StartCoroutine(HandleButtonInteraction(OnlineMultiplayerUIManager.Instance.Team1HostJoinButton, OnlineMultiplayerUIManager.Instance.Team1HostLeaveButton,
                        OnlineMultiplayerUIManager.Instance.Team2HostJoinButton, OnlineMultiplayerUIManager.Instance.Team2HostLeaveButton,
                        OnlineMultiplayerUIManager.Instance.Team1ClientJoinButton, OnlineMultiplayerUIManager.Instance.Team2ClientJoinButton));
                }

                int playerJoiningIndex = Array.IndexOf(playerPossibleOddNumber, PlayerJoiningNumber);

                if (PlayerPositionFix == -1)
                {
                    bool switchTeam = false;
                    // Iterate from the 0th index to the current playerJoiningIndex
                    for (int i = 0; i <= playerJoiningIndex; i++)
                    {
                        // Check if the HostTeam2PlayerName at this index is null
                        if (string.IsNullOrEmpty(OnlineMultiplayerUIManager.Instance.HostTeam1PlayerName[i].text)/* &&
                             (teamNumber != -1 || !OnlineMultiplayerUIManager.Instance.HostTeam1OccupiedPlace[i].Value)*/)
                        {
                            // Set the text for this index
                            OnlineMultiplayerUIManager.Instance.HostTeam1PlayerName[i].text = Name;
                            localPositionSet = true;
                            OnlineMultiplayerUIManager.Instance.HostTeam1OccupiedPlace[i].Value = true;
                     
                            if (HasStateAuthority)
                            {
                                OnlineMultiplayerUIManager.Instance.HostTeam1PlayerAvtar[i].sprite = GeneralModule.Instance.Avtars[OnlineMultiplayerUIManager.Instance.PlayerAvtarNumber];
                                PlayerAvtarIndex = OnlineMultiplayerUIManager.Instance.PlayerAvtarNumber;
                            }
                            else
                            {
                                OnlineMultiplayerUIManager.Instance.HostTeam1PlayerAvtar[i].sprite = GeneralModule.Instance.Avtars[PlayerAvtarIndex];
                            }

                            HostPlayerNameTextReference = OnlineMultiplayerUIManager.Instance.HostTeam1PlayerName[i];
                            HostPlayerOccupiedPlaceReference = OnlineMultiplayerUIManager.Instance.HostTeam1OccupiedPlace[i];
                            HostPlayerAvtarReference = OnlineMultiplayerUIManager.Instance.HostTeam1PlayerAvtar[i];
                            localPlayerJoiningNumber = playerPossibleOddNumber[i];
                            PlayerPositionFix = i;
                            switchTeam = true;
                            Debug.Log("Assigned Name to index: " + i);

                            break; // Exit the loop after assigning
                        }
                    
                    }

                    if (!switchTeam && joinFirstTime)
                    {
                        ManualSwitchTeam = false;
                    }

                }
                else
                {
                    if (string.IsNullOrEmpty(OnlineMultiplayerUIManager.Instance.HostTeam1PlayerName[PlayerPositionFix].text)/* &&
                        (teamNumber != -1 || !OnlineMultiplayerUIManager.Instance.HostTeam1OccupiedPlace[PlayerPositionFix].Value)*/)
                    {
                        OnlineMultiplayerUIManager.Instance.HostTeam1PlayerName[PlayerPositionFix].text = Name;
                        localPositionSet = true;
                        OnlineMultiplayerUIManager.Instance.HostTeam1OccupiedPlace[PlayerPositionFix].Value = true;
                        if (HasStateAuthority)
                        {
                            OnlineMultiplayerUIManager.Instance.HostTeam1PlayerAvtar[PlayerPositionFix].sprite = GeneralModule.Instance.Avtars[OnlineMultiplayerUIManager.Instance.PlayerAvtarNumber];
                            PlayerAvtarIndex = OnlineMultiplayerUIManager.Instance.PlayerAvtarNumber;
                        }
                        else
                        {
                            OnlineMultiplayerUIManager.Instance.HostTeam1PlayerAvtar[PlayerPositionFix].sprite = GeneralModule.Instance.Avtars[PlayerAvtarIndex];
                        }

                        HostPlayerNameTextReference = OnlineMultiplayerUIManager.Instance.HostTeam1PlayerName[PlayerPositionFix];
                        HostPlayerOccupiedPlaceReference = OnlineMultiplayerUIManager.Instance.HostTeam1OccupiedPlace[PlayerPositionFix];
                        HostPlayerAvtarReference = OnlineMultiplayerUIManager.Instance.HostTeam1PlayerAvtar[PlayerPositionFix];
                        localPlayerJoiningNumber = playerPossibleOddNumber[PlayerPositionFix];
                    }
                    else
                    {
                        ManualSwitchTeam = false;
                    }
                    
                }

                Debug.Log("Player index " + playerJoiningIndex);
                if(OnlineMultiplayerManager.Instance.IsThisHostPlayer && HasStateAuthority)
                {
                    OnlineMultiplayerManager.Instance.index = playerJoiningIndex;
                    OnlineMultiplayerManager.Instance.team = "Team1";
                }


            }
            if (PlayerJoiningNumber % 2 == 0)
            {
                int playerJoiningIndex = Array.IndexOf(playerPossibleEvenNumber, PlayerJoiningNumber);
                Debug.Log("Player index " + playerJoiningIndex);

                if (PlayerPositionFix == -1)
                {
                    bool switchTeam = false;

                    // Iterate from the 0th index to the current playerJoiningIndex
                    for (int i = 0; i <= playerJoiningIndex; i++)
                    {
                        // Check if the HostTeam2PlayerName at this index is null
                        if (string.IsNullOrEmpty(OnlineMultiplayerUIManager.Instance.ClientTeam2PlayerName[i].text) /*&&
                            (teamNumber != -1 || !OnlineMultiplayerUIManager.Instance.ClientTeam2OccupiedPlace[i].Value)*/)
                        {
                            // Set the text for this index
                            OnlineMultiplayerUIManager.Instance.ClientTeam2PlayerName[i].text = Name;
                            localPositionSet = true;
                            OnlineMultiplayerUIManager.Instance.ClientTeam2OccupiedPlace[i].Value = true;

                            if (HasStateAuthority)
                            {
                                OnlineMultiplayerUIManager.Instance.ClientTeam2PlayerAvtar[i].sprite = GeneralModule.Instance.Avtars[OnlineMultiplayerUIManager.Instance.PlayerAvtarNumber];
                                PlayerAvtarIndex = OnlineMultiplayerUIManager.Instance.PlayerAvtarNumber;
                            }
                            else
                            {
                                OnlineMultiplayerUIManager.Instance.ClientTeam2PlayerAvtar[i].sprite = GeneralModule.Instance.Avtars[PlayerAvtarIndex];
                            }

                            ClientPlayerNameTextReference = OnlineMultiplayerUIManager.Instance.ClientTeam2PlayerName[i];
                            ClientPlayerOccupiedPlaceReference = OnlineMultiplayerUIManager.Instance.ClientTeam2OccupiedPlace[i];
                            ClientPlayerAvtarReference = OnlineMultiplayerUIManager.Instance.ClientTeam2PlayerAvtar[i];
                            localPlayerJoiningNumber = playerPossibleEvenNumber[i];
                            PlayerPositionFix = i;
                            switchTeam = true;
                            Debug.Log("Assigned Name to index: " + i);
                            break; // Exit the loop after assigning
                        }
                    }

                    if (!switchTeam)
                    {
                        ManualSwitchTeam = false;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(OnlineMultiplayerUIManager.Instance.ClientTeam2PlayerName[PlayerPositionFix].text) /* &&
                        (teamNumber != -1 || !OnlineMultiplayerUIManager.Instance.ClientTeam2OccupiedPlace[PlayerPositionFix].Value)*/)
                    {
                        OnlineMultiplayerUIManager.Instance.ClientTeam2PlayerName[PlayerPositionFix].text = Name;
                        localPositionSet = true;
                        OnlineMultiplayerUIManager.Instance.ClientTeam2OccupiedPlace[PlayerPositionFix].Value = true;

                        if (HasStateAuthority)
                        {
                            OnlineMultiplayerUIManager.Instance.ClientTeam2PlayerAvtar[PlayerPositionFix].sprite = GeneralModule.Instance.Avtars[OnlineMultiplayerUIManager.Instance.PlayerAvtarNumber];
                            PlayerAvtarIndex = OnlineMultiplayerUIManager.Instance.PlayerAvtarNumber;
                        }
                        else
                        {
                            OnlineMultiplayerUIManager.Instance.ClientTeam2PlayerAvtar[PlayerPositionFix].sprite = GeneralModule.Instance.Avtars[PlayerAvtarIndex];
                        }

                        ClientPlayerNameTextReference = OnlineMultiplayerUIManager.Instance.ClientTeam2PlayerName[PlayerPositionFix];
                        ClientPlayerOccupiedPlaceReference = OnlineMultiplayerUIManager.Instance.ClientTeam2OccupiedPlace[PlayerPositionFix];
                        ClientPlayerAvtarReference = OnlineMultiplayerUIManager.Instance.ClientTeam2PlayerAvtar[PlayerPositionFix];
                        localPlayerJoiningNumber = playerPossibleEvenNumber[PlayerPositionFix];
                    }
                    else
                    {
                    
                        ManualSwitchTeam = false;
                    }
                }

                if(OnlineMultiplayerManager.Instance.IsThisHostPlayer && HasStateAuthority)
                {
                    OnlineMultiplayerManager.Instance.index = playerJoiningIndex;
                    OnlineMultiplayerManager.Instance.team = "Team2";

                }


            }
            else
            {
                int playerJoiningIndex = Array.IndexOf(playerPossibleOddNumber, PlayerJoiningNumber);
                Debug.Log("Player index " + playerJoiningIndex);

                if (PlayerPositionFix == -1)
                {
                    bool switchTeam = false;
                    for (int i = 0; i <= playerJoiningIndex; i++)
                    {
                        Debug.Log("check occupies position " + i + " " + OnlineMultiplayerUIManager.Instance.ClientTeam1OccupiedPlace[i].Value);
                        // Check if the HostTeam2PlayerName at this index is null
                        if (string.IsNullOrEmpty(OnlineMultiplayerUIManager.Instance.ClientTeam1PlayerName[i].text) /*&&
                            (teamNumber != -1 || !OnlineMultiplayerUIManager.Instance.ClientTeam1OccupiedPlace[i].Value)*/)
                        {
                            // Set the text for this index
                            OnlineMultiplayerUIManager.Instance.ClientTeam1PlayerName[i].text = Name;
                            localPositionSet = true;
                            OnlineMultiplayerUIManager.Instance.ClientTeam1OccupiedPlace[i].Value = true;

                            if (HasStateAuthority)
                            {
                                OnlineMultiplayerUIManager.Instance.ClientTeam1PlayerAvtar[i].sprite = GeneralModule.Instance.Avtars[OnlineMultiplayerUIManager.Instance.PlayerAvtarNumber];
                                PlayerAvtarIndex = OnlineMultiplayerUIManager.Instance.PlayerAvtarNumber;
                            }
                            else
                            {
                                OnlineMultiplayerUIManager.Instance.ClientTeam1PlayerAvtar[i].sprite = GeneralModule.Instance.Avtars[PlayerAvtarIndex];
                            }

                            ClientPlayerNameTextReference = OnlineMultiplayerUIManager.Instance.ClientTeam1PlayerName[i];
                            ClientPlayerOccupiedPlaceReference = OnlineMultiplayerUIManager.Instance.ClientTeam1OccupiedPlace[i];
                            ClientPlayerAvtarReference = OnlineMultiplayerUIManager.Instance.ClientTeam2PlayerAvtar[i];
                            localPlayerJoiningNumber = playerPossibleOddNumber[i];
                            PlayerPositionFix = i;
                            switchTeam = true;
                            Debug.Log("Assigned Name to index: " + i);
                            break; // Exit the loop after assigning
                        }
                    }

                    if(!switchTeam)
                    {
                        Debug.Log("Manual swithc team false from PlayerPositionFix -1");
                        ManualSwitchTeam = false;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(OnlineMultiplayerUIManager.Instance.ClientTeam1PlayerName[PlayerPositionFix].text) /*&&
                        (teamNumber != -1 || !OnlineMultiplayerUIManager.Instance.ClientTeam1OccupiedPlace[PlayerPositionFix].Value)*/)
                    {
                        OnlineMultiplayerUIManager.Instance.ClientTeam1PlayerName[PlayerPositionFix].text = Name;
                        localPositionSet = true;
                        OnlineMultiplayerUIManager.Instance.ClientTeam1OccupiedPlace[PlayerPositionFix].Value = true;

                        if (HasStateAuthority)
                        {
                            OnlineMultiplayerUIManager.Instance.ClientTeam1PlayerAvtar[PlayerPositionFix].sprite = GeneralModule.Instance.Avtars[OnlineMultiplayerUIManager.Instance.PlayerAvtarNumber];
                            PlayerAvtarIndex = OnlineMultiplayerUIManager.Instance.PlayerAvtarNumber;
                        }
                        else
                        {
                            OnlineMultiplayerUIManager.Instance.ClientTeam1PlayerAvtar[PlayerPositionFix].sprite = GeneralModule.Instance.Avtars[PlayerAvtarIndex];
                        }

                        ClientPlayerNameTextReference = OnlineMultiplayerUIManager.Instance.ClientTeam1PlayerName[PlayerPositionFix];
                        ClientPlayerOccupiedPlaceReference = OnlineMultiplayerUIManager.Instance.ClientTeam1OccupiedPlace[PlayerPositionFix];
                        ClientPlayerAvtarReference = OnlineMultiplayerUIManager.Instance.ClientTeam1PlayerAvtar[PlayerPositionFix];
                        localPlayerJoiningNumber = playerPossibleOddNumber[PlayerPositionFix];
                    }
                    else
                    {
                        Debug.Log("Manual swithc team false from not PlayerPositionFix -1");
                        ManualSwitchTeam = false;
                    }
                }

                if(OnlineMultiplayerManager.Instance.IsThisHostPlayer && HasStateAuthority)
                {
                    OnlineMultiplayerManager.Instance.index = playerJoiningIndex;
                    OnlineMultiplayerManager.Instance.team = "Team1";
                }


            }

            if(!ManualSwitchTeam)   
            {
                Debug.Log("Called Manua switch Team function automatically");
                OnChangeSwitchTeam();
            }

            if (HasStateAuthority)
            {
                Debug.Log("set name and avtar");
                Name = OnlineMultiplayerUIManager.Instance.PlayerNameInput[0].text;
                PlayerAvtar = OnlineMultiplayerManager.Instance.PlayerAvtar;
                ChangedPlayerAvtar = !ChangedPlayerAvtar;
                Debug.Log("Name : " + Name);

            }
            else
            {
                Debug.Log("Name : " + Name);
            
            }

            if(OnlineMultiplayerManager.Instance.IsThisHostPlayer)
            {
                OnlineMultiplayerUIManager.Instance.ActiveOrDeactiveKickButton();
            }
        }

        public void LeaveAllPlayers()
        {
            OnlineMultiplayerUIManager.Instance.LeaveSession();
        }
           
        public void SwitchTeam()
        {
            // StartCoroutine(WaitForPlayerCountSync());

            if (PlayerIsInTeamTemporary)
            {
                SwitchTeamPropety = !SwitchTeamPropety;
            }

        }
     

        public void GoToMenuByHost()
        {
            GeneralModule.Instance.scorePanel.SetActive(false);
            OnlineMultiplayerUIManager.Instance.GoToMenuByHost();
        }

        #endregion

        #region RPC Methods

       [Rpc(RpcSources.All, RpcTargets.All)]
        public void Rpc_JoinPlayerInAnyTeam(bool isRandomTeam, int teamNumber)
        {
            JoinPlayerInAnyTeam(isRandomTeam, teamNumber);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void Rpc_RemovedPlayerByHost()
        {
            OnlineMultiplayerUIManager.Instance.LeaveSession();
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void Rpc_LobbyClosedByHost()
        {
            //  IsThisHostPlayer = false;
            // FusionManager.Instance.IsThisHostPlayer = false;
            OnlineMultiplayerUIManager.Instance.AllCanvas.SetActive(true);
            OnlineMultiplayerUIManager.Instance.IntroAnimation.SetActive(false);
            OnlineMultiplayerUIManager.Instance.IntroVsAnimation.SetActive(false);
            Debug.Log("Lobby closed by host");
            OnlineMultiplayerUIManager.Instance.LeaveSession();
        }
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void Rpc_IntroAnimation()
        {
            if (OnlineMultiplayerManager.Instance.IsThisHostPlayer)
            {
                OnlineMultiplayerUIManager.Instance.GeneralGamePanels[(int)GeneralPanelType.SaccadeMenuHost].SetActive(false);
            }
            else
            {
                OnlineMultiplayerUIManager.Instance.GeneralGamePanels[(int)GeneralPanelType.SaccadeMenuClient].SetActive(false);
            }
            Debug.Log("Intro Animation Run In All Device");
            StartCoroutine(OnlineMultiplayerManager.Instance.HandleIntroAnimationAndPlayerLogic()); // new change
            //FusionManager.Instance.HandleIntroAnimationAndPlayerLogic();
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void Rpc_ShowResultAnimation()
        {       
            GameSoundManager.Instance.StopBackgroundMusic();
            OnlineMultiplayerManager.Instance.isScoreShowed = false;

            if (OnlineMultiplayerManager.Instance.IsThisHostPlayer && !PlayerIsInTeamTemporary)
            {
                GeneralModule.Instance.checkWinner(OnlineMultiplayerManager.Instance.Team1ScoreNetworked,OnlineMultiplayerManager.Instance.Team2ScoreNetworked);
                OnlineMultiplayerUIManager.Instance.SpectModeMenuButton.SetActive(true);
            }
        }

        //NEW DELAYED SCOREBOARD
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void Rpc_ShowSpectatorResultAfterDelay()
        {
            StartCoroutine(ShowSpectatorScoreboardRoutine());
        }

        private IEnumerator ShowSpectatorScoreboardRoutine()
        {
            yield return new WaitForSeconds(GeneralModule.Instance.dealyForScore);

            int t1Score = Mathf.RoundToInt(OnlineMultiplayerManager.Instance.Team1ScoreNetworked);
            int t2Score = Mathf.RoundToInt(OnlineMultiplayerManager.Instance.Team2ScoreNetworked);
            bool amIInTeam1 = PlayerJoiningNumber % 2 != 0;
            bool amIInTeam2 = PlayerJoiningNumber % 2 == 0;
            bool iWon = (amIInTeam1 && t1Score > t2Score) || (amIInTeam2 && t2Score > t1Score);
            bool isDraw = t1Score == t2Score;
    /*
            if (isDraw)
            {
                GameSoundManager.Instance.PlayVictorySound();
            }
            else if (iWon)
            {
                GameSoundManager.Instance.PlayVictorySound();
            }
            else
            {
                GameSoundManager.Instance.PlayChargingSound1();
            }*/

            Rpc_ShowResultAnimation(); // ✅ Show animation
          //  SaccadesUIManager.Instance.GoToScoreBoard(); // ✅ Show scoreboard
        }




        public void PlayerLeft(PlayerRef player)
        {
        
        }

        private void OnDestroy()
        {
        
        
        }

    

        #endregion

    }
