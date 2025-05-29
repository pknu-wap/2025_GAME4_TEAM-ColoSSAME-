using System.Collections.Generic;
using UnityEngine;

public class TournamentController : MonoBehaviour
{
    public TournamentSaveManager saveManager;
    private TournamentData currentTournament;

    private void Awake()
    {
        currentTournament = saveManager.LoadTournament();
    }

    public void StartTournament(List<string> characterKeys)
    {
        currentTournament = new TournamentData();

        for (int i = 0; i < 4; i++)
        {
            currentTournament.quarterFinals.Add(new Match
            {
                player1Key = FormatKey(characterKeys[i * 2]),
                player2Key = FormatKey(characterKeys[i * 2 + 1])
            });
        }

        saveManager.SaveTournament(currentTournament);
        Debug.Log("🎯 8강 대진표 생성 완료");
    }

    public void SetQuarterFinalWinner(int matchIndex, string winnerKey)
    {
        currentTournament.quarterFinals[matchIndex].winnerKey = winnerKey;
        Debug.Log($"✅ 8강 {matchIndex + 1}경기 승자: {winnerKey}");

        TryAdvanceToNextRounds();
        saveManager.SaveTournament(currentTournament);
    }

    public void SetSemiFinalWinner(int matchIndex, string winnerKey)
    {
        currentTournament.semiFinals[matchIndex].winnerKey = winnerKey;
        Debug.Log($"✅ 4강 {matchIndex + 1}경기 승자: {winnerKey}");

        TryAdvanceToNextRounds();
        saveManager.SaveTournament(currentTournament);
    }

    public void SetFinalWinner(string winnerKey)
    {
        if (currentTournament.finalMatch != null)
        {
            currentTournament.finalMatch.winnerKey = winnerKey;
            Debug.Log($"🏆 결승전 승자: {winnerKey}");
            saveManager.SaveTournament(currentTournament);
        }
    }

    private bool AllMatchesFinished(List<Match> matches)
    {
        foreach (var match in matches)
        {
            if (string.IsNullOrEmpty(match.winnerKey))
                return false;
        }

        return true;
    }

    public TournamentData GetTournamentData()
    {
        return currentTournament;
    }

    private string FormatKey(string key)
    {
        if (key.StartsWith("Team"))
            return key;
        return $"Team{key.PadLeft(2, '0')}";
    }

    // ✅ 추가된 함수: 나머지 매치 자동 결과 처리
    public void AutoResolveRemainingMatches()
    {
        void Resolve(List<Match> matches)
        {
            foreach (var match in matches)
            {
                if (string.IsNullOrEmpty(match.winnerKey))
                {
                    string winner = Random.value > 0.5f ? match.player1Key : match.player2Key;
                    match.winnerKey = winner;
                    Debug.Log($"🎲 자동 결과 처리: {match.player1Key} vs {match.player2Key} → {winner}");
                }
            }
        }

        if (currentTournament.quarterFinals.Exists(m => string.IsNullOrEmpty(m.winnerKey)))
            Resolve(currentTournament.quarterFinals);
        else if (currentTournament.semiFinals.Exists(m => string.IsNullOrEmpty(m.winnerKey)))
            Resolve(currentTournament.semiFinals);
        else if (currentTournament.finalMatch != null && string.IsNullOrEmpty(currentTournament.finalMatch.winnerKey))
        {
            string winner = Random.value > 0.5f
                ? currentTournament.finalMatch.player1Key
                : currentTournament.finalMatch.player2Key;
            currentTournament.finalMatch.winnerKey = winner;
            Debug.Log($"🎲 자동 결승 결과 처리: {winner}");
        }

        TryAdvanceToNextRounds();

        saveManager.SaveTournament(currentTournament);
    }
    
    public void TryAdvanceToNextRounds()
    {
        // 4강 자동 구성
        if (currentTournament.semiFinals.Count < 2 &&
            AllMatchesFinished(currentTournament.quarterFinals))
        {
            var usedTeams = new HashSet<string>();

            for (int i = 0; i < 2; i++)
            {
                string p1 = currentTournament.quarterFinals[i * 2].winnerKey;
                string p2 = currentTournament.quarterFinals[i * 2 + 1].winnerKey;

                // 중복 방지
                if (usedTeams.Contains(p1) || usedTeams.Contains(p2))
                {
                    Debug.LogError($"❌ 중복된 팀이 발견됨: {p1}, {p2}");
                    continue;
                }

                usedTeams.Add(p1);
                usedTeams.Add(p2);

                currentTournament.semiFinals.Add(new Match
                {
                    player1Key = p1,
                    player2Key = p2
                });
            }

            Debug.Log("🎯 4강 대진표 생성 완료");
        }

        // 결승 자동 구성
        if (currentTournament.finalMatch == null &&
            AllMatchesFinished(currentTournament.semiFinals))
        {
            string f1 = currentTournament.semiFinals[0].winnerKey;
            string f2 = currentTournament.semiFinals[1].winnerKey;

            if (f1 != null && f2 != null && f1 != f2)
            {
                currentTournament.finalMatch = new Match
                {
                    player1Key = f1,
                    player2Key = f2
                };
                Debug.Log("🎯 결승 대진표 생성 완료");
            }
            else
            {
                Debug.LogWarning("⚠️ 결승전 구성 실패: 중복 또는 null winner");
            }
        }
    }

    
    public void SaveTournament()
    {
        saveManager.SaveTournament(currentTournament);
    }

}

