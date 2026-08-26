using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PredictionUI : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private int baseReward = 10;
    [SerializeField] private float maxOdds = 5f;

    [Header("다음 경기")]
    [SerializeField] private Image teamAImage;
    [SerializeField] private TMP_Text teamAName;
    [SerializeField] private TMP_Text teamARank;
    [SerializeField] private Image teamBImage;
    [SerializeField] private TMP_Text teamBName;
    [SerializeField] private TMP_Text teamBRank;

    [Header("예측 배당")]
    [SerializeField] private TMP_Text teamAOddsName;   
    [SerializeField] private TMP_Text teamBOddsName;
    [SerializeField] private TMP_Text teamAOdds;
    [SerializeField] private TMP_Text teamBOdds;

    [Header("승부 예측 버튼")]
    [SerializeField] private TMP_Text buttonAText;     
    [SerializeField] private TMP_Text buttonBText;
    [SerializeField] private Button buttonA;   
    [SerializeField] private Button buttonB;  

    [Header("상태/결과 (선택)")]
    [SerializeField] private TMP_Text statusText;

    private PredictionManager manager;
    private Prediction current;

    private void Awake() => manager = new PredictionManager(baseReward, maxOdds);

    private void OnEnable() => Refresh();

    public void Refresh()
    {
        var league = LeagueManager.Instance?.league;
        if (league == null) return;

        string resultMsg = manager.Resolve(league);      // 지난 예측 정산
        var preds = manager.GetOrCreate(league);          // 이번 라운드 경기
        if (preds.Count == 0) return;

        current = preds[0];                              
        var teamA = league.teams.Find(t => t.id == current.teamAId);
        var teamB = league.teams.Find(t => t.id == current.teamBId);

        // 팀 정보
        teamAImage.sprite = LeagueManager.Instance.GetTeamSprite(teamA.id);
        teamAName.text = teamA.name;
        teamARank.text = $"{teamA.rank}등";
        teamBImage.sprite = LeagueManager.Instance.GetTeamSprite(teamB.id);
        teamBName.text = teamB.name;
        teamBRank.text = $"{teamB.rank}등";

        // 배당
        var (oddsA, oddsB) = manager.GetOdds(current);
        teamAOddsName.text = teamA.name;
        teamAOdds.text = oddsA.ToString("0.00");
        teamBOddsName.text = teamB.name;
        teamBOdds.text = oddsB.ToString("0.00");

        // 버튼 연결 
        buttonA.onClick.RemoveAllListeners();
        buttonB.onClick.RemoveAllListeners();
        buttonAText.text = $"{teamA.name} 승";
        buttonBText.text = $"{teamB.name} 승";
        buttonA.onClick.AddListener(() => OnPredict(current.teamAId));
        buttonB.onClick.AddListener(() => OnPredict(current.teamBId));

        UpdateState(league, resultMsg);
    }

    private void OnPredict(int teamId)
    {
        manager.Predict(LeagueManager.Instance.league, current.matchId, teamId);
        Refresh();
    }

    private void UpdateState(League league, string resultMsg)
    {
        bool predicted = current.pickedTeamId != 0;
        buttonA.interactable = !predicted;
        buttonB.interactable = !predicted;

        if (statusText == null) return;

        if (resultMsg != null)
            statusText.text = resultMsg;                          // 방금 정산 결과
        else if (!predicted)
            statusText.text = "예측할 팀을 선택하세요";
        else
        {
            var picked = league.teams.Find(t => t.id == current.pickedTeamId);
            statusText.text = $"예측: {picked.name} 승 (적중 시 +{manager.RewardStored(current)})";
        }
    }
}