using UnityEngine;
using UnityEngine.UI;
using TMPro;  // ✅ TMP 네임스페이스 추가

public class TournamentUIController : MonoBehaviour
{ 
    void OnEnable()
    {
        RefreshUI();
    }
    
    [Header("토너먼트 매니저 연결")]
    public TournamentSaveManager saveManager;

    [Header("8강 UI 이미지")]
    public GameObject quarterFinalUI;
    public Image[] qfP1Images; // 0~3
    public Image[] qfP2Images;
    public TextMeshProUGUI[] qfP1Texts; // ✅ TMP
    public TextMeshProUGUI[] qfP2Texts;

    [Header("4강 UI 이미지")]
    public GameObject semiFinalUI;
    public Image[] sfP1Images; // 0~1
    public Image[] sfP2Images;
    public TextMeshProUGUI[] sfP1Texts; // ✅ TMP
    public TextMeshProUGUI[] sfP2Texts;

    [Header("결승 UI 이미지")]
    public GameObject finalUI;
    public Image finalP1Image;
    public Image finalP2Image;
    public TextMeshProUGUI finalP1Text; // ✅ TMP
    public TextMeshProUGUI finalP2Text;

    public void RefreshUI()
    {
        TournamentData data = saveManager.LoadTournament();

        // 모든 UI 그룹 비활성화
        quarterFinalUI.SetActive(false);
        semiFinalUI.SetActive(false);
        finalUI.SetActive(false);

        // 1. 8강 표시
        for (int i = 0; i < data.quarterFinals.Count; i++)
        {
            if (string.IsNullOrEmpty(data.quarterFinals[i].winnerKey))
            {
                quarterFinalUI.SetActive(true);
                for (int j = 0; j < data.quarterFinals.Count; j++)
                {
                    var m = data.quarterFinals[j];
                    qfP1Images[j].sprite = LoadTeamSprite(m.player1Key);
                    qfP2Images[j].sprite = LoadTeamSprite(m.player2Key);

                    qfP1Texts[j].text = GetTeamDisplayName(m.player1Key); // ✅ j로 수정
                    qfP2Texts[j].text = GetTeamDisplayName(m.player2Key);
                }
                return;
            }
        }

        // 2. 4강 표시
        for (int i = 0; i < data.semiFinals.Count; i++)
        {
            if (string.IsNullOrEmpty(data.semiFinals[i].winnerKey))
            {
                semiFinalUI.SetActive(true);
                for (int j = 0; j < data.semiFinals.Count; j++)
                {
                    var m = data.semiFinals[j];
                    sfP1Images[j].sprite = LoadTeamSprite(m.player1Key);
                    sfP2Images[j].sprite = LoadTeamSprite(m.player2Key);

                    sfP1Texts[j].text = GetTeamDisplayName(m.player1Key);
                    sfP2Texts[j].text = GetTeamDisplayName(m.player2Key);
                }
                return;
            }
        }

        // 3. 결승 표시
        if (data.finalMatch != null && string.IsNullOrEmpty(data.finalMatch.winnerKey))
        {
            finalUI.SetActive(true);
            finalP1Image.sprite = LoadTeamSprite(data.finalMatch.player1Key);
            finalP2Image.sprite = LoadTeamSprite(data.finalMatch.player2Key);

            finalP1Text.text = GetTeamDisplayName(data.finalMatch.player1Key);
            finalP2Text.text = GetTeamDisplayName(data.finalMatch.player2Key);
            return;
        }

        // 4. 모든 라운드 완료됨
        Debug.Log("🏁 토너먼트가 종료되었습니다.");
    }

    private Sprite LoadTeamSprite(string key)
    {
        if (string.IsNullOrEmpty(key)) return null;

        if (!key.StartsWith("Team"))
            key = $"Team{key.PadLeft(2, '0')}";

        string path = $"TeamImages/{key}";
        Sprite sprite = Resources.Load<Sprite>(path);

        if (sprite == null)
            Debug.LogError($"❌ [스프라이트 로드 실패] key = '{key}', path = '{path}'");

        return sprite;
    }

    private string GetTeamDisplayName(string key)
    {
        if (string.IsNullOrEmpty(key)) return "-";

        if (!key.StartsWith("Team"))
            key = $"Team{key.PadLeft(2, '0')}";

        return $"팀 {key.Substring(4)}";
    }
}
