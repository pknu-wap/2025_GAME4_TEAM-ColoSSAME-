using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainSceneTeamDisplay : MonoBehaviour
{
    public TournamentSaveManager saveManager;

    public TextMeshProUGUI roundText;

    [Header("내 팀")]
    public Image myTeamImage;
    public TextMeshProUGUI myTeamText;

    [Header("상대 팀")]
    public Image enemyTeamImage;
    public TextMeshProUGUI enemyTeamText;

    [Header("버튼 및 종료 UI")]
    public GameObject matchEnterButton;
    public GameObject gameEndPanel;

    private const string myTeamKey = "Team01"; // 고정

    void OnEnable()
    {
        ShowCurrentMatch();
    }

    public void ShowCurrentMatch()
    {
        TournamentData data = saveManager.LoadTournament();
        Debug.Log("✅ ShowCurrentMatch 실행됨");

        // 1. 우승했는지 먼저 확인
        if (data.finalMatch != null && data.finalMatch.winnerKey == myTeamKey)
        {
            roundText.text = "우승";
            ApplyEliminationUI(true);
            SetTeamDisplayVisible(false);
            return;
        }

        // 2. 현재 진행 중인 경기 찾기
        Match currentMatch = FindCurrentMatchWithMyTeam(data);
        if (currentMatch != null)
        {
            string enemyKey = (currentMatch.player1Key == myTeamKey) ? currentMatch.player2Key : currentMatch.player1Key;

            myTeamImage.sprite = LoadTeamSprite(myTeamKey);
            myTeamText.text = GetTeamDisplayName(myTeamKey);

            enemyTeamImage.sprite = LoadTeamSprite(enemyKey);
            enemyTeamText.text = GetTeamDisplayName(enemyKey);

            ApplyEliminationUI(false);
            SetTeamDisplayVisible(true);
            return;
        }

        // 3. 탈락 처리
        roundText.text = "토너먼트 탈락";
        ApplyEliminationUI(true);
        SetTeamDisplayVisible(false);
    }

    private Match FindCurrentMatchWithMyTeam(TournamentData data)
    {
        foreach (var match in data.quarterFinals)
            if ((match.player1Key == myTeamKey || match.player2Key == myTeamKey) &&
                string.IsNullOrEmpty(match.winnerKey))
            {
                roundText.text = "8강";
                return match;
            }

        foreach (var match in data.semiFinals)
            if ((match.player1Key == myTeamKey || match.player2Key == myTeamKey) &&
                string.IsNullOrEmpty(match.winnerKey))
            {
                roundText.text = "4강";
                return match;
            }

        if (data.finalMatch != null &&
            (data.finalMatch.player1Key == myTeamKey || data.finalMatch.player2Key == myTeamKey) &&
            string.IsNullOrEmpty(data.finalMatch.winnerKey))
        {
            roundText.text = "결승";
            return data.finalMatch;
        }

        return null;
    }

    private void ApplyEliminationUI(bool eliminatedOrEnded)
    {
        matchEnterButton.SetActive(!eliminatedOrEnded);
        gameEndPanel.SetActive(eliminatedOrEnded);
    }

    private void SetTeamDisplayVisible(bool visible)
    {
        myTeamImage.gameObject.SetActive(visible);
        myTeamText.gameObject.SetActive(visible);
        enemyTeamImage.gameObject.SetActive(visible);
        enemyTeamText.gameObject.SetActive(visible);
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
