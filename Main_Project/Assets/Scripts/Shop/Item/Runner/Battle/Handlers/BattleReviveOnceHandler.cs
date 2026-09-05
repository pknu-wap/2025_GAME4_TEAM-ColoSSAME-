using BattleK.Scripts.AI;
using Shop.Item;
using Shop.Item.Runner.Battle.Stat;
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
        if (!owner || owner.runtimeStat == null) return false;
        if (!BattleItemEffectRules.CanTrigger(runtime, 1)) return false;

        int reviveHp = BattleItemEffectRules.ResolveReviveHp(owner, runtime.Effect);
        owner.runtimeStat.CurrentHP = Mathf.Clamp(reviveHp, 1, owner.runtimeStat.MaxHP);
        BattleItemStatApplier.UpdateHpBar(owner);
        runtime.TriggerCount++;
        return true;
    }
}
