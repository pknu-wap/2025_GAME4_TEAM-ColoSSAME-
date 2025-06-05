using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TournamentUIController : MonoBehaviour
{
    [Header("기본 설정")]
    public string myTeamKey = "Team01";

    [Header("토너먼트 매니저 연결")]
    public TournamentSaveManager saveManager;

    [Header("라운드 텍스트")]
    public TextMeshProUGUI roundText;

    [Header("8강 UI")]
    public GameObject quarterFinalUI;
    public Image[] qfP1Images;
    public Image[] qfP2Images;
    public TextMeshProUGUI[] qfP1Texts;
    public TextMeshProUGUI[] qfP2Texts;

    [Header("4강 UI")]
    public GameObject semiFinalUI;
    public Image[] sfP1Images;
    public Image[] sfP2Images;
    public TextMeshProUGUI[] sfP1Texts;
    public TextMeshProUGUI[] sfP2Texts;

    [Header("결승 UI")]
    public GameObject finalUI;
    public Image finalP1Image;
    public Image finalP2Image;
    public TextMeshProUGUI finalP1Text;
    public TextMeshProUGUI finalP2Text;

    void OnEnable()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        TournamentData data = saveManager.LoadTournament();

        quarterFinalUI.SetActive(false);
        semiFinalUI.SetActive(false);
        finalUI.SetActive(false);

        // 8강
        for (int i = 0; i < data.quarterFinals.Count; i++)
        {
            var m = data.quarterFinals[i];
            if (string.IsNullOrEmpty(m.winnerKey))
            {
                quarterFinalUI.SetActive(true);
                for (int j = 0; j < data.quarterFinals.Count; j++)
                {
                    var match = data.quarterFinals[j];
                    qfP1Images[j].sprite = LoadTeamSprite(match.player1Key);
                    qfP2Images[j].sprite = LoadTeamSprite(match.player2Key);
                    qfP1Texts[j].text = GetTeamDisplayName(match.player1Key);
                    qfP2Texts[j].text = GetTeamDisplayName(match.player2Key);
                }

                roundText.text = IsMyTeamEliminated(data) ? "토너먼트 탈락" : "8강";
                return;
            }
        }

        // 4강
        for (int i = 0; i < data.semiFinals.Count; i++)
        {
            var m = data.semiFinals[i];
            if (string.IsNullOrEmpty(m.winnerKey))
            {
                semiFinalUI.SetActive(true);
                for (int j = 0; j < data.semiFinals.Count; j++)
                {
                    var match = data.semiFinals[j];
                    sfP1Images[j].sprite = LoadTeamSprite(match.player1Key);
                    sfP2Images[j].sprite = LoadTeamSprite(match.player2Key);
                    sfP1Texts[j].text = GetTeamDisplayName(match.player1Key);
                    sfP2Texts[j].text = GetTeamDisplayName(match.player2Key);
                }

                roundText.text = IsMyTeamEliminated(data) ? "토너먼트 탈락" : "4강";
                return;
            }
        }

        // 결승
        if (data.finalMatch != null)
        {
            var m = data.finalMatch;

            finalUI.SetActive(true);
            finalP1Image.sprite = LoadTeamSprite(m.player1Key);
            finalP2Image.sprite = LoadTeamSprite(m.player2Key);
            finalP1Text.text = GetTeamDisplayName(m.player1Key);
            finalP2Text.text = GetTeamDisplayName(m.player2Key);

            if (!string.IsNullOrEmpty(m.winnerKey))
            {
                roundText.text = (m.winnerKey == myTeamKey) ? "우승" : "토너먼트 탈락";
            }
            else
            {
                roundText.text = IsMyTeamEliminated(data) ? "토너먼트 탈락" : "결승";
            }

            return;
        }

        // 아무 라운드도 진행되지 않는 경우 (보통 없음)
        roundText.text = "토너먼트 종료";
    }

    private bool IsMyTeamEliminated(TournamentData data)
    {
        if (data.finalMatch != null && data.finalMatch.winnerKey == myTeamKey)
            return false;

        bool aliveInQuarter = data.quarterFinals.Exists(m =>
            (m.player1Key == myTeamKey || m.player2Key == myTeamKey) &&
            (string.IsNullOrEmpty(m.winnerKey) || m.winnerKey == myTeamKey));

        bool aliveInSemi = data.semiFinals.Exists(m =>
            (m.player1Key == myTeamKey || m.player2Key == myTeamKey) &&
            (string.IsNullOrEmpty(m.winnerKey) || m.winnerKey == myTeamKey));

        var f = data.finalMatch;
        bool aliveInFinal = f != null &&
            (f.player1Key == myTeamKey || f.player2Key == myTeamKey) &&
            (string.IsNullOrEmpty(f.winnerKey) || f.winnerKey == myTeamKey);

        return !(aliveInQuarter || aliveInSemi || aliveInFinal);
    }

    private Sprite LoadTeamSprite(string key)
    {
        if (string.IsNullOrEmpty(key)) return null;
        if (!key.StartsWith("Team")) key = $"Team{key.PadLeft(2, '0')}";
        return Resources.Load<Sprite>($"TeamImages/{key}");
    }

    private string GetTeamDisplayName(string key)
    {
        if (string.IsNullOrEmpty(key)) return "-";
        if (!key.StartsWith("Team")) key = $"Team{key.PadLeft(2, '0')}";
        return $"팀 {key.Substring(4)}";
    }
}
