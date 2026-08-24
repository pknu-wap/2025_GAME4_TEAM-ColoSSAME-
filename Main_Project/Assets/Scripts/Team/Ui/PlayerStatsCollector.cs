using System.Collections.Generic;
using UnityEngine;
using BattleK.Scripts.Data.Type;

public class PlayerStatsCollector : MonoBehaviour
{
    [SerializeField] private List<CharacterStatsRow> _playerStats = new();

    public IReadOnlyList<CharacterStatsRow> PlayerStats => _playerStats;


    public void CollectPlayerUnits()
    {
        _playerStats.Clear();

        var myUnits = UserManager.Instance.user.myUnits;

        if (myUnits == null)
        {
            Debug.LogWarning("myUnits 없음");
            return;
        }

        foreach (Unit unit in myUnits)
        {
            CharacterData data =
                UnitDataManager.Instance.GetCharacterData(unit.unitId);

            if (data == null)
                continue;

            _playerStats.Add(new CharacterStatsRow
            {
                Unit_ID = unit.unitId,
                Unit_Name = unit.unitName,
                Level = unit.level,
                Rarity = unit.rarity,

                ATK = data.Stat_Distribution.ATK,
                DEF = data.Stat_Distribution.DEF,
                HP = data.Stat_Distribution.HP,
                AGI = data.Stat_Distribution.AGI
            });
        }

        Debug.Log($"플레이어 스탯 수집 완료 : {_playerStats.Count}");
    }
}