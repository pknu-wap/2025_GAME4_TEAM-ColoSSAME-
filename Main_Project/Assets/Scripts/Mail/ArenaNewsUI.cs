using UnityEngine;
using TMPro;

public class ArenaNewsUI : MonoBehaviour
{
    [Header("문구 데이터")]
    [SerializeField] private ArenaNewsTemplateSO templates;

    [Header("소식통 3칸")]
    public TextMeshProUGUI leagueNewsText;   // 리그 소식
    public TextMeshProUGUI rumorText;        // 경기장 소문
    public TextMeshProUGUI opinionText;      // 우리 팀 여론

    public int streakThreshold = 3;

    private ArenaNewsGenerator generator;

    private void Awake()
    {
        if (templates == null)
            Debug.LogError("[ArenaNewsUI] 뉴스 템플릿 SO(ArenaNewsTemplates)가 연결되지 않았습니다.");

        generator = new ArenaNewsGenerator(templates, streakThreshold);
    }

    private void OnEnable() => RefreshNews();

    public void RefreshNews()
    {
        var league = LeagueManager.Instance?.league;
        if (league == null)
        {
            Set(leagueNewsText, "리그 정보를 불러오는 중...");
            Set(rumorText, "");
            Set(opinionText, "");
            return;
        }

        var news = generator.Generate(league);
        Set(leagueNewsText, news.leagueNews);
        Set(rumorText, news.rumor);
        Set(opinionText, news.opinion);
    }

    private static void Set(TextMeshProUGUI target, string text)
    {
        if (target != null) target.text = text;
    }
}
