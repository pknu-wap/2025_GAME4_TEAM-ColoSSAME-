
using BattleK.Scripts.Manager.Battle;

namespace BattleK.Scripts.Combat.Status
{
    public interface IStatusEffect
    {
        object Key { get; }
        StatusStackPolicy StackPolicy { get; }

        void OnApply(StatusEffectManager host, float now);
        void Refresh(float now);
        bool Update(StatusEffectManager host, float now);
        void OnRemove(StatusEffectManager host);
    }
}