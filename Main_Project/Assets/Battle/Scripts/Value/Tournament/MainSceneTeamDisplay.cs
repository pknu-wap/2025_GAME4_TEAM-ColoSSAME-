using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainSceneTeamDisplay : MonoBehaviour
{
    public TournamentSaveManager saveManager;

    [Header("내 팀")]
    public Image myTeamImage;
    public TextMeshProUGUI myTeamText;

    [Header("상대 팀")]
    public Image enemyTeamImage;
    public TextMeshProUGUI enemyTeamText;

    private const string myTeamKey = "Team01"; // 고정

    void OnEnable()
    {
        ShowCurrentMatch();
    }

    public void ShowCurrentMatch()
    {
        TournamentData data = saveManager.LoadTournament();
        Debug.Log("✅ ShowCurrentMatch 실행됨");
    
        var d = saveManager.LoadTournament();
        Debug.Log($"🎮 불러온 팀 수: QF={d.quarterFinals.Count}, SF={d.semiFinals.Count}");

        // 내 팀이 현재 출전한 경기 찾기
        Match currentMatch = FindCurrentMatchWithMyTeam(data);
        if (currentMatch == null)
        {
            Debug.LogWarning("📭 현재 Team01이 포함된 진행 중인 경기가 없습니다.");
            return;
        }

        string enemyKey = (currentMatch.player1Key == myTeamKey) ? currentMatch.player2Key : currentMatch.player1Key;

        // 이미지 & 이름 표시
        myTeamImage.sprite = LoadTeamSprite(myTeamKey);
        myTeamText.text = GetTeamDisplayName(myTeamKey);

        enemyTeamImage.sprite = LoadTeamSprite(enemyKey);
        enemyTeamText.text = GetTeamDisplayName(enemyKey);
    }

    private Match FindCurrentMatchWithMyTeam(TournamentData data)
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

    private Sprite LoadTeamSprite(string key)
    {
        if (string.IsNullOrEmpty(key)) return null;
        if (!key.StartsWith("Team"))
            key = $"Team{key.PadLeft(2, '0')}";
        return Resources.Load<Sprite>($"TeamImages/{key}");
    }

    private string GetTeamDisplayName(string key)
    {
        if (string.IsNullOrEmpty(key)) return "-";
        if (!key.StartsWith("Team"))
            key = $"Team{key.PadLeft(2, '0')}";
        return $"팀 {key.Substring(4)}";
    }
}
