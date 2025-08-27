using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine.Networking;

public class PlayerData : IAvatarEntity
{
    public string id;
    public int age;
    public string imageUrl;
    public string name; // Temporary name, defaults to ID
    public string team;
    public int joiningNumber;
    public int bpm;
    public float lastUpdateTime;
    public int avatarIndex;
    public Sprite avatar; // Placeholder for avatar
    public int consecutiveHighPoints = 0; // Tracks consecutive updates with max points
    public int lastPoints = 0; // Points from the previous update
    private HeartRateReceiver receiver;

    public PlayerData(string id, HeartRateReceiver receiver)
    {
        this.id = id;
        this.name = id; // Default name is ID
        this.bpm = 0;
        this.lastUpdateTime = Time.time;
        this.receiver = receiver;
    }

    public void UpdateBPM(int newBPM)
    {
        bpm = newBPM;
    }

    public void UpdatePlayerInfo(string newName, int newAge, string newImageUrl, Sprite newAvatar)
    {
        name = newName;
        age = newAge;
        imageUrl = newImageUrl;
        avatar = newAvatar;
    }

    // IAvatarEntity interface implementation
    public int GetAvatarIndex() => avatarIndex;
    public int GetConsecutiveCorrectAnswers() => consecutiveHighPoints; // Not used in this context, default to 0
    public string GetEntityName() => name;
    public bool IsHostControlled() => false; // No host concept here, default to false
    public int GetJoiningNumber() => joiningNumber;

    public bool ApplyGreenBorder() => (consecutiveHighPoints == receiver.PointsForMediumHeartRateValue);
    public bool ActivateRoundParticles() => (consecutiveHighPoints == receiver.PointsForMediumHeartRateValue);
    public bool ActivateFlowParticles() => consecutiveHighPoints == receiver.PointsForHighHeartRateValue;
}

public class HeartRateReceiver : MonoBehaviour
{
    public static HeartRateReceiver Instance { get; set; }

    #if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void ConnectToMQTT(string brokerUrl, int port, string clientId, string topic);

        [DllImport("__Internal")]
        private static extern void DisconnectMQTT();
    #else
    private MqttClient client;
#endif

    //private MqttClient client;
    private string apiUrl = "https://keshavinfotechdemo2.com/keshav/KG2/AndyT/index.php";
    public string brokerAddress = "localhost";
    public int brokerPort = 1883; // Default to standard MQTT port for non-WebGL
    public int webGLPort = 9001;  // WebSocket port for WebGL
    public string topic = "heartRate";
    public int LowHeartRateValue;
    public int HighHeartRateValue;
    public int PointsForLowHeartRateValue;
    public int PointsForMediumHeartRateValue;
    public int PointsForHighHeartRateValue;

    [Serializable]
    public class HeartRateData
    {
        public string id;
        public int bpm;
    }

    [Serializable]
    public class VerificationResult
    {
        public string name;
        public int age;
        public string image_url;
    }

    [Serializable]
    public class VerificationResponse
    {
        public int status;
        public string message;
        public VerificationResult result;
    }

    private ConcurrentQueue<HeartRateData> heartRateQueue = new ConcurrentQueue<HeartRateData>();
    private Dictionary<string, PlayerData> players = new Dictionary<string, PlayerData>();
    public Dictionary<string, int> teamScores = new Dictionary<string, int> { { "Team1", 0 }, { "Team2", 0 } };
    private Dictionary<string, List<string>> teamPlayers = new Dictionary<string, List<string>>
    {
        { "Team1", new List<string>() },
        { "Team2", new List<string>() }
    };
    private float lastUpdateTime = 0f;
    private float updateInterval = 3f; // Check every second
    public bool isGameRunning = false;
    public HeartRateGameManager heartRateGameManager;

    private List<int> availableTeam1Numbers = new List<int> { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25, 27, 29, 31 };
    private List<int> availableTeam2Numbers = new List<int> { 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32 };

    private HashSet<string> verifiedIds = new HashSet<string>(); // Track verified IDs

    //[Header("Avatar Effect Thresholds (Configurable)")]
    // public int AvtarBorderHighlightThreshold;
    // public int AvtarRoundParticleThreshold;
    // public int AvtarFlowParticleThreshold;



    private void Awake()
    {
        Instance = this;
        InitializeMQTT();
    }

    void Start()
    {
   
    }

    private void InitializeMQTT()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
                    StartCoroutine(ConnectToWebGLMQTT());
        #else
                if (client == null || !client.IsConnected)
                {
                    try
                    {
                        client = new MqttClient(brokerAddress);
                        client.Connect("UnityClient_" + Guid.NewGuid().ToString());
                        client.MqttMsgPublishReceived += OnMessageReceived;
                        client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
                        Debug.Log("Connected to Mosquitto broker at " + brokerAddress + " and subscribed to " + topic);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("MQTT initialization failed: " + e.Message);
                    }
                }
        #endif
    }

    private IEnumerator ConnectToWebGLMQTT()
    {
        yield return new WaitForSeconds(3f);
        #if UNITY_WEBGL && !UNITY_EDITOR
            ConnectToMQTT(brokerAddress, webGLPort, "UnityClient_" + Guid.NewGuid().ToString(), topic); // Use brokerPort 8000
        #endif
    }


    void Update()
    {

        // Your existing Update logic (if any)
        if (Time.time - lastUpdateTime > updateInterval)
        {
            lastUpdateTime = Time.time;

            // Process all queued heart rate data
            while (heartRateQueue.TryDequeue(out HeartRateData data))
            {
                ProcessHeartRate(data.id, data.bpm);
            }

            CheckPlayerTimeouts();
            if (isGameRunning)
            {
                UpdateTeamScores();
                UpdateGameplayAnimation();
            }
        }
    }

    #if UNITY_WEBGL && !UNITY_EDITOR
    public void OnConnected()
    {
        Debug.Log("WebGL: Connected to localhost WebSocket MQTT broker");
    }

    public void OnConnectionFailed(string error)
    {
        Debug.LogError("WebGL: MQTT connection failed: " + error);
    }

    public void OnConnectionLost(string error)
    {
        Debug.LogError("WebGL: MQTT connection lost: " + error);
    }

    public void OnMessageReceivedJS(string message)
    {
        Debug.Log($"WebGL: Raw MQTT message: {message}");
        try
        {
            List<HeartRateData> heartRateData = JsonConvert.DeserializeObject<List<HeartRateData>>(message);
            foreach (var data in heartRateData)
            {
                Debug.Log($"WebGL: Received: ID={data.id}, BPM={data.bpm}");
                heartRateQueue.Enqueue(data);
            }
        }
        catch (Exception ex)
        {
            try
            {
                HeartRateData data = JsonConvert.DeserializeObject<HeartRateData>(message);
                Debug.Log($"WebGL: Received single: ID={data.id}, BPM={data.bpm}");
                heartRateQueue.Enqueue(data);
            }
            catch
            {
                Debug.LogError("WebGL: Failed to parse JSON: " + ex.Message + "\nMessage: " + message);
            }
        }
    }

    #else
    void OnMessageReceived(object sender, MqttMsgPublishEventArgs e)
    {
        Debug.Log("On msg reveived ");
        if (client == null) return;

        string message = Encoding.UTF8.GetString(e.Message);
        try
        {
            List<HeartRateData> heartRateData = JsonConvert.DeserializeObject<List<HeartRateData>>(message);
            foreach (var data in heartRateData)
            {
                Debug.Log($"Received: ID={data.id}, BPM={data.bpm}");
                //ProcessHeartRate(data.id, data.bpm);
                heartRateQueue.Enqueue(data); // Queue the data
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to parse JSON: " + ex.Message + "\nMessage: " + message);
        }
    }
    #endif

    void ProcessHeartRate(string id, int bpm)
    {
        if (players.Count > 32) return; // Max 32 players

        if (!players.ContainsKey(id) && isGameRunning) return;

        if (!players.ContainsKey(id))
        {
            int avtarIndex = GetRandomAvtar();
            players[id] = new PlayerData(id, this);
            players[id].avatar = GeneralModule.Instance.Avtars[avtarIndex];
            players[id].avatarIndex = avtarIndex;
            AssignToTeam(id);
            HeartRateUIManager.Instance.UpdatePlayerUI(id); // Update UI for new player
        }

        players[id].UpdateBPM(bpm);
        players[id].lastUpdateTime = Time.time;

        int currentPoints = CalculatePoints(bpm);
        if (currentPoints == PointsForHighHeartRateValue) // Assuming 3 is the maximum points
        {
            players[id].consecutiveHighPoints = PointsForHighHeartRateValue;
        }
        else if (currentPoints == PointsForMediumHeartRateValue)
        {
            players[id].consecutiveHighPoints = PointsForMediumHeartRateValue;
        }
        else
        {
            players[id].consecutiveHighPoints = PointsForLowHeartRateValue;
        }
        Debug.Log("Player id" + id + players[id].consecutiveHighPoints);
        players[id].lastPoints = currentPoints;

        // USE when need to verify through backend and comment above code.

        //if (!players.ContainsKey(id))
        //{
        //    if (!verifiedIds.Contains(id)) // Check if ID has not been verified yet
        //    {
        //        verifiedIds.Add(id);
        //        StartCoroutine(VerifyPlayer(id, bpm));
        //    }
        //}
        //else
        //{
        //    players[id].UpdateBPM(bpm);
        //    players[id].lastUpdateTime = Time.time;

        //    int currentPoints = CalculatePoints(bpm);
        //    if (currentPoints == PointsForHighHeartRateValue)
        //    {
        //        players[id].consecutiveHighPoints = PointsForHighHeartRateValue;
        //    }
        //    else if (currentPoints == PointsForMediumHeartRateValue)
        //    {
        //        players[id].consecutiveHighPoints = PointsForMediumHeartRateValue;
        //    }
        //    else
        //    {
        //        players[id].consecutiveHighPoints = PointsForLowHeartRateValue;
        //    }
        //    Debug.Log("Player id " + id + " points: " + players[id].consecutiveHighPoints);
        //    players[id].lastPoints = currentPoints;
        //}
    }

    private IEnumerator VerifyPlayer(string id, int bpm)
    {
        // Try GET request first (primary approach for retrieval)
        string getUrl = apiUrl + "?id=" + UnityWebRequest.EscapeURL(id);
        Debug.Log("Check url" + getUrl);
        using (UnityWebRequest getRequest = UnityWebRequest.Get(getUrl))
        {
            yield return getRequest.SendWebRequest();

            Debug.Log("GET Verification response for ID " + id + ": " + getRequest.downloadHandler.text);

            if (getRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    VerificationResponse response = JsonConvert.DeserializeObject<VerificationResponse>(getRequest.downloadHandler.text);
                    if (response != null && response.status == 1 && response.result != null && !string.IsNullOrEmpty(response.result.name))
                    {
                        StartCoroutine(AddPlayerWithAvatar(id, bpm, response.result.name, response.result.age, response.result.image_url));
                        verifiedIds.Add(id); // Mark ID as verified
                        Debug.Log("ID " + id + " verified via GET successfully with status " + response.status);
                        yield break; // Exit if successful
                    }
                    else
                    {
                        Debug.Log("Invalid GET response for ID " + id);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("Failed to parse GET response for ID " + id + ": " + ex.Message);
                }
            }
            else
            {
                Debug.LogError("GET verification failed for ID " + id + ": " + getRequest.error + " (Status Code: " + getRequest.responseCode + ")");
            }
        }
    }

    private IEnumerator AddPlayerWithAvatar(string id, int bpm, string name, int age, string imageUrl)
    {
        Sprite avatarSprite = null;
        if (!string.IsNullOrEmpty(imageUrl))
        {
            using (UnityWebRequest imageRequest = UnityWebRequestTexture.GetTexture(imageUrl))
            {
                yield return imageRequest.SendWebRequest();
                if (imageRequest.result == UnityWebRequest.Result.Success)
                {
                    Texture2D texture = ((DownloadHandlerTexture)imageRequest.downloadHandler).texture;
                    avatarSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                }
                else
                {
                    Debug.LogWarning($"Failed to load image for ID {id}: {imageRequest.error}");
                    avatarSprite = GeneralModule.Instance.Avtars[GetRandomAvtar()];
                }
            }
        }
        else
        {
            avatarSprite = GeneralModule.Instance.Avtars[GetRandomAvtar()];
        }

        players[id] = new PlayerData(id, this);
        players[id].UpdatePlayerInfo(name, age, imageUrl, avatarSprite);
        players[id].UpdateBPM(bpm);
        players[id].lastUpdateTime = Time.time;
        AssignToTeam(id);
        HeartRateUIManager.Instance.UpdatePlayerUI(id);
    }


    public int GetRandomAvtar()
    {
        return UnityEngine.Random.Range(0, GeneralModule.Instance.Avtars.Count());
        
    }

    void AssignToTeam(string id)
    {
        if (teamPlayers["Team1"].Count >= 16 && teamPlayers["Team2"].Count >= 16) return; // Both teams full

        string team = teamPlayers["Team1"].Count <= teamPlayers["Team2"].Count ? "Team1" : "Team2";
        if (teamPlayers[team].Count >= 16)
            team = team == "Team1" ? "Team2" : "Team1"; // Switch if target team is full

        int joiningNumber;
        if (team == "Team1")
        {
            if (availableTeam1Numbers.Count > 0)
            {
                joiningNumber = availableTeam1Numbers[0];
                availableTeam1Numbers.RemoveAt(0);
            }
            else
            {
                Debug.LogError("No available numbers for Team1");
                return;
            }
        }
        else
        {
            if (availableTeam2Numbers.Count > 0)
            {
                joiningNumber = availableTeam2Numbers[0];
                availableTeam2Numbers.RemoveAt(0);
            }
            else
            {
                Debug.LogError("No available numbers for Team2");
                return;
            }
        }

        teamPlayers[team].Add(id);
        players[id].team = team;
        players[id].joiningNumber = joiningNumber; // Assign the joining number
        Debug.Log($"Assigned {id} to {team} with joiningNumber {joiningNumber}");

        UpdateTeamData();

    }

    public void SwitchTeam(string id)
    {
        if (!players.ContainsKey(id) || isGameRunning) return;

        string currentTeam = players[id].team;
        string newTeam = currentTeam == "Team1" ? "Team2" : "Team1";

        if (teamPlayers[newTeam].Count >= 16) return; // Target team full

        // Free up the current joiningNumber
        int currentJoiningNumber = players[id].joiningNumber;
        if (currentTeam == "Team1")
        {
            availableTeam1Numbers.Add(currentJoiningNumber);
            availableTeam1Numbers.Sort();
        }
        else
        {
            availableTeam2Numbers.Add(currentJoiningNumber);
            availableTeam2Numbers.Sort();
        }

        // Assign new joiningNumber for the new team
        int newJoiningNumber;
        if (newTeam == "Team1")
        {
            if (availableTeam1Numbers.Count > 0)
            {
                newJoiningNumber = availableTeam1Numbers[0];
                availableTeam1Numbers.RemoveAt(0);
            }
            else
            {
                Debug.LogError("No available numbers for Team1");
                return;
            }
        }
        else
        {
            if (availableTeam2Numbers.Count > 0)
            {
                newJoiningNumber = availableTeam2Numbers[0];
                availableTeam2Numbers.RemoveAt(0);
            }
            else
            {
                Debug.LogError("No available numbers for Team2");
                return;
            }
        }

        teamPlayers[currentTeam].Remove(id);
        teamPlayers[newTeam].Add(id);
        players[id].team = newTeam;
        players[id].joiningNumber = newJoiningNumber;
        HeartRateUIManager.Instance.UpdatePlayerUI(id);
        Debug.Log($"Switched {id} from {currentTeam} to {newTeam}");

        UpdateTeamData();
    }

    void CheckPlayerTimeouts()
    {
        float currentTime = Time.time;
        List<string> playersToRemove = new List<string>();

        foreach (var player in players)
        {
            if (currentTime - player.Value.lastUpdateTime > 10f) // 10 seconds timeout
            {
                playersToRemove.Add(player.Key);
            }
        }

        foreach (var id in playersToRemove)
        {
            RemovePlayer(id);
        }

        if (isGameRunning && (teamPlayers["Team1"].Count == 0 || teamPlayers["Team2"].Count == 0))
        {
            EndGame();
        }
    }

    void RemovePlayer(string id)
    {
        Debug.Log("Remove player after timeout");
        if (players.ContainsKey(id))
        {
            string team = players[id].team;
            int joiningNumber = players[id].joiningNumber;
            teamPlayers[team].Remove(id);
            if (team == "Team1")
            {
                availableTeam1Numbers.Add(joiningNumber);
                availableTeam1Numbers.Sort();
            }
            else
            {
                availableTeam2Numbers.Add(joiningNumber);
                availableTeam2Numbers.Sort();
            }
            if (isGameRunning)
                PlayerAvtarInSpectMode.Instance.RemoveAvatar(joiningNumber);
            players.Remove(id);
            HeartRateUIManager.Instance.UpdatePlayerUI(id, true); // Remove from UI
            Debug.Log($"Removed player {id} from {team}");

            UpdateTeamData();

        }
    }

    void UpdateTeamData()
    {
        TeamPlayersManager.Instance.Team1TotalPlayers = teamPlayers["Team1"].Count;
        TeamPlayersManager.Instance.Team2TotalPlayers = teamPlayers["Team2"].Count;
        TeamPlayersManager.Instance.TotalPlayersJoinedAnyOneTeam = teamPlayers["Team1"].Count + teamPlayers["Team2"].Count;
        TeamPlayersManager.Instance.SetScoreValueForEachPlayer();
    }

    void UpdateTeamScores()
    {    
        foreach (var player in players)
        {
            string team = player.Value.team;
            int points = CalculatePoints(player.Value.bpm);
            float adjustedPoints = points * (team == "Team1" ? TeamPlayersManager.Instance.ScoreValueForTeam1 : TeamPlayersManager.Instance.ScoreValueForTeam2);
            teamScores[team] += (int)adjustedPoints;
            Debug.Log($"Player {player.Key} in {team} contributes {adjustedPoints} points (raw: {points}, multiplier: {(team == "Team1" ? TeamPlayersManager.Instance.ScoreValueForTeam1 : TeamPlayersManager.Instance.ScoreValueForTeam2)})");
        }

        foreach (var player in players)
        {
            Debug.Log(player.Value.consecutiveHighPoints + "consecutiveHighPoints " + player.Value.name);
            if (player.Value.consecutiveHighPoints == PointsForHighHeartRateValue)
            {                
                // Trigger full consecutive animation for the player's team
                if (player.Value.team == "Team1")
                {
                    AnimationManager.Instance.Team1Character.UpdateAnimationBasedConsecutiveAnswers();
                }
                else
                {
                    AnimationManager.Instance.Team2Character.UpdateAnimationBasedConsecutiveAnswers();
                }
                PlayerAvtarInSpectMode.Instance.UpdateAvatarBorder(player.Value);
                // Reset the counter after triggering
                //player.Value.consecutiveHighPoints = 0;
            }
            else if (player.Value.consecutiveHighPoints == PointsForMediumHeartRateValue)
            {
                // Trigger half consecutive animation for the player's team
                if (player.Value.team == "Team1")
                {
                    AnimationManager.Instance.Team1Character.UpdateAnimationBasedHalfConsecutiveAnswers();
                }
                else
                {
                    AnimationManager.Instance.Team2Character.UpdateAnimationBasedHalfConsecutiveAnswers();
                }
                PlayerAvtarInSpectMode.Instance.UpdateAvatarBorder(player.Value);
                // Optionally reset or keep the counter (keeping it here to allow progression to 5)
            }
            else
            {
                PlayerAvtarInSpectMode.Instance.UpdateAvatarBorder(player.Value);
            }
        }

        // Average scores if teams are unequal
        //if (teamPlayers["Team1"].Count != teamPlayers["Team2"].Count)
        //{
        //    teamScores["Team1"] = teamPlayers["Team1"].Count > 0 ? (int)(teamScores["Team1"] / (float)teamPlayers["Team1"].Count) : 0;
        //    teamScores["Team2"] = teamPlayers["Team2"].Count > 0 ? (int)(teamScores["Team2"] / (float)teamPlayers["Team2"].Count) : 0;
        //}

        AnimationManager.Instance.UpdateCharacterAnimations(teamScores["Team1"], teamScores["Team2"]);
        AnimationManager.Instance.UpdateTugOfWar(teamScores["Team1"], teamScores["Team2"]);
        GeneralModule.Instance.Team1BoardScoreText.text = teamScores["Team1"].ToString();
        GeneralModule.Instance.Team2BoardScoreText.text = teamScores["Team2"].ToString();
       // HeartRateUIManager.Instance.UpdateScores(teamScores["Team1"], teamScores["Team2"]);
    }


    int CalculatePoints(int bpm)
    {
        if (bpm < LowHeartRateValue) return PointsForLowHeartRateValue;
        else if (bpm <= HighHeartRateValue) return PointsForMediumHeartRateValue;
        else return PointsForHighHeartRateValue;
    }

    void UpdateGameplayAnimation()
    {
        if (teamScores["Team1"] > teamScores["Team2"])
            Debug.Log("Animation: Team1 winning");
        else if (teamScores["Team2"] > teamScores["Team1"])
            Debug.Log("Animation: Team2 winning");
        else
            Debug.Log("Animation: Tie game");
    }

    public IEnumerator StartGame()
    {
        yield return StartCoroutine(IntroAnimationManager.Instance.HandleIntroAnimationAndPlayerLogic());

        if (teamPlayers["Team1"].Count > 0 && teamPlayers["Team2"].Count > 0)
        {

            // HeartRateUIManager.Instance.ShowGameplayPanel(true);
            HeartRateUIManager.Instance.HeartRateModuleUI.SetActive(false);
            GeneralModule.Instance.Canvas.SetActive(false);
            AnimationManager.Instance.GamePlayAnimation.SetActive(true);
            yield return new WaitForSeconds(3f);

            // Start the game
            isGameRunning = true;
            heartRateGameManager.isPlaying = true;

            Debug.Log("Game Started");
        }
        else
        {
            Debug.Log("Cannot start game: Each team needs at least 1 player");
        }
    }

    void EndGame()
    {
        isGameRunning = false;
        HeartRateUIManager.Instance.ShowGameplayPanel(false);
        string winner = teamPlayers["Team1"].Count > 0 ? "Team1" : "Team2";
        Debug.Log($"Game Over: {winner} wins!");
    }

    public void SetPlayerName(string id, string newName)
    {
        if (players.ContainsKey(id))
        {
            players[id].name = newName;
            HeartRateUIManager.Instance.UpdatePlayerUI(id);
        }
    }

    public void ResetGame()
    {
        teamScores["Team1"] = 0;
        teamScores["Team2"] = 0;

        HeartRateUIManager.Instance.gameLengthSlider.value = HeartRateUIManager.Instance.GameLenghtMaxValue;
        heartRateGameManager.countdownTime = HeartRateUIManager.Instance.GameLenghtMaxValue * 60;
    }
    public Dictionary<string, PlayerData> GetPlayers() => players;
    public Dictionary<string, List<string>> GetTeamPlayers() => teamPlayers;

    public IAvatarEntity[] GetAvatarEntities()
    {
        return players.Values.ToArray();
    }

    private void OnEnable()
    {
        Debug.Log("OnEnable called, resetting game state");
        // Clear existing data
        heartRateQueue = new ConcurrentQueue<HeartRateData>(); // Reset queue
        players.Clear();
        teamPlayers["Team1"].Clear();
        teamPlayers["Team2"].Clear();
        verifiedIds.Clear();
        ResetGame();
        HeartRateUIManager.Instance.ResetUI();
        TeamPlayersManager.Instance.Team1TotalPlayers = 0;
        TeamPlayersManager.Instance.Team2TotalPlayers = 0;
        TeamPlayersManager.Instance.TotalPlayersJoinedAnyOneTeam = 0;

        availableTeam1Numbers = new List<int> { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25, 27, 29, 31 };
        availableTeam2Numbers = new List<int> { 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32 };

        // Reinitialize MQTT if needed
        InitializeMQTT();
        isGameRunning = false; // Ensure game is not running on reset

    }

    private void OnDisable()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
                DisconnectMQTT();
        #else
            if (client != null && client.IsConnected)
            {
                client.Disconnect();
                Debug.Log("Disconnected from Mosquitto broker");
            }
        #endif
    }

    void OnDestroy()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        DisconnectMQTT();
        #else
        if (client != null && client.IsConnected)
        {
            client.Disconnect();
            Debug.Log("Disconnected from Mosquitto broker");
        }
        #endif
    }
}