using System.Collections.Generic;
using System.Linq;
using BattleK.Scripts.AI;
using BattleK.Scripts.Manager;
using UnityEngine;

namespace BattleK.Scripts.HP
{
    public class HPManager : MonoBehaviour
    {
        [Header("Manager")]
        [SerializeField] private AI_Manager _aiManager;
    
        [Header("AICore")]
        public List<AICore> _playerUnits = new();
        public List<AICore> _enemyUnits  = new();

        public void setUnits()
        {
            _playerUnits = _aiManager.playerUnits;
            _enemyUnits = _aiManager.enemyUnits;
        }
        public void ApplyHpToHPBar()
        {
            foreach (var target in _playerUnits.Where(target => target.hpBar != null))
            {
                target.hpBar.UpdateHPBar();
            }

            foreach (var target in _enemyUnits.Where(target => target.hpBar != null))
            {
                target.hpBar.UpdateHPBar();
            }
        }
    }
}
