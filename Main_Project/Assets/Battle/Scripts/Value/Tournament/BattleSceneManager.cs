using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleSceneManager : MonoBehaviour
{
    public TournamentSaveManager saveManager;
    public TournamentController tournamentController;

    private string myTeamKey = "Team01";
    private string enemyTeamKey;
    private Match currentMatch;

    [Header("우리 팀 (좌측)")]
    public Image myTeamImage;
    public TextMeshProUGUI myTeamText;

    [Header("상대 팀 (우측)")]
    public Image enemyTeamImage;
    public TextMeshProUGUI enemyTeamText;

    [Header("결과창 - 우리 팀")]
    public Image resultMyTeamImage;
    public TextMeshProUGUI resultMyTeamText;
    public TextMeshProUGUI resultMyTeamResult;

    [Header("결과창 - 상대 팀")]
    public Image resultEnemyTeamImage;
    public TextMeshProUGUI resultEnemyTeamText;
    public TextMeshProUGUI resultEnemyTeamResult;

    [Header("결과창 오브젝트")]
    public GameObject resultPanel;

    void Start()
    {
        TournamentData data = saveManager.LoadTournament();
        currentMatch = FindMyCurrentMatch(data);

        if (currentMatch == null)
        {
            Debug.LogError("❌ 현재 매치 없음.");
            return;
        }

        enemyTeamKey = (currentMatch.player1Key == myTeamKey) ? currentMatch.player2Key : currentMatch.player1Key;

        myTeamImage.sprite = LoadTeamSprite(myTeamKey);
        myTeamText.text = GetTeamDisplayName(myTeamKey);

        enemyTeamImage.sprite = LoadTeamSprite(enemyTeamKey);
        enemyTeamText.text = GetTeamDisplayName(enemyTeamKey);

        resultPanel.SetActive(false); // 결과창 초기 숨김
    }

    public void OnWin()
    {
        Debug.Log("🧨🧨🧨 OnWin() 호출됨");
        ApplyResult(myTeamKey);
    }

    public void OnLose()
    {
        ApplyResult(enemyTeamKey);
    }

    void ApplyResult(string winnerKey)
{
    try
    {
        Debug.Log("🎯 ApplyResult 진입");
        Debug.Log($"currentMatch: {currentMatch.player1Key} vs {currentMatch.player2Key}");

        TournamentData data = saveManager.LoadTournament();

        Debug.Log($"💬 winnerKey: {(winnerKey == null ? "NULL" : $"[{winnerKey}]")}");
        Debug.Log($"💬 myTeamKey: {(myTeamKey == null ? "NULL" : $"[{myTeamKey}]")}");
        Debug.Log($"💬 winnerKey.Equals(myTeamKey): {winnerKey == myTeamKey}");

        // Final Match 확인
        if (data.finalMatch != null && MatchesEqual(data.finalMatch, currentMatch))
        {
            Debug.Log("🏆 결승 승자 설정");
            tournamentController?.SetFinalWinner(winnerKey);
        }

        Debug.Log("1");

        // Semi Finals 확인
        for (int i = 0; i < data.semiFinals.Count; i++)
        {
            if (MatchesEqual(data.semiFinals[i], currentMatch))
            {
                Debug.Log("🔥 4강 승자 설정");
                tournamentController?.SetSemiFinalWinner(i, winnerKey);
                break;
            }
        }

        Debug.Log("2");

        // Quarter Finals 확인
        for (int i = 0; i < data.quarterFinals.Count; i++)
        {
            if (MatchesEqual(data.quarterFinals[i], currentMatch))
            {
                Debug.Log("🧊 8강 승자 설정 진입 전");
                if (tournamentController != null)
                {
                    tournamentController.SetQuarterFinalWinner(i, winnerKey);
                    Debug.Log($"8강 {i}경기 승자: {winnerKey}");
                }
                else
                {
                    Debug.LogError("❌ tournamentController가 null입니다. 인스펙터에서 연결 확인 필요.");
                }
                break;
            }
        }

        Debug.Log("3");

        resultMyTeamImage.sprite = myTeamImage.sprite;
        resultMyTeamText.text = myTeamText.text;
        Debug.Log("4");

        resultEnemyTeamImage.sprite = enemyTeamImage.sprite;
        resultEnemyTeamText.text = enemyTeamText.text;
        Debug.Log("5");

        Debug.Log($"💡 winnerKey = {winnerKey}, myTeamKey = {myTeamKey}");
        Debug.Log("6");
        Debug.Log($"== 비교 결과: {winnerKey == myTeamKey}");

        if (winnerKey == myTeamKey)
        {
            Debug.Log("🟩 조건문 진입: 우리 팀 승리");

            resultMyTeamResult.text = "승";
            resultEnemyTeamResult.text = "패";

            resultEnemyTeamImage.color = new Color(1, 1, 1, 0.3f);
            resultMyTeamImage.color = Color.white;

            tournamentController?.AutoResolveRemainingMatches();
            Debug.Log("✅ 우리 팀 승리 완료");
        }
        else
        {
            resultMyTeamResult.text = "패";
            resultEnemyTeamResult.text = "승";

            resultMyTeamImage.color = new Color(1, 1, 1, 0.3f);
            resultEnemyTeamImage.color = Color.white;

            Debug.Log("💀 우리 팀 패배 - 게임 오버");
        }

        resultPanel.SetActive(true);
        tournamentController?.SaveTournament();
    }
    catch (System.Exception e)
    {
        Debug.LogError($"💥 ApplyResult 예외 발생: {e.Message}\n{e.StackTrace}");
    }
}


    Match FindMyCurrentMatch(TournamentData data)
    {
        foreach (var match in data.quarterFinals)
            if ((match.player1Key == myTeamKey || match.player2Key == myTeamKey) && string.IsNullOrEmpty(match.winnerKey))
                return match;

        foreach (var match in data.semiFinals)
            if ((match.player1Key == myTeamKey || match.player2Key == myTeamKey) && string.IsNullOrEmpty(match.winnerKey))
                return match;

        if (data.finalMatch != null &&
            (data.finalMatch.player1Key == myTeamKey || data.finalMatch.player2Key == myTeamKey) &&
            string.IsNullOrEmpty(data.finalMatch.winnerKey))
            return data.finalMatch;

        return null;
    }

    Sprite LoadTeamSprite(string key)
    {
        if (!key.StartsWith("Team"))
            key = $"Team{key.PadLeft(2, '0')}";
        return Resources.Load<Sprite>($"TeamImages/{key}");
    }

    string GetTeamDisplayName(string key)
    {
        if (!key.StartsWith("Team"))
            key = $"Team{key.PadLeft(2, '0')}";
        return $"팀 {key.Substring(4)}";
    }

    bool MatchesEqual(Match a, Match b)
    {
        return (a.player1Key == b.player1Key && a.player2Key == b.player2Key) ||
               (a.player1Key == b.player2Key && a.player2Key == b.player1Key);
    }
}
