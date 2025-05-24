using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    public TournamentController tournamentController;

    // 캐릭터 키들을 받아서 8강 시작
    public void StartTournamentManually()
    {
        // 예시 캐릭터 키 8개 (실제 캐릭터 ID를 여기에 넣어야 함)
        List<string> characterKeys = new() { "1", "2", "3", "4", "5", "6", "7", "8" };
        tournamentController.StartTournament(characterKeys);
    }

    // 특정 경기 결과를 기록 (8강)
    public void RecordQuarterFinalResult(int matchIndex, string winnerKey)
    {
        tournamentController.SetQuarterFinalWinner(matchIndex, winnerKey);
    }

    // 특정 경기 결과를 기록 (4강)
    public void RecordSemiFinalResult(int matchIndex, string winnerKey)
    {
        tournamentController.SetSemiFinalWinner(matchIndex, winnerKey);
    }

    // 결승 결과 기록
    public void RecordFinalResult(string winnerKey)
    {
        tournamentController.SetFinalWinner(winnerKey);
    }

    // UI 등에서 현재 상태 조회
    public void PrintTournamentStatus()
    {
        var data = tournamentController.GetTournamentData();

        Debug.Log("▶ 8강:");
        for (int i = 0; i < data.quarterFinals.Count; i++)
        {
            var m = data.quarterFinals[i];
            Debug.Log($"  {i+1}경기: {m.player1Key} vs {m.player2Key} → 승자: {m.winnerKey}");
        }

        Debug.Log("▶ 4강:");
        for (int i = 0; i < data.semiFinals.Count; i++)
        {
            var m = data.semiFinals[i];
            Debug.Log($"  {i+1}경기: {m.player1Key} vs {m.player2Key} → 승자: {m.winnerKey}");
        }

        if (data.finalMatch != null)
        {
            var m = data.finalMatch;
            Debug.Log($"▶ 결승: {m.player1Key} vs {m.player2Key} → 승자: {m.winnerKey}");
        }
    }
}