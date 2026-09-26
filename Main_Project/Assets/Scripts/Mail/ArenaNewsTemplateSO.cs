using System;
using System.Collections.Generic;
using UnityEngine;

// 우리 팀 여론 상황 분류
public enum OpinionMood { Preseason, Hype, Crisis, Top, Win, Bottom, Lose, Draw }

// 소식통 문구/점수 데이터
[CreateAssetMenu(fileName = "ArenaNewsTemplates", menuName = "Game/Arena News Templates")]
public class ArenaNewsTemplateSO : ScriptableObject
{
    [Serializable]
    public class LeagueEntry
    {
        public NewsEventType type;
        public int score;
        [TextArea] public List<string> templates = new();
    }

    [Serializable]
    public class RumorEntry
    {
        public UnitNewsType type;
        public int score;
        [TextArea] public List<string> templates = new();
    }

    [Serializable]
    public class OpinionEntry
    {
        public OpinionMood mood;
        [TextArea] public List<string> templates = new();
    }

    [Header("1. 리그 소식   {winner} {loser} {winnerEul} {loserEul} {winnerWa} {loserWa} {streak}")]
    public List<LeagueEntry> leagueNews = new();
    [Tooltip("리그 시작 직후 / 사건이 없을 때 쓰는 문구")]
    public List<string> openingLines = new();
    [Tooltip("직전 라운드와 같은 종류의 사건에 주는 감점")]
    public int repeatPenalty = 30;

    [Header("2. 경기장 소문   {team} {unit} {unitEul} {detail}")]
    public List<RumorEntry> rumors = new();
    [Tooltip("소문 재료가 없을 때 쓰는 문구")]
    public List<string> noRumorLines = new();
    [Tooltip("다음 상대팀 소문에 주는 가산점 (승부예측 참고용)")]
    public int nextOpponentBonus = 30;

    [Header("3. 우리 팀 여론   {team} {rank} {streak}")]
    public List<OpinionEntry> opinions = new();


    public bool TryGetLeague(NewsEventType type, out int score, out List<string> templates)
    {
        var e = leagueNews.Find(x => x.type == type);
        score = e?.score ?? 0;
        templates = e?.templates;
        return templates != null && templates.Count > 0;
    }

    public bool TryGetRumor(UnitNewsType type, out int score, out List<string> templates)
    {
        var e = rumors.Find(x => x.type == type);
        score = e?.score ?? 0;
        templates = e?.templates;
        return templates != null && templates.Count > 0;
    }

    public List<string> GetOpinion(OpinionMood mood)
        => opinions.Find(x => x.mood == mood)?.templates;


    private void Reset() => FillDefaults();

    [ContextMenu("기본 문구로 채우기")]
    private void FillDefaults()
    {
        static LeagueEntry L(NewsEventType t, int s, params string[] tpl) => new() { type = t, score = s, templates = new(tpl) };
        static RumorEntry R(UnitNewsType t, int s, params string[] tpl) => new() { type = t, score = s, templates = new(tpl) };
        static OpinionEntry O(OpinionMood m, params string[] tpl) => new() { mood = m, templates = new(tpl) };

        leagueNews = new List<LeagueEntry>
        {
            L(NewsEventType.Draw, 30,
                "{winnerWa} {loser}, 무승부로 승점 1점씩 나누다",
                "{winner} vs {loser}, 팽팽한 접전 끝에 무승부"),
            L(NewsEventType.WinStreakMax, 100,
                "{winner}, 파죽의 {streak}연승 — 막을 자가 없다",
                "{winner}, {streak}연승 달성으로 리그를 지배하다"),
            L(NewsEventType.TopRank, 100,
                "{winner}, {loserEul} 꺾고 단독 선두 등극",
                "{winner}, 승리와 함께 리그 정상에 서다"),
            L(NewsEventType.Upset, 100,
                "최하위 {winner}, 선두 {loserEul} 꺾는 이변",
                "{winner}, {loserEul} 상대로 대이변 연출"),
            L(NewsEventType.WinStreak5, 80,
                "{winner}, 5연승으로 거침없이 질주하다",
                "{winner}, 파죽의 5연승 달성",
                "{winner}의 상승세가 멈추지 않는다 — 5연승"),
            L(NewsEventType.LoseStreak5, 80,
                "{loser}, 5연패 수렁에 빠지다",
                "{loser}, 5연패 — 반전의 계기가 필요하다",
                "{loser}의 침체가 깊어지다 — 5연패"),
            L(NewsEventType.BreakLoseStreak, 80,
                "{winner}, {streak}연패를 끊고 반격에 나서다",
                "{winner}, 연패 탈출 성공 — 분위기 반전 노린다",
                "{winner}, 마침내 연패의 사슬을 끊다"),
            L(NewsEventType.EndWinStreak, 80,
                "{loser}의 {streak}연승 행진이 막을 내리다",
                "{loser}, 연승 마감 — {winner}에 발목 잡히다",
                "{winner}, {loser}의 연승을 끊어내다"),
            L(NewsEventType.WinStreakN, 60,
                "{winner}, {streak}연승으로 기세를 올리다",
                "{winner}, 또 한 번 승리하며 {streak}연승 달성",
                "{winner}, 연승 행진을 이어가다"),
            L(NewsEventType.LoseStreakN, 60,
                "{loser}, {streak}연패 — 위기에 빠지다",
                "{loser}, 연패 행진이 멈추지 않는다",
                "{loser}, {streak}연패 수렁에서 헤어나오지 못하다"),
            L(NewsEventType.FirstWin, 60,
                "{winner}, 시즌 첫 승 신고",
                "{winner}, 첫 승리를 거두며 웃음을 되찾다",
                "{winner}, 마침내 시즌 첫 승을 따내다"),
            L(NewsEventType.FirstLose, 60,
                "{loser}, 시즌 첫 패배를 당하다",
                "{loser}의 무패 행진, {winner}에 의해 막히다",
                "{loser}, 첫 패 — 무패 기록이 무너지다"),
            L(NewsEventType.PlainWin, 40,
                "{winner}, {loserEul} 꺾고 승점을 쌓다",
                "{winner}, {loserEul} 상대로 값진 승리"),
        };

        openingLines = new List<string>
        {
            "콜로세움 리그, 드디어 막을 올리다",
            "10개 가문이 왕좌를 두고 격돌한다",
            "카이루스 가문, 이번 시즌 우승 후보로 주목받다",
            "이그니스 가문의 새 전략, 이번 시즌 통할 것인가",
            "녹스 가문, 조용한 강자로 떠오르다",
            "플로라 가문, 치유의 전술로 이변을 노린다",
            "루멘 가문의 철벽 수비, 이번 시즌도 건재할까",
            "아스트라 가문, 예언대로라면 우승은 따놓은 당상",
            "테라 가문의 수호자들, 투기장에 입성하다",
            "모르스 가문, 망령의 힘으로 리그를 뒤흔든다",
        };
        repeatPenalty = 30;

        rumors = new List<RumorEntry>
        {
            R(UnitNewsType.NewSkill, 80,
                "{team}의 {unit}, 전에 없던 기술을 쓴다는 말이 돈다",
                "훈련장에서 {unit}의 낯선 몸놀림을 봤다는 이야기",
                "\"{unit}, 새 수를 감췄다더군\" — 주점에서 흘러나온 말"),
            R(UnitNewsType.Recruited, 70,
                "{team}, 새 얼굴 {unitEul} 막사에 들였다",
                "{team}의 새 식구 {unit} — 아직 실력을 본 사람이 없다",
                "낯선 이름 {unit}, {team} 막사를 드나든다는 소문"),
            R(UnitNewsType.LevelJump, 50,
                "{team}의 {unit}, 해가 진 뒤에도 훈련장에 남아 있다는 말",
                "{unit}의 몸놀림이 전보다 매서워졌다는 이야기가 {team} 주변에 돈다",
                "\"{unit}, 요즘 사람이 달라졌더군\" — 막사 근처에서 들은 말"),
        };

        noRumorLines = new List<string>
        {
            "투기장은 조용하다 — 딱히 들리는 말이 없다",
            "요 며칠 뒷말이 없다. 다들 다음 경기만 기다리는 분위기",
            "소문이 마른 날이다 — 폭풍 전의 고요일까",
        };
        nextOpponentBonus = 30;

        opinions = new List<OpinionEntry>
        {
            O(OpinionMood.Preseason,
                "개막을 앞두고 {team}의 이름이 시장 곳곳에서 오르내린다",
                "\"{team}? 올해는 좀 다를 거라던데\" — 주점에서 들려온 말",
                "관중들은 아직 {team}을 반신반의하는 눈치다"),
            O(OpinionMood.Hype,
                "{streak}연승 소식에 관중석이 {team}의 이름을 연호한다",
                "\"{team}에 걸면 딴다\" — 도박꾼들 사이에 도는 말",
                "노련한 관중들은 신중하다 — \"{streak}연승이라도 방심하면 끝이지\""),
            O(OpinionMood.Crisis,
                "{streak}연패에 관중석에서 야유가 쏟아졌다",
                "\"{team}에 돈 건 놈이 바보지\" — 도박꾼들의 비아냥",
                "{team}의 후원자들이 슬슬 등을 돌린다는 소문"),
            O(OpinionMood.Top,
                "관중들이 {team}의 이름을 외친다 — 정상에 선 자의 무게",
                "\"정상은 오르는 것보다 지키는 게 어렵다\" — 한 노관중의 말",
                "{team}의 후원자들, 요즘 표정이 밝다"),
            O(OpinionMood.Win,
                "관중들은 오늘 {team}의 경기에 만족한 모습이다",
                "\"{rank}위? 더 올라갈 수 있지\" — 시민들 사이의 기대",
                "이기긴 했으나 관중석의 반응은 미지근했다"),
            O(OpinionMood.Bottom,
                "최하위 {team}, 관중석마저 조금씩 비어간다",
                "\"바닥이니 올라갈 일만 남았지\" — 한 노인의 씁쓸한 위로",
                "{team}의 후원자들이 지원을 끊는다는 말이 돈다"),
            O(OpinionMood.Lose,
                "{team}의 패배에 관중들은 한동안 말을 잃었다",
                "\"한 번 졌다고 끝은 아니다\" — 시민들은 다음 경기를 기다린다",
                "주점에서는 벌써 {team}의 전술을 두고 설전이 벌어졌다"),
            O(OpinionMood.Draw,
                "무승부에 관중석이 술렁인다 — \"이길 경기였다\"는 반응",
                "\"승점 1점이라도 어디냐\" — {team} 지지자들의 위안",
                "무승부에 돈을 건 도박꾼들만 웃었다는 뒷말"),
        };
    }
}
