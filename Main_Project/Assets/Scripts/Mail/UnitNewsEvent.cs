using System;
using System.Collections.Generic;
using BattleK.Scripts.Data.Stat;

public enum UnitNewsType
{
    LevelJump,   // 한 라운드에 레벨 +2 이상
    NewSkill,    // 승급으로 새 스킬 획득
    Recruited,   // 새 유닛 영입
}

// 적 유닛에게 일어난 사건 하나
[Serializable]
public class UnitNewsEvent
{
    public int round;          // 0 = 리그 시작 시점(승급/영입)
    public int teamId;
    public string teamName;
    public string unitName;
    public UnitNewsType type;
    public int amount;         // 레벨 상승량 / 등급
    public string detail;      // 스킬 이름 등
}


public static class UnitNewsRecorder
{
    private const int KeepRounds = 2;   // 이보다 오래된 이벤트는 버림

    public static void Record(League league, int round, EnemyTeam team, Unit unit,
                              UnitNewsType type, int amount = 0, string detail = null)
    {
        if (league == null || team == null || unit == null) return;

        league.unitEvents ??= new List<UnitNewsEvent>();
        league.unitEvents.RemoveAll(e => e.round < round - KeepRounds);
        league.unitEvents.Add(new UnitNewsEvent
        {
            round = round,
            teamId = team.id,
            teamName = team.name,
            unitName = unit.UnitName,
            type = type,
            amount = amount,
            detail = detail,
        });
    }
}
