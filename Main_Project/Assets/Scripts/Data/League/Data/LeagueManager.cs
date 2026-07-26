using UnityEngine;
using System.Linq;
using System;
using System.IO;
using System.Collections.Generic;


public class LeagueManager : MonoBehaviour
{
    public static LeagueManager Instance { get; private set; }

    public League league;

    public LeagueSaveManager saveManager;
    private LeagueSettingManager settingManager;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);

            saveManager = GetComponent<LeagueSaveManager>();
            settingManager = GetComponent<LeagueSettingManager>();

            league = saveManager.LoadLeague();

            if (league != null)
            {
                Debug.Log("기존 리그 데이터 로드 완료");
            }
            else
            {
                Debug.Log("저장된 리그 데이터가 없습니다. 게임 시작 시 생성됩니다.");
            }

            Debug.Log("LeagueManager 초기화 완료");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    // 항상 새로운 리그 데이터를 생성 (버튼에서 호출)
    public void NewLeague()
    {
        
        // 기존 save 파일 삭제
        if (File.Exists(saveManager.SavePath))
        {
            File.Delete(saveManager.SavePath);
            Debug.Log("기존 리그 세이브 파일 삭제 완료");
        }
        
        league = settingManager.InitializeLeague();
        saveManager.SaveLeague(league);

        CalculateRanking();

        Debug.Log("새로운 리그 데이터 생성 완료");
    }
    
    // 순위 계산 (공동 랭크 반영)
    public void CalculateRanking()
    {
        var sorted = league.teams
            .OrderByDescending(t => t.points)
            .ThenByDescending(t => (t.goalsFor - t.goalsAgainst))
            .ThenByDescending(t => t.goalsFor)
            .ToList();

        int currentRank = 1;
        int sameRankCount = 1;

        for (int i = 0; i < sorted.Count; i++)
        {
            if (i == 0)
            {
                // 첫 팀은 1위
                sorted[i].rank = currentRank;
            }
            else
            {
                Team prev = sorted[i - 1];
                Team curr = sorted[i];

                if (curr.points == prev.points &&
                    (curr.goalsFor - curr.goalsAgainst) == (prev.goalsFor - prev.goalsAgainst) &&
                    curr.goalsFor == prev.goalsFor)
                {
                    // 동률 → 같은 rank 부여
                    curr.rank = prev.rank;
                    sameRankCount++;
                }
                else
                {
                    // 동률 아님 → 순위 증가
                    currentRank += sameRankCount;
                    curr.rank = currentRank;
                    sameRankCount = 1;
                }
            }
        }

        Debug.Log("순위 계산 완료 (공동 랭크 반영)");
    }
    
    // 경기 결과 반영 (예시)
    public void UpdateMatchResult(int roundNumber, string matchId, Result result)
    {
        Round round = league.schedule.Find(r => r.roundNumber == roundNumber);
        if (round == null)
        {
            Debug.LogError($"라운드 {roundNumber}를 찾을 수 없습니다.");
            return;
        }

        LeagueMatch match = round.matches.Find(m => m.matchId == matchId);
        if (match == null)
        {
            Debug.LogError($"매치 {matchId}를 찾을 수 없습니다.");
            return;
        }

        match.result = result;

        // 팀 전적 업데이트
        ApplyMatchResultToTeams(match.teamAId, match.teamBId, result);

        // 순위 계산
        CalculateRanking();

        // 저장
        saveManager.SaveLeague(league);

        Debug.Log($"경기 결과 업데이트 완료: {matchId}");
    }
    
    // 팀 전적 업데이트
    private void ApplyMatchResultToTeams(int teamAId, int teamBId, Result result)
    {
        Team teamA = league.teams.Find(t => t.id == teamAId);
        Team teamB = league.teams.Find(t => t.id == teamBId);

        if (teamA == null || teamB == null)
        {
            Debug.LogError("팀 정보를 찾을 수 없습니다.");
            return;
        }

        teamA.played++;
        teamB.played++;

        teamA.goalsFor += result.scoreA;
        teamA.goalsAgainst += result.scoreB;

        teamB.goalsFor += result.scoreB;
        teamB.goalsAgainst += result.scoreA;

        if (result.scoreA > result.scoreB)
        {
            teamA.win++;
            teamB.lose++;
            teamA.points += league.settings.pointRule.win;
            teamB.points += league.settings.pointRule.lose;
        }
        else if (result.scoreA < result.scoreB)
        {
            teamB.win++;
            teamA.lose++;
            teamB.points += league.settings.pointRule.win;
            teamA.points += league.settings.pointRule.lose;
        }
        else
        {
            teamA.draw++;
            teamB.draw++;
            teamA.points += league.settings.pointRule.draw;
            teamB.points += league.settings.pointRule.draw;
        }
    }
    
    private int GetCurrentRoundNumber()
    {
        int myTeamId = league.settings.playerTeamId;
        Team myTeam = league.teams.Find(t => t.id == myTeamId);

        if (myTeam != null)
        {
            return myTeam.played + 1; // played=0이면 1라운드
        }
        else
        {
            Debug.LogWarning("내 팀 정보를 찾을 수 없습니다.");
            return 1;
        }
    }

    public void ProcessRoundResult(bool isPlayerWin)
    {
        int currentRound = GetCurrentRoundNumber();
        Round round = league.schedule.Find(r => r.roundNumber == currentRound);

        if (round == null)
        {
            Debug.LogWarning("현재 라운드 정보를 찾을 수 없습니다.");
            return;
        }

        int playerTeamId = league.settings.playerTeamId;

        foreach (var match in round.matches)
        {
            Team teamA = league.teams.Find(t => t.id == match.teamAId);
            Team teamB = league.teams.Find(t => t.id == match.teamBId);

            if (teamA == null || teamB == null)
            {
                Debug.LogWarning("팀 정보를 찾을 수 없습니다.");
                continue;
            }

            Result result = new Result();

            if (match.teamAId == playerTeamId || match.teamBId == playerTeamId)
            {
                // 플레이어 경기 처리
                if (isPlayerWin)
                {
                    result.scoreA = match.teamAId == playerTeamId ? 1 : 0;
                    result.scoreB = match.teamBId == playerTeamId ? 1 : 0;
                    result.winner = playerTeamId;
                    
                    UserManager.Instance.AddGold(100);
                }
                else
                {
                    result.scoreA = match.teamAId == playerTeamId ? 0 : 1;
                    result.scoreB = match.teamBId == playerTeamId ? 0 : 1;
                    result.winner = match.teamAId == playerTeamId ? match.teamBId : match.teamAId;
                    
                    UserManager.Instance.AddGold(50);
                }
            }
            else
            {
                // 랜덤 결과 처리
                int rand = UnityEngine.Random.Range(0, 3); // 0=draw, 1=teamA win, 2=teamB win

                if (rand == 0)
                {
                    result.scoreA = 1;
                    result.scoreB = 1;
                    result.winner = 0; // 무승부
                }
                else if (rand == 1)
                {
                    result.scoreA = 2;
                    result.scoreB = 0;
                    result.winner = teamA.id;
                }
                else
                {
                    result.scoreA = 0;
                    result.scoreB = 2;
                    result.winner = teamB.id;
                }
            }

            // 결과 반영
            match.result = result;
            ApplyMatchResultToTeams(teamA.id, teamB.id, result);
        }

        // 적 유닛 레벨 랜덤 성장
        EnemyTeamService.GrowUnitsAfterRound(league);

        // 순위 계산 및 저장
        CalculateRanking();
        RefreshCurrentMatchInfo();
        saveManager.SaveLeague(league);

        Debug.Log(" 라운드 결과 처리 완료");
        
        if (IsLeagueFinished())
        {
            Debug.Log(" 리그 종료!");

            if (IsPlayerChampion())
            {
                Debug.Log(" 우승! 다음 리그로 이동");
                StartNextLeague();
            }
            else
            {
                Debug.Log(" 우승 실패 - 리그 종료");
                // 여기서 엔딩 / 재도전 UI 띄우면 됨
            }
        }
    }
    
    public void StartNextLeague()
    {
        int nextTier = Mathf.Min(league.settings.tier + 1, 6);

        // 플레이어 팀 정보 보존 (InitializeLeague가 덮어쓰기 때문)
        int savedPlayerTeamId = league.settings.playerTeamId;
        string savedPlayerTeamName = league.settings.playerTeamName;

        // 적 팀 성장 (새 리그 생성 전에 처리)
        EnemyTeamService.GrowTeamsForNextLeague(league, nextTier);

        // 새 리그 생성
        league = settingManager.InitializeLeague();

        // 플레이어 팀 정보 복원
        league.settings.playerTeamId = savedPlayerTeamId;
        league.settings.playerTeamName = savedPlayerTeamName;

        league.settings.tier = nextTier;
        league.settings.tierName = GetTierName(nextTier);

        saveManager.SaveLeague(league);

        Debug.Log($"{nextTier}성 {league.settings.tierName} 시작!");

        // 필요하면 씬 이동
        // SceneManager.LoadScene("LeagueScene");
    }
    
    
    
    public bool IsLeagueFinished()
    {
        int myTeamId = league.settings.playerTeamId;
        Team myTeam = league.teams.Find(t => t.id == myTeamId);

        if (myTeam == null) return false;

        return myTeam.played >= league.settings.totalRounds;
    }

    public bool IsPlayerChampion()
    {
        int myTeamId = league.settings.playerTeamId;
        Team myTeam = league.teams.Find(t => t.id == myTeamId);

        if (myTeam == null) return false;

        return myTeam.rank == 1;
    }


    public Sprite GetTeamSprite(int teamId)
    {
        string path = $"TeamImages/team_{teamId}";
        Sprite sprite = Resources.Load<Sprite>(path);

        if (sprite == null)
        {
            Debug.LogWarning($"❌ 팀 스프라이트를 찾을 수 없습니다: {path}");
        }

        return sprite;
    }
    
    public string GetTierName(int tier)
    {
        switch (tier)
        {
            case 1: return "입문 리그";
            case 2: return "도전자 리그";
            case 3: return "검투사 리그";
            case 4: return "챔피언 리그";
            case 5: return "지배자 리그";
            case 6: return "불멸자 리그";
            default: return "알 수 없는 리그";
        }
    }
    
    public void RefreshCurrentMatchInfo()
    {
        if (league == null)
        {
            Debug.LogWarning(" league가 null입니다.");
            return;
        }

        int playerTeamId = league.settings.playerTeamId;

        Team playerTeam = league.teams.Find(t => t.id == playerTeamId);

        if (playerTeam == null)
        {
            Debug.LogWarning($" 플레이어 팀을 찾을 수 없습니다. id: {playerTeamId}");
            return;
        }

        int currentRoundNumber = playerTeam.played + 1;

        Round currentRound = league.schedule.Find(r => r.roundNumber == currentRoundNumber);

        if (currentRound == null)
        {
            Debug.LogWarning($" 현재 라운드를 찾을 수 없습니다. round: {currentRoundNumber}");
            return;
        }

        LeagueMatch currentMatch = currentRound.matches.Find(
            m => m.teamAId == playerTeamId || m.teamBId == playerTeamId
        );

        if (currentMatch == null)
        {
            Debug.LogWarning($" 현재 라운드에서 플레이어 경기를 찾을 수 없습니다. round: {currentRoundNumber}");
            return;
        }

        int enemyTeamId = currentMatch.teamAId == playerTeamId
            ? currentMatch.teamBId
            : currentMatch.teamAId;

        league.currentRound = currentRoundNumber;
        league.currentMatchId = currentMatch.matchId;
        league.currentEnemyTeamId = enemyTeamId;
        league.currentEnemy = EnemySaveManager.Instance?.GetTeam(enemyTeamId);

        league.currentMatchIndex = currentRound.matches.IndexOf(currentMatch);

        saveManager.SaveLeague(league);

        Debug.Log($" 현재 경기 갱신 완료 / Round: {league.currentRound}, Match: {league.currentMatchId}, Enemy: {league.currentEnemyTeamId}");
    }

}
