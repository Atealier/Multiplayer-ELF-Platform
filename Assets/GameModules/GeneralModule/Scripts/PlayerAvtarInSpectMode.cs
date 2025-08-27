using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Fusion;
using Fusion.Sockets;
using DG.Tweening;
using System.Linq;

public class PlayerAvtarInSpectMode : MonoBehaviour
{
    public static PlayerAvtarInSpectMode Instance { get; set; }
    [SerializeField] GameObject[] Team1PlayersAvtar;
    [SerializeField] GameObject[] Team2PlayersAvtar;
    [SerializeField] Vector2 bigSize = new Vector2(150, 150); // Size for 1-4 players
    [SerializeField] Vector2 normalSize = new Vector2(100, 100); // Size for more than 4 players
    [SerializeField] private GridLayoutGroup team1Layout; // Assign Team1's GridLayoutGroup
    [SerializeField] private GridLayoutGroup team2Layout; // Assign Team2's GridLayoutGroup

    private Dictionary<int, int> team1AvatarDict = new Dictionary<int, int>();
    private Dictionary<int, int> team2AvatarDict = new Dictionary<int, int>();

    // private Dictionary<Player, GameObject> playerAvatarMap = new Dictionary<Player, GameObject>();
    private Dictionary<IAvatarEntity, GameObject> avatarMap = new Dictionary<IAvatarEntity, GameObject>();
    private Dictionary<IAvatarEntity, Coroutine> particleCoroutines = new Dictionary<IAvatarEntity, Coroutine>();

    public GameObject ScoreBoardButton;
    public Color NormalBorderColor;
    public Color HighlightedBorderColor;
    public Color PowerBorderColor;

    private void Awake()
    {
        Instance = this;
        
    }
      
    private void OnEnable()
    {
        for(int i=0; i< Team1PlayersAvtar.Length; i++)
        {
            Team1PlayersAvtar[i].GetComponent<Image>().color = NormalBorderColor;
            Team2PlayersAvtar[i].GetComponent<Image>().color = NormalBorderColor;
        }

        AnimationManager.Instance.TugOfWarGroup.transform.localPosition = new Vector3(0.12f, 0f, 0f);
        // Clear existing avatars
        foreach (var avatar in Team1PlayersAvtar.Concat(Team2PlayersAvtar))
        {
            avatar.SetActive(false);
        }
        ScoreBoardButton.SetActive(false);
        team1AvatarDict.Clear();
        team2AvatarDict.Clear();
        avatarMap.Clear();

        IAvatarEntity[] entities = FindObjectsOfType<MonoBehaviour>().OfType<IAvatarEntity>().ToArray();
        Debug.Log("Find entities in and its count " + entities.Count());

        if(entities.Count() == 0)
        {
            entities = HeartRateReceiver.Instance.GetAvatarEntities();
            Debug.Log("Count of entities " + entities.Count());
        }

        foreach (IAvatarEntity entity in entities)
        {
            if (entity.IsHostControlled())
                continue;

            int joiningNumber = entity.GetJoiningNumber();
            int index = -1;
            Debug.Log("Check all entity set in avtar ");
            if (joiningNumber % 2 == 0)
            {
                index = Array.IndexOf(GeneralModule.Instance.playerPossibleEvenNumber, joiningNumber);

                Debug.Log("Team2 player avtar length" + Team2PlayersAvtar.Length);
                for (int i = 0; i < Team2PlayersAvtar.Length; i++)
                {
                    if (!team2AvatarDict.ContainsKey(index))
                    {
                        Debug.Log("Index check in on enable" + index);
                        GameObject obj = Team2PlayersAvtar[index];
                        obj.SetActive(true);
                        obj.transform.GetChild(1).GetComponent<Image>().sprite = GeneralModule.Instance.Avtars[entity.GetAvatarIndex()];

                        team2AvatarDict.Add(index, i);
                        avatarMap.Add(entity, obj);
                        break;
                    }
                }
            }
            else
            {
                index = Array.IndexOf(GeneralModule.Instance.playerPossibleOddNumber, joiningNumber);
                               
                Debug.Log("Check joininig number in on enable" + index + " check joining numner of odd" + joiningNumber);
                for (int i = 0; i < Team1PlayersAvtar.Length; i++)
                {
                    if (!team1AvatarDict.ContainsKey(index))
                    {
                        GameObject obj = Team1PlayersAvtar[index];
                        Debug.Log("Name of game object " + obj.name + " check index +" + i);
                        obj.SetActive(true);
                        obj.transform.GetChild(1).GetComponent<Image>().sprite =
                            GeneralModule.Instance.Avtars[entity.GetAvatarIndex()];

                        team1AvatarDict.Add(index, i);
                        avatarMap.Add(entity, obj);
                        break;
                    }
                }
            }
        }

        AdjustAvatarSizes();
    }

    public void UpdateAvatarBorder(IAvatarEntity entity)
    {
        Debug.LogWarning("called update avatar");

        if (avatarMap.TryGetValue(entity, out GameObject avatarObj))
        {
            Image borderImage = avatarObj.GetComponent<Image>();
            DynamicParticleRadius particleController = avatarObj.GetComponentInChildren<DynamicParticleRadius>(true); // include inactive children

            //  Find the nested ParticleAttractorLinear safely
            Transform attractorTransform = avatarObj.transform.Find("Canvas/Particle attractor 004 soft noise/Particle attractor");


            ParticleAttractorLinear particleLinerar = null;
            if (attractorTransform != null)
            {
                particleLinerar = attractorTransform.GetComponent<ParticleAttractorLinear>();
            }

            if(borderImage != null)
            {
                if(entity.ApplyGreenBorder())
                {
                    Debug.Log("Apple green border color");
                    borderImage.color = HighlightedBorderColor; // transparent green
                    borderImage.DOFade(1f, 0.5f).SetEase(Ease.InOutSine);

                    avatarObj.transform.localScale = Vector3.one;
                    //avatarObj.transform.DOScale(1.1f, 0.2f)
                    //    .SetEase(Ease.OutBack)
                    //    .OnComplete(() =>
                    //    {
                    //        avatarObj.transform.DOScale(1f, 0.2f).SetEase(Ease.InBack);
                    //    });
                                        
                }


                if (particleController != null && particleLinerar != null)
                {
                    Debug.Log("Check consecutive point " + entity.GetConsecutiveCorrectAnswers());
                    if (entity.ActivateRoundParticles())
                    {
                        particleController.gameObject.SetActive(false);
                        particleController.gameObject.SetActive(true);

                        // Restart coroutine timer
                        if (particleCoroutines.TryGetValue(entity, out Coroutine existing))
                        {
                            StopCoroutine(existing);
                        }
                        particleCoroutines[entity] = StartCoroutine(TurnOffAfterDelay(entity, particleController.gameObject, 1.5f));

                        particleLinerar.speedAfter = 0f;
                        Debug.Log("[AFTER] ROUND ");
                        //float newZ = (entity.GetConsecutiveCorrectAnswers() * 2f) + 10;
                        particleLinerar.orbitalZAfter = 25f;
                        particleLinerar.ApplyOrbitalZAfter();

                    }
                    else if(entity.ActivateFlowParticles())
                    {
                        particleController.gameObject.SetActive(false);
                        particleController.gameObject.SetActive(true);

                        borderImage.color = PowerBorderColor; // transparent green
                        borderImage.DOFade(1f, 0.5f).SetEase(Ease.InOutSine);

                        // Restart coroutine timer for Flow
                        if (particleCoroutines.TryGetValue(entity, out Coroutine existing))
                        {
                            StopCoroutine(existing);
                        }
                        particleCoroutines[entity] = StartCoroutine(TurnOffAfterDelay(entity, particleController.gameObject, 3f));

                        particleLinerar.orbitalZAfter = 0f;
                        particleLinerar.ApplyOrbitalZAfter();
                        particleLinerar.speedAfter = 0.05f;
                        Debug.Log("[AFTER] FLOW ");
                    }
                    else
                    {
                        Debug.Log("Apple normal border color");
                        borderImage.color = NormalBorderColor;
                        avatarObj.transform.localScale = Vector3.one;
                    }

                }
                else
                {
                    Debug.Log($"Particle components not found for player");
                }
            }
            else
            {
                Debug.LogWarning($"No Border Image found for player");// {player.Name}'s avatar.");
            }
        }
        else
        {
            Debug.LogWarning("PLAYER AVATAR NOT FOUND");
        }
    }

    private IEnumerator TurnOffAfterDelay(IAvatarEntity entity, GameObject particleObj, float delay)
    {
        yield return new WaitForSeconds(delay);
        particleObj.SetActive(false);
        particleCoroutines.Remove(entity);
    }

    public void RemoveAvatar(int joiningNumber)
    {
        if (joiningNumber % 2 == 0)
        {
            int index = Array.IndexOf(OnlineMultiplayerManager.Instance.playerPossibleEvenNumber, joiningNumber);

            if (team2AvatarDict.ContainsKey(index))
            {
                int i = team2AvatarDict[index];
                //Team2PlayersAvtar[i].transform.GetChild(0).GetComponent<Image>().sprite = null; // Old
                Team2PlayersAvtar[i].transform.GetChild(1).GetComponent<Image>().sprite = null;
                Team2PlayersAvtar[i].SetActive(false);
                team2AvatarDict.Remove(index);
            }

        }
        else
        {
            int index = Array.IndexOf(OnlineMultiplayerManager.Instance.playerPossibleOddNumber, joiningNumber);

            if (team1AvatarDict.ContainsKey(index))
            {
                int i = team1AvatarDict[index];
                // Team1PlayersAvtar[i].transform.GetChild(0).GetComponent<Image>().sprite = null; // Old
                Team1PlayersAvtar[i].transform.GetChild(1).GetComponent<Image>().sprite = null;
                Team1PlayersAvtar[i].SetActive(false);
                team1AvatarDict.Remove(index);
            }

        }
        AdjustAvatarSizes();
    }

    private void AdjustAvatarSizes()
    {
        UpdateAvatarSizes(team1Layout);
        UpdateAvatarSizes(team2Layout);
    }
    private void UpdateAvatarSizes(GridLayoutGroup layout)
    {
        int activePlayers = 0;
        foreach (Transform child in layout.transform)
        {
            if (child.gameObject.activeSelf) activePlayers++;
        }

        Vector2 targetSize = (activePlayers <= 4) ? bigSize : normalSize;
        layout.cellSize = targetSize;
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
