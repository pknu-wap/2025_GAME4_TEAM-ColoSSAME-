using UnityEngine;
using TMPro;

public class ArenaNewsUI : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI[] newsTexts = new TextMeshProUGUI[5];
    public int streakThreshold = 3;

    private ArenaNewsGenerator generator;

    private void Awake() => generator = new ArenaNewsGenerator(streakThreshold);
    private void OnEnable() => RefreshNews();

    public void RefreshNews()
    {
        if (LeagueManager.Instance == null || LeagueManager.Instance.league == null)
        {
            if (newsTexts.Length > 0 && newsTexts[0] != null)
                newsTexts[0].text = "리그 정보를 불러오는 중...";
            return;
        }
        var news = generator.Generate(LeagueManager.Instance.league);
        for (int i = 0; i < newsTexts.Length; i++)
        {
            if (newsTexts[i] == null) continue;
            newsTexts[i].text = i < news.Count ? news[i] : "";
        }
    }
}