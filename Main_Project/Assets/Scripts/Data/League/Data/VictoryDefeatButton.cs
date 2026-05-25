using System;
using UnityEngine;

public class VictoryDefeatButton : MonoBehaviour
{
    public LeagueManager leagueManager;
    private void Start()
    {
        if (leagueManager == null)
            leagueManager = LeagueManager.Instance;
    }

    public void OnClickWin()
    {
        leagueManager.ProcessRoundResult(true);
    }

    public void OnClickLose()
    {
        leagueManager.ProcessRoundResult(false);
    }
}