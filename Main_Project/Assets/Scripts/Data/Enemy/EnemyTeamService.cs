using BattleK.Scripts.AI.Skill.Base;
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
    public static void GrowUnitsAfterRound(League league, Round playedRound)
    {
        int playerTeamId = league.settings.playerTeamId;
        int cap = league.settings.tier * 10;

        foreach (Team leagueTeam in league.teams)
        {
            if (leagueTeam.id == playerTeamId) continue;

            EnemyTeam team = EnemySaveManager.Instance.GetTeam(leagueTeam.id);
            if (team == null) continue;

            int result = GetTeamResult(playedRound, leagueTeam.id);

            bool changed = false;
            foreach (Unit unit in team.units)
            {
                if (unit.level >= cap) continue;
                int grow = GrowAmount(result);           
                if (grow > 0)
                {
                    unit.level = Mathf.Min(unit.level + grow, cap);
                    changed = true;
                }
            }
            if (changed) EnemySaveManager.Instance.SaveTeam(team);
        }
    }

    private static int GetTeamResult(Round round, int teamId)
    {
        var match = round.matches.Find(m => m.teamAId == teamId || m.teamBId == teamId);
        if (match?.result == null) return -1;              
        if (match.result.winner == 0) return 0;             
        return match.result.winner == teamId ? 1 : -1;       
    }

    private static int GrowAmount(int result)
    {
        switch (result)
        {
            case 1: return UnityEngine.Random.Range(0, 3);              // 승: 0,1,2
            case 0: return UnityEngine.Random.Range(0, 2);              // 무: 0,1
            default: return UnityEngine.Random.value < 0.25f ? 1 : 0;    // 패: 25% +1
        }
    }

    // 리그 승급 성장
    public static void GrowTeamsForNextLeague(League league, int nextTier)
    {
        if (UnitDataManager.Instance == null) return;

        int playerTeamId = league.settings.playerTeamId;
        int resetLevel = (nextTier - 1) * 10;

        foreach (Team leagueTeam in league.teams)
        {
            if (leagueTeam.id == playerTeamId) continue;

            EnemyTeam team = EnemySaveManager.Instance.GetTeam(leagueTeam.id);
            if (team == null) continue;

            int rankBonus = Mathf.Max(0, 4 - leagueTeam.rank);

            foreach (Unit unit in team.units)
            {
                unit.rarity = Mathf.Min(unit.rarity + 1, 5);
                unit.level = resetLevel + rankBonus;
                unit.exp = 0f;
                GrantSkillByRarity(unit);
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
        GrantSkillsUpToRarity(newUnit);
        team.units.Add(newUnit);

        Debug.Log($"[EnemyGrowth] {team.name}에 {picked.Unit_Name} 추가 (level {startLevel})");
    }

    private static SkillPoolRegistrySO _registry;
    private static SkillPoolRegistrySO Registry =>
        _registry != null ? _registry : (_registry = Resources.Load<SkillPoolRegistrySO>("SkillPool/SkillPoolRegistry"));

    // 등급에 맞는 스킬 자동 부여
    private static void GrantSkillByRarity(Unit unit)
    {
        var pool = Registry?.GetPool(unit.unitClass);
        if (pool == null) return;
        int r = unit.rarity;

        if (r == 3 || r == 4)
        {
            var choices = pool.GetSkillChoices(r);
            if (choices.Count == 0) return;
            AddSkill(unit, choices[Random.Range(0, choices.Count)]);
        }
        else if (r == 5)
        {
            var ult = pool.GetUltimate();
            if (ult != null) AddSkill(unit, ult);
        }
    }

    private static void AddSkill(Unit unit, SkillSO skill)
    {
        if (unit.skills.Exists(s => s.skillName == skill.SkillName)) return;
        unit.skills.Add(new UnitSkill(skill.SkillName, 1));

        if (!unit.selectedSkills.Contains(skill.SkillName))
            unit.selectedSkills.Add(skill.SkillName);
    }

    // 획득 유닛 스킬 소급 부여
    private static void GrantSkillsUpToRarity(Unit unit)
    {
        var pool = Registry?.GetPool(unit.unitClass);
        if (pool == null) return;

        for (int r = 3; r <= Mathf.Min(unit.rarity, 4); r++)
        {
            var choices = pool.GetSkillChoices(r);
            if (choices.Count == 0) continue;
            AddSkill(unit, choices[Random.Range(0, choices.Count)]);
        }
        if (unit.rarity >= 5)
        {
            var ult = pool.GetUltimate();
            if (ult != null) AddSkill(unit, ult);
        }
    }

    // 팀 전력 점수
    public static float GetTeamPower(int teamId)
    {
        EnemyTeam team = EnemySaveManager.Instance.GetTeam(teamId);
        if (team == null || team.units == null) return 0f;

        float power = 0f;
        foreach (var u in team.units)
        {
            power += u.level * 4f;                          // 레벨 
            //power += u.rarity * 3f;                         // 등급 
            //power += (u.selectedSkills?.Count ?? 0) * 3f;   // 장착 스킬
        }
        return power;
    }
}

