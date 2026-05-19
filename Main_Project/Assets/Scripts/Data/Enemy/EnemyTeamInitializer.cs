using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyTeamInitializer
{
    private const int StartEnemyUnitCount = 4;

    public void InitializeFromLeague(League league)
    {
        if (league == null)
        {
            Debug.LogError("[EnemyTeamInitializer] league가 null입니다.");
            return;
        }

        if (league.teams == null)
        {
            Debug.LogError("[EnemyTeamInitializer] league.teams가 null입니다.");
            return;
        }

        if (UnitDataManager.Instance == null)
        {
            Debug.LogError("[EnemyTeamInitializer] UnitDataManager.Instance가 없습니다.");
            return;
        }

        if (!UnitDataManager.Instance.IsLoaded)
        {
            Debug.LogError("[EnemyTeamInitializer] UnitDataManager 로드가 아직 완료되지 않았습니다.");
            return;
        }

        if (EnemySaveManager.Instance == null)
        {
            Debug.LogError("[EnemyTeamInitializer] EnemySaveManager.Instance가 없습니다.");
            return;
        }

        foreach (Team leagueTeam in league.teams)
        {
            if (leagueTeam.id == league.settings.playerTeamId)
            {
                continue;
            }

            if (EnemySaveManager.Instance.HasTeam(leagueTeam.id))
            {
                continue;
            }

            EnemyTeam enemyTeam = CreateEnemyTeamFromLeagueTeam(leagueTeam);

            if (enemyTeam == null)
            {
                continue;
            }

            EnemySaveManager.Instance.AddTeam(enemyTeam);
        }

        EnemySaveManager.Instance.Save();

        Debug.Log("[EnemyTeamInitializer] 상대팀 초기화 완료");
    }

    private EnemyTeam CreateEnemyTeamFromLeagueTeam(Team leagueTeam)
    {
        List<CharacterData> familyUnits =
            UnitDataManager.Instance.GetFamilyUnits(leagueTeam.fid);

        if (familyUnits == null || familyUnits.Count == 0)
        {
            Debug.LogError($"[EnemyTeamInitializer] 가문 유닛을 찾을 수 없습니다. fid: {leagueTeam.fid}");
            return null;
        }

        EnemyTeam enemyTeam = new EnemyTeam(
            leagueTeam.id,
            leagueTeam.fid,
            leagueTeam.name
        );

        List<CharacterData> recruits = familyUnits
            .Where(character => character.Rarity == 1)
            .Take(StartEnemyUnitCount)
            .ToList();

        if (recruits.Count == 0)
        {
            Debug.LogWarning($"[EnemyTeamInitializer] 1성 훈련병이 없습니다. fid: {leagueTeam.fid}");
        }

        foreach (CharacterData character in recruits)
        {
            Unit unit = new Unit(
                character.Unit_ID,
                character.Rarity,
                character.Unit_Name,
                character.Class
            );

            unit.level = 1;
            unit.exp = 0f;

            enemyTeam.units.Add(unit);
        }

        return enemyTeam;
    }
}