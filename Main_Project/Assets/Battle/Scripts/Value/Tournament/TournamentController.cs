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

        // 8강 (4경기)
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

        // 4강 자동 구성
        if (currentTournament.semiFinals.Count < 2 && AllMatchesFinished(currentTournament.quarterFinals))
        {
            for (int i = 0; i < 2; i++)
            {
                currentTournament.semiFinals.Add(new Match
                {
                    player1Key = currentTournament.quarterFinals[i * 2].winnerKey,
                    player2Key = currentTournament.quarterFinals[i * 2 + 1].winnerKey
                });
            }
            Debug.Log("🎯 4강 대진표 생성 완료");
        }

        saveManager.SaveTournament(currentTournament);
    }

    public void SetSemiFinalWinner(int matchIndex, string winnerKey)
    {
        currentTournament.semiFinals[matchIndex].winnerKey = winnerKey;
        Debug.Log($"✅ 4강 {matchIndex + 1}경기 승자: {winnerKey}");

        // 결승 구성
        if (currentTournament.finalMatch == null && AllMatchesFinished(currentTournament.semiFinals))
        {
            currentTournament.finalMatch = new Match
            {
                player1Key = currentTournament.semiFinals[0].winnerKey,
                player2Key = currentTournament.semiFinals[1].winnerKey
            };
            Debug.Log("🎯 결승 대진표 생성 완료");
        }

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

    // ✅ 숫자 키 → TeamXX 형식 보정 (오직 StartTournament()에서만 사용)
    private string FormatKey(string key)
    {
        if (key.StartsWith("Team"))
            return key;
        return $"Team{key.PadLeft(2, '0')}";
    }
}
