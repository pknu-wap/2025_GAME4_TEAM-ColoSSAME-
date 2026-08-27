using System;
using System.Collections.Generic;
using System.Linq;
using BattleK.Scripts.AI;
using BattleK.Scripts.Manager;
using UnityEngine;

namespace BattleK.Scripts.HP
{
    public enum StatusVisualType
    {
        Normal,
        Poison,
        Burn,
        Freeze,
        Stun,
        Buff
    }

    [Serializable]
    public struct StatusColorEntry
    {
        public StatusVisualType Type;
        public Color Color;
        [Tooltip("숫자가 클수록 우선 적용")]
        public int Priority;
    }

    public class HPManager : MonoBehaviour
    {
        [Header("Manager")]
        [SerializeField] private AI_Manager _aiManager;

        [Header("AICore")]
        public List<StaticAICore> _playerUnits = new();
        public List<StaticAICore> _enemyUnits  = new();

        [Header("Team Default Colors")]
        [Tooltip("상태이상이 없을 때만 적용되는 기본 색")]
        [SerializeField] private Color allyDefaultColor = Color.green;
        [SerializeField] private Color enemyDefaultColor = Color.red;

        [Header("Shield Color")]
        [SerializeField] private Color shieldColor = new Color(0.6f, 0.6f, 0.6f, 0.5f);

        [Header("Status Colors (피아 공통)")]
        [SerializeField] private List<StatusColorEntry> statusColors = new()
        {
            new StatusColorEntry { Type = StatusVisualType.Poison, Color = new Color(0.6f, 0.2f, 0.8f), Priority = 10 },
            new StatusColorEntry { Type = StatusVisualType.Burn,   Color = new Color(0.9f, 0.1f, 0.1f), Priority = 10 },
            new StatusColorEntry { Type = StatusVisualType.Freeze, Color = new Color(0.4f, 0.8f, 1f),   Priority = 10 },
            new StatusColorEntry { Type = StatusVisualType.Stun,   Color = new Color(0.9f, 0.9f, 0.2f), Priority = 5  },
            new StatusColorEntry { Type = StatusVisualType.Buff,   Color = new Color(0.2f, 1f, 0.6f),   Priority = 1  },
        };

        public void setUnits()
        {
            _playerUnits = _aiManager.playerUnits;
            _enemyUnits = _aiManager.enemyUnits;
        }

        public void ApplyHpToHPBar()
        {
            foreach (var target in _playerUnits.Where(t => t.HPBar))
                RefreshUnit(target, isAlly: true);

            foreach (var target in _enemyUnits.Where(t => t.HPBar))
                RefreshUnit(target, isAlly: false);
        }

        public void NotifyStatusChanged(StaticAICore unit, bool isAlly)
        {
            RefreshUnit(unit, isAlly);
        }

        private void RefreshUnit(StaticAICore unit, bool isAlly)
        {
            if (!unit.HPBar) return;

            unit.HPBar.UpdateHPBar();
            unit.HPBar.SetFillColor(ResolveColor(unit, isAlly));
            unit.HPBar.SetShieldColor(shieldColor);
        }

        private Color ResolveColor(StaticAICore unit, bool isAlly)
        {
            var baseColor = isAlly ? allyDefaultColor : enemyDefaultColor;

            StatusColorEntry? best = null;
            foreach (var entry in statusColors)
            {
                if (!unit.HasVisualStatus(entry.Type)) continue;
                if (best == null || entry.Priority > best.Value.Priority) best = entry;
            }

            return best?.Color ?? baseColor;
        }
    }
}