using BattleK.Scripts.AI;
using UnityEngine;

internal sealed class BattleReviveOnceHandler : IBattleItemEffectHandler
{
    public ItemEffectKind EffectKind => ItemEffectKind.ReviveOnce;

    public void Execute(BattleItemEffectRuntime runtime, BattleItemEffectContext context)
    {
        TryRevive(runtime, context);
    }

    public bool TryRevive(BattleItemEffectRuntime runtime, BattleItemEffectContext context)
    {
        StaticAICore owner = runtime.Owner;
        if (!owner || owner.Stat == null) return false;
        if (!BattleItemEffectRules.CanTrigger(runtime, 1)) return false;

        int reviveHp = BattleItemEffectRules.ResolveReviveHp(owner, runtime.Effect);
        owner.Stat.CurrentHP = Mathf.Clamp(reviveHp, 1, owner.Stat.MaxHP);
        BattleItemStatApplier.UpdateHpBar(owner);
        runtime.TriggerCount++;
        return true;
    }
}
