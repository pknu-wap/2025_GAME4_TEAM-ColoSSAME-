using System;
using BattleK.Scripts.AI;
using UnityEngine;

namespace BattleK.Scripts.Combat
{
    public class UnitHealth
    {
        private readonly StaticAICore _owner;
        private int _current;
        private int _max;
        private bool _initialized;

        public event Action<UnitHealth> Changed;

        public UnitHealth(StaticAICore owner)
        {
            _owner = owner;
        }

        public int Current
        {
            get { EnsureInitialized(); return _current; }
        }

        public int Max
        {
            get { EnsureInitialized(); return _max; }
        }

        public float Ratio => Max <= 0 ? 0f : Mathf.Clamp01((float)Current / Max);

        public void Initialize(int max, int current)
        {
            _max = Mathf.Max(1, max);
            _current = Mathf.Clamp(current, 1, _max);
            _initialized = true;
            Changed?.Invoke(this);
        }

        public int Reduce(int amount)
        {
            var applied = Mathf.Clamp(amount, 0, Current);
            if (applied == 0) return 0;

            _current -= applied;
            Changed?.Invoke(this);
            return applied;
        }

        public int Increase(int amount)
        {
            var before = Current;
            _current = Mathf.Min(before + amount, _max);
            var delta = _current - before;
            if (delta != 0) Changed?.Invoke(this);
            return delta;
        }

        public void SetCurrent(int value)
        {
            EnsureInitialized();
            _current = Mathf.Clamp(value, 0, _max);
            Changed?.Invoke(this);
        }

        public void SetMax(int newMax)
        {
            EnsureInitialized();
            var diff = newMax - _max;
            _max = Mathf.Max(1, newMax);
            _current = Mathf.Clamp(_current + diff, 1, _max);
            Changed?.Invoke(this);
        }

        public void NotifyChanged() => Changed?.Invoke(this);

        private void EnsureInitialized()
        {
            if (_initialized) return;

            var stat = _owner.runtimeStat;
            _max = stat.MaxHP;
            _current = stat.CurrentHP;
            _initialized = true;
        }
    }
}