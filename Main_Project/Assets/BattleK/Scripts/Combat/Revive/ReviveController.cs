using System.Collections.Generic;
using BattleK.Scripts.AI;
using BattleK.Scripts.Combat.Status;
using BattleK.Scripts.Manager.Battle;
using UnityEngine;

namespace BattleK.Scripts.Combat.Revive
{
    public class ReviveController
    {
        private readonly StaticAICore _owner;
        private readonly List<IReviveProvider> _providers = new();

        public int Count => _providers.Count;

        public ReviveController(StaticAICore owner)
        {
            _owner = owner;
        }

        public void Add(IReviveProvider provider)
        {
            if (provider != null) _providers.Add(provider);
        }

        public void Remove(IReviveProvider provider) => _providers.Remove(provider);

        public void Clear() => _providers.Clear();

        public bool TryRevive(out ReviveResult result)
        {
            IReviveProvider chosen = null;
            foreach (var provider in _providers)
            {
                if (!provider.CanRevive(_owner)) continue;
                if (chosen == null || provider.Priority > chosen.Priority) chosen = provider;
            }

            if (chosen == null)
            {
                result = default;
                return false;
            }

            result = chosen.Revive(_owner);
            if (chosen.IsSpent) _providers.Remove(chosen);

            Apply(result);
            return true;
        }

        private void Apply(in ReviveResult result)
        {
            _owner.Health.SetCurrent(Mathf.Max(1, result.Hp));

            _owner.TryGetComponent(out StatusEffectManager host);

            if (result.ClearStatusEffects)
            {
                if (host) host.ClearAll();
                _owner.RemoveAllDebuffs();
                _owner.ExitCCState();
            }

            if (result.InvincibleSeconds > 0f && host) host.ApplyEffect(new TimedInvincibleEffect(_owner, result.InvincibleSeconds, result.Visual));
        }
    }
}