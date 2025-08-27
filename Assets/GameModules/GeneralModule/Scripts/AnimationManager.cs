using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    public static AnimationManager Instance { get; private set; }

    public CharacterAnimationController Team1Character, Team2Character;
    public GameObject TugOfWarGroup;
    private float minX, maxX, maxOffset;
    public GameObject Team1Rope, Team2Rope;
    float edgeBuffer = 0.35f;

    public GameObject GamePlayAnimation;

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
    }

    
    private void Start()
    {
        if(TugOfWarGroup != null && Team1Character != null && Team2Character != null)
        {
            //Bounds team1Bounds = GetCharacterBounds(Team1Character.gameObject);
            //Bounds team2Bounds = GetCharacterBounds(Team2Character.gameObject);
            Bounds team1Bounds = GetRopeBounds(Team1Character.gameObject);
            Bounds team2Bounds = GetRopeBounds(Team2Character.gameObject);
            float leftEdge = team1Bounds.min.x;
            float rightEdge = team2Bounds.max.x;
            float leftScreenEdge = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).x;
            float rightScreenEdge = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x;
            minX = leftScreenEdge - leftEdge;
            maxX = rightScreenEdge - rightEdge;
            maxOffset = maxX;
        }
    }

    private Bounds GetRopeBounds(GameObject rope)
    {
        Bounds bounds = new Bounds(rope.transform.position, Vector3.zero);
        SpriteRenderer[] renderers = rope.GetComponentsInChildren<SpriteRenderer>();
        if (renderers.Length == 0) return bounds;

        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer.enabled)
            {
                if (bounds.size == Vector3.zero) bounds = renderer.bounds;
                else bounds.Encapsulate(renderer.bounds);
            }
        }
        return bounds;
    }

    private Bounds GetCharacterBounds(GameObject character)
    {
        Bounds bounds = new Bounds(character.transform.position, Vector3.zero);
        SpriteRenderer[] renderers = character.GetComponentsInChildren<SpriteRenderer>();
        if (renderers.Length == 0) return bounds;

        foreach (SpriteRenderer renderer in renderers)
        {
            if(renderer.enabled)
            {
                if (bounds.size == Vector3.zero) bounds = renderer.bounds;
                else bounds.Encapsulate(renderer.bounds);
            }
        }
        return bounds;
    }
    public void UpdateTugOfWar(float team1Score, float team2Score)
    {
        if (TugOfWarGroup == null) return;

        Debug.Log("Update Thug Of War");
        int t1Score = Mathf.RoundToInt(team1Score);
        int t2Score = Mathf.RoundToInt(team2Score);
        float difference = t1Score - t2Score;
        float factor = Mathf.Clamp(difference / 30f, -1f, 1f);
        float targetX = -factor * maxOffset;
        float speed = 2.0f;
        Vector3 targetPos = new Vector3(targetX, TugOfWarGroup.transform.position.y, TugOfWarGroup.transform.position.z);

        if (Team1Rope != null && Team2Rope != null)
        {
            // Get rope bounds
            Bounds ropeBounds = GetRopeBounds(TugOfWarGroup); // you can also make a GetRopeBounds() if needed
            float leftScreenEdge = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).x;
            float rightScreenEdge = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x;

            // Calculate future rope edges after moving TugOfWarGroup
            float ropeLeft = ropeBounds.min.x + (targetPos.x - TugOfWarGroup.transform.position.x);
            float ropeRight = ropeBounds.max.x + (targetPos.x - TugOfWarGroup.transform.position.x);

            // Adjust targetPos.x if rope goes off-screen
            if (ropeLeft < leftScreenEdge - edgeBuffer) targetPos.x += (leftScreenEdge - edgeBuffer) - ropeLeft;
            if (ropeRight > rightScreenEdge + edgeBuffer) targetPos.x -= ropeRight - (rightScreenEdge + edgeBuffer);
        }

        TugOfWarGroup.transform.position = Vector3.Lerp(TugOfWarGroup.transform.position, targetPos, Time.deltaTime * speed);
    }

    public void UpdateCharacterAnimations(float team1Score, float team2Score)
    {
        int t1Score = Mathf.RoundToInt(team1Score);
        int t2Score = Mathf.RoundToInt(team2Score);
        if (t1Score == 0 && t2Score == 0) return;
        bool team1Leads;
        if (t1Score == 0 || t2Score == 0)
        {
            int leadingScore = Mathf.Max(t1Score, t2Score);
            team1Leads = t1Score > t2Score;
            int diff = leadingScore <= 20 ? 10 : (leadingScore <= 30 ? 20 : 30);
            Team1Character.UpdateAnimationState(diff, team1Leads);
            Team2Character.UpdateAnimationState(diff, !team1Leads);
            return;
        }

        int higherScore = Mathf.Max(t1Score, t2Score);
        int lowerScore = Mathf.Min(t1Score, t2Score);
        float percentageDifference = (float)(higherScore - lowerScore) / higherScore * 100;
        team1Leads = t1Score > t2Score;
        int diffValue = percentageDifference < 10 ? 9 : (percentageDifference < 20 ? 10 : (percentageDifference < 30 ? 20 : 30));
        Team1Character.UpdateAnimationState(diffValue, team1Leads);
        Team2Character.UpdateAnimationState(diffValue, !team1Leads);
    }

}
