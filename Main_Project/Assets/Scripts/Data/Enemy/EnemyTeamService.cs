using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class EnemyTeamService
{
    private const int StartEnemyUnitCount = 4;

    // 최초 생성
    public static void InitializeFromLeague(League league)
    {
        if (league == null) { Debug.LogError("[EnemyTeamService] league가 null입니다."); return; }
        if (league.teams == null) { Debug.LogError("[EnemyTeamService] league.teams가 null입니다."); return; }
        if (UnitDataManager.Instance == null) { Debug.LogError("[EnemyTeamService] UnitDataManager.Instance가 없습니다."); return; }
        if (!UnitDataManager.Instance.IsLoaded) { Debug.LogError("[EnemyTeamService] UnitDataManager 로드가 아직 완료되지 않았습니다."); return; }
        if (EnemySaveManager.Instance == null) { Debug.LogError("[EnemyTeamService] EnemySaveManager.Instance가 없습니다."); return; }

        foreach (Team leagueTeam in league.teams)
        {
            if (leagueTeam.id == league.settings.playerTeamId) continue;
            if (EnemySaveManager.Instance.HasTeam(leagueTeam.id)) continue;

            EnemyTeam enemyTeam = CreateEnemyTeamFromLeagueTeam(leagueTeam);
            if (enemyTeam == null) continue;

            EnemySaveManager.Instance.AddTeam(enemyTeam);
        }

        EnemySaveManager.Instance.Save();
        Debug.Log("[EnemyTeamService] 상대팀 초기화 완료");
    }

    private static EnemyTeam CreateEnemyTeamFromLeagueTeam(Team leagueTeam)
    {
        List<CharacterData> familyUnits = UnitDataManager.Instance.GetFamilyUnits(leagueTeam.fid);
        if (familyUnits == null || familyUnits.Count == 0)
        {
            Debug.LogError($"[EnemyTeamService] 가문 유닛을 찾을 수 없습니다. fid: {leagueTeam.fid}");
            return null;
        }

        EnemyTeam enemyTeam = new EnemyTeam(leagueTeam.id, leagueTeam.fid, leagueTeam.name);

        List<CharacterData> recruits = familyUnits
            .Where(character => character.Rarity == 1)
            .Take(StartEnemyUnitCount)
            .ToList();

        if (recruits.Count == 0)
            Debug.LogWarning($"[EnemyTeamService] 1성 훈련병이 없습니다. fid: {leagueTeam.fid}");

        foreach (CharacterData character in recruits)
        {
            Unit unit = new Unit(character.Unit_ID, character.Rarity, character.Unit_Name, character.Class);
            unit.level = 1;
            unit.exp = 0f;
            enemyTeam.units.Add(unit);
        }

        return enemyTeam;
    }

    // 라운드 종료 성장
    public static void GrowUnitsAfterRound(League league)
    {
        if (EnemySaveManager.Instance == null) return;

        int playerTeamId = league.settings.playerTeamId;
        int cap = league.settings.tier * 10;

        foreach (Team leagueTeam in league.teams)
        {
            if (leagueTeam.id == playerTeamId) continue;

            EnemyTeam team = EnemySaveManager.Instance.GetTeam(leagueTeam.id);
            if (team == null) continue;

            bool changed = false;
            foreach (Unit unit in team.units)
            {
                if (unit.level < cap)
                {
                    unit.level = Mathf.Min(unit.level + UnityEngine.Random.Range(0, 2), cap);
                    changed = true;
                }
            }

            if (changed) EnemySaveManager.Instance.SaveTeam(team);
        }
    }

    // 리그 승급 성장
    public static void GrowTeamsForNextLeague(League league, int nextTier)
    {
        if (EnemySaveManager.Instance == null || UnitDataManager.Instance == null) return;

        int playerTeamId = league.settings.playerTeamId;
        int resetLevel = (nextTier - 1) * 10;

        foreach (Team leagueTeam in league.teams)
        {
            if (leagueTeam.id == playerTeamId) continue;

            EnemyTeam team = EnemySaveManager.Instance.GetTeam(leagueTeam.id);
            if (team == null) continue;

            foreach (Unit unit in team.units)
            {
                unit.rarity = Mathf.Min(unit.rarity + 1, 5);
                unit.level = resetLevel;
                unit.exp = 0f;
            }

            AddNewLowestRarityUnit(team, leagueTeam.fid, resetLevel);

            team.growthStage = nextTier;
            EnemySaveManager.Instance.SaveTeam(team);
        }

        Debug.Log($"적 팀 성장 완료 (tier {nextTier}, resetLevel {resetLevel})");
    }

    private static void AddNewLowestRarityUnit(EnemyTeam team, string fid, int startLevel)
    {
        var familyUnits = UnitDataManager.Instance.GetFamilyUnits(fid);
        if (familyUnits == null || familyUnits.Count == 0) return;

        var existingIds = new HashSet<string>(team.units.Select(u => u.unitId));

        var remaining = familyUnits
            .Where(c => c.Rarity > 1 && !existingIds.Contains(c.Unit_ID))
            .ToList();

        if (remaining.Count == 0)
        {
            Debug.Log($"[EnemyGrowth] {fid} 가문에 추가할 유닛 없음");
            return;
        }

        int minRarity = remaining.Min(c => c.Rarity);
        var candidates = remaining.Where(c => c.Rarity == minRarity).ToList();

        var picked = candidates[UnityEngine.Random.Range(0, candidates.Count)];
        var newUnit = new Unit(picked.Unit_ID, picked.Rarity, picked.Unit_Name, picked.Class);
        newUnit.level = startLevel;
        newUnit.exp = 0f;
        team.units.Add(newUnit);

        Debug.Log($"[EnemyGrowth] {team.name}에 {picked.Unit_Name} 추가 (level {startLevel})");
    }
}