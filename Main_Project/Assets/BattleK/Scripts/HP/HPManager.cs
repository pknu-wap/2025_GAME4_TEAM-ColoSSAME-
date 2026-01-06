using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPManager : MonoBehaviour
{
    [Header("Manager")]
    [SerializeField] private AI_Manager _aiManager;
    
    [Header("AICore")]
    public List<AICore> _playerUnits = new List<AICore>();
    public List<AICore> _enemyUnits  = new List<AICore>();

    public void setUnits()
    {
        _playerUnits = _aiManager.playerUnits;
        _enemyUnits = _aiManager.enemyUnits;
    }
    public void ApplyHpToHPBar()
    {
        foreach (var target in _playerUnits)
        {
            if(target.hpBar == null) continue;
            target.hpBar.UpdateHPBar();
        }
        foreach (var target in _enemyUnits)
        {
            if(target.hpBar == null) continue;
            target.hpBar.UpdateHPBar();
        }
    }
}
