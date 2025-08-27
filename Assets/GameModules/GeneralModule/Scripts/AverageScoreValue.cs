using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamPlayersManager : MonoBehaviour
{
    public static TeamPlayersManager Instance { get; set; }

    public int Team1TotalPlayers, Team2TotalPlayers, TotalPlayersJoinedAnyOneTeam;
    public float ScoreValueForTeam1, ScoreValueForTeam2;

    private void Awake()
    {
        Instance = this;
    }

    public void SetScoreValueForEachPlayer()
    {
        Debug.Log("Team1 total players : " + Team1TotalPlayers + " Team2 total players : " + Team2TotalPlayers + " Total players which are joined any one team : " + TotalPlayersJoinedAnyOneTeam);


        if (Team1TotalPlayers != Team2TotalPlayers)
        {
            if (Team1TotalPlayers != 0)
                ScoreValueForTeam1 = TotalPlayersJoinedAnyOneTeam / (float)Team1TotalPlayers;
            else
                ScoreValueForTeam1 = 0;

            Debug.Log("Score value for team1 : " + ScoreValueForTeam1);

            if (Team2TotalPlayers != 0)
                ScoreValueForTeam2 = TotalPlayersJoinedAnyOneTeam / (float)Team2TotalPlayers;
            else
                ScoreValueForTeam2 = 0;

            Debug.Log("Score value for team2 : " + ScoreValueForTeam2);
        }
        else
        {
            ScoreValueForTeam1 = 1;

            Debug.Log("Score value for team1 : " + ScoreValueForTeam1);

            ScoreValueForTeam2 = 1;

            Debug.Log("Score value for team2 : " + ScoreValueForTeam2);
        }

    }
}
