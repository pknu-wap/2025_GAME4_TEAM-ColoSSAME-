using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPManager : MonoBehaviour
{
    [Header("Manager")]
    [SerializeField] private AI_Manager ai_Manager;
    
    [Header("AICore")]
    public List<AICore> PlayerUnits = new List<AICore>();
    public List<AICore> EnemyUnits  = new List<AICore>();
    
    [Header("HP Bar")]
    public HPBar HPBar;

    public void setUnits()
    {
        PlayerUnits = ai_Manager.playerUnits;
        EnemyUnits = ai_Manager.enemyUnits;
    }
    public void ApplyHpToHPBar()
    {
        foreach (var target in PlayerUnits)
        {
            if(target.hpBar == null) continue;
            target.hpBar.UpdateHPBar();
        }
        foreach (var target in EnemyUnits)
        {
            if(target.hpBar == null) continue;
            target.hpBar.UpdateHPBar();
        }
    }
}
