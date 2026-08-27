using System.Collections.Generic;
using UnityEngine;

public class PredictionManager
{
    private readonly int baseReward;
    private readonly float maxOdds;

    public PredictionManager(int baseReward = 10, float maxOdds = 5f)
    {
        this.baseReward = baseReward;
        this.maxOdds = maxOdds;
    }

    private void Save(League league) => LeagueManager.Instance.saveManager.SaveLeague(league);

    private int CurrentRound(League league)
    {
        var my = league.teams.Find(t => t.id == league.settings.playerTeamId);
        return (my != null ? my.played : 0) + 1;
    }

    public List<Prediction> GetOrCreate(League league)
    {
        int round = CurrentRound(league);
        var existing = league.predictions.FindAll(p => p.round == round);
        if (existing.Count > 0) return existing;

        var r = league.schedule.Find(x => x.roundNumber == round);
        if (r == null) return new List<Prediction>();

        int playerId = league.settings.playerTeamId;
        var enemyMatches = r.matches.FindAll(m => m.teamAId != playerId && m.teamBId != playerId);
        if (enemyMatches.Count == 0) return new List<Prediction>();

        // 라운드 시드로 1경기
        var rng = new System.Random(round);
        var m = enemyMatches[rng.Next(enemyMatches.Count)];

        league.predictions.Add(new Prediction
        {
            round = round,
            matchId = m.matchId,
            teamAId = m.teamAId,
            teamBId = m.teamBId,
            pickedTeamId = 0,
            oddsX100 = 0,
            resolved = false
        });
        Save(league);
        return league.predictions.FindAll(p => p.round == round);
    }

    // 배당 = 1/승률 (상한 maxOdds)
    public float CalcOdds(int pickedId, int aId, int bId)
    {
        float powPicked = EnemyTeamService.GetTeamPower(pickedId);
        float powOther = EnemyTeamService.GetTeamPower(pickedId == aId ? bId : aId);
        float total = powPicked + powOther;
        float winProb = total > 0 ? powPicked / total : 0.5f;
        float odds = winProb > 0 ? 1f / winProb : maxOdds;
        return Mathf.Min(odds, maxOdds);
    }

    // 두 팀 배당 표시용
    public (float oddsA, float oddsB) GetOdds(Prediction p)
        => (CalcOdds(p.teamAId, p.teamAId, p.teamBId),
            CalcOdds(p.teamBId, p.teamAId, p.teamBId));

    // 미리보기 보상 
    public int RewardFor(Prediction p, int pickedId)
        => Mathf.RoundToInt(baseReward * CalcOdds(pickedId, p.teamAId, p.teamBId));

    // 확정 보상 
    public int RewardStored(Prediction p)
        => Mathf.RoundToInt(baseReward * (p.oddsX100 / 100f));

    public void Predict(League league, string matchId, int pickedTeamId)
    {
        var p = league.predictions.Find(x => x.matchId == matchId);
        if (p == null || p.pickedTeamId != 0) return;
        p.pickedTeamId = pickedTeamId;
        p.oddsX100 = Mathf.RoundToInt(CalcOdds(pickedTeamId, p.teamAId, p.teamBId) * 100f);
        Save(league);
    }

    // 정산 
    public string Resolve(League league)
    {
        string msg = null;
        bool changed = false;
        foreach (var p in league.predictions)
        {
            if (p.resolved || p.pickedTeamId == 0) continue;
            var r = league.schedule.Find(x => x.roundNumber == p.round);
            var match = r?.matches.Find(m => m.matchId == p.matchId);
            if (match?.result == null) continue;   // 아직 경기 안 함

            if (match.result.winner == p.pickedTeamId)
            {
                int reward = RewardStored(p);
                UserManager.Instance.AddGold(reward);
                msg = $"예측 적중! +{reward}";
            }
            else msg = "예측 실패...";
            p.resolved = true;
            changed = true;
        }
        if (changed) Save(league);
        return msg;
    }
}