using System;
using System.Collections.Generic;
using System.Linq;
using BattleK.Scripts.AI;
using BattleK.Scripts.Combat.Damage;
using BattleK.Scripts.Combat.Status;
using BattleK.Scripts.HP;
using UnityEngine;

namespace BattleK.Scripts.Manager.Battle
{
    public class StatusEffectManager : MonoBehaviour
    {
        [Header("References")]
        public StaticAICore _aiCore;

        private readonly Dictionary<StatusVisualType, int> _visualRefCounts = new();

        private readonly List<IStatusEffect> _effects = new();
        private readonly List<IStatusEffect> _buffer = new();
        private readonly Dictionary<UnitStatusFlags, int> _flagCounts = new();
        private static readonly UnitStatusFlags[] AllFlags = (UnitStatusFlags[])Enum.GetValues(typeof(UnitStatusFlags));
        private int _hardCcCount;

        private void Awake()
        {
            if (!_aiCore) TryGetComponent(out _aiCore);
            enabled = false;
        }

        private void Update()
        {
            var now = Time.time;

            _buffer.Clear();
            _buffer.AddRange(_effects);

            foreach (var effect in from effect in _buffer where _effects.Contains(effect) where effect.Update(this, now) where _effects.Remove(effect) select effect)
            {
                effect.OnRemove(this);
            }

            if (_effects.Count == 0) enabled = false;
        }
        
        public void ApplyEffect(IStatusEffect effect)
        {
            var now = Time.time;

            if (effect.StackPolicy != StatusStackPolicy.Independent)
            {
                var existing = _effects.Find(e => Equals(e.Key, effect.Key));
                if (existing != null)
                {
                    if (effect.StackPolicy == StatusStackPolicy.Refresh) existing.Refresh(now);
                    return;
                }
            }

            _effects.Add(effect);
            effect.OnApply(this, now);
            enabled = true;
        }
        
        public void ClearAll()
        {
            var snapshot = _effects.ToArray();
            _effects.Clear();
            foreach (var effect in snapshot) effect.OnRemove(this);

            _flagCounts.Clear();
            enabled = false;
        }

        public void ApplyDot(object key, StatusStackPolicy stackPolicy, StaticAICore attacker, int amount, DamageType type, DamageFlags flags, IDamageSourceProfile profile, object origin, float interval, float duration, StatusVisualType visual)
        {
            ApplyEffect(new DotEffect(key, stackPolicy, attacker, _aiCore, amount, type, flags, profile, origin, interval, duration, visual));
        }
        
        public void AddVisual(StatusVisualType type)
        {
            _visualRefCounts.TryGetValue(type, out var count);
            _visualRefCounts[type] = count + 1;
            _aiCore.SetVisualStatus(type, true);
        }

        public void RemoveVisual(StatusVisualType type)
        {
            if (!_visualRefCounts.TryGetValue(type, out var count)) return;
            count = Mathf.Max(0, count - 1);
            _visualRefCounts[type] = count;
            if (count == 0) _aiCore.SetVisualStatus(type, false);
        }

        public void EnterHardCc(PlayerState anim)
        {
            if (_hardCcCount++ == 0) _aiCore.EnterCCState(anim);
        }

        public void ExitHardCc()
        {
            _hardCcCount = Mathf.Max(0, _hardCcCount - 1);
            if (_hardCcCount == 0) _aiCore.ExitCCState();
        }
        
        public void AddFlags(UnitStatusFlags flags)
        {
            foreach (var f in AllFlags)
            {
                if (f == UnitStatusFlags.None || (flags & f) == 0) continue;
                _flagCounts.TryGetValue(f, out var n);
                _flagCounts[f] = n + 1;
            }
        }

        public void RemoveFlags(UnitStatusFlags flags)
        {
            foreach (var f in AllFlags)
            {
                if (f == UnitStatusFlags.None || (flags & f) == 0) continue;
                if (!_flagCounts.TryGetValue(f, out var n)) continue;
                if (n <= 1) _flagCounts.Remove(f);
                else _flagCounts[f] = n - 1;
            }
        }

        public bool HasFlag(UnitStatusFlags flag) => _flagCounts.TryGetValue(flag, out var n) && n > 0;

        public bool HasTag(StatusTags tag)
        {
            foreach (var e in _effects)
                if (e is ITaggedStatusEffect t && (t.Tags & tag) != 0) return true;
            return false;
        }

        public int RemoveByTag(StatusTags include, StatusTags exclude = StatusTags.None)
        {
            var removed = 0;
            foreach (var effect in _effects.ToArray())
            {
                if (effect is not ITaggedStatusEffect t) continue;
                if ((t.Tags & include) == 0 || (t.Tags & exclude) != 0) continue;

                _effects.Remove(effect);
                effect.OnRemove(this);
                removed++;
            }

            if (_effects.Count == 0) enabled = false;
            return removed;
        }
    }
}