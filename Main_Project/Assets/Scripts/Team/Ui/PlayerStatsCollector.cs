using System.Collections.Generic;
using BattleK.Scripts.Data.Stat;
using UnityEngine;

public class PlayerStatsCollector : MonoBehaviour
{
    [SerializeField] private List<UnitBaseStat> _playerStats = new();

    public IReadOnlyList<UnitBaseStat> PlayerStats => _playerStats;

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
            CharacterData data = UnitDataManager.Instance.GetCharacterData(unit.Id);

            if (data == null)
                continue;

            _playerStats.Add(UnitBaseStat.FromFamilyAndSave(data, unit));
        }

        Debug.Log($"플레이어 스탯 수집 완료 : {_playerStats.Count}");
    }
}