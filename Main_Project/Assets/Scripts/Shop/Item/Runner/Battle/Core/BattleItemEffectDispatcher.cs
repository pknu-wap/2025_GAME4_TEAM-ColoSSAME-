using System.Collections.Generic;
using BattleK.Scripts.AI;

internal sealed class BattleItemEffectDispatcher
{
    private readonly BattleItemEffectContext context;
    private readonly Dictionary<ItemEffectKind, IBattleItemEffectHandler> handlers = new();
    private readonly BattleReviveOnceHandler reviveOnceHandler;
    private readonly BattlePeriodicHealMaxHpPercentHandler periodicHealHandler;
    private readonly BattleLowHpStatPercentBonusHandler lowHpHandler;
    private readonly BattleKillStackingStatPercentHandler killStackingHandler;
    private readonly BattleTeamDeathStackingStatPercentHandler teamDeathStackingHandler;
    private readonly BattleDeathExplosionMaxHpPercentHandler deathExplosionHandler;

    public BattleItemEffectDispatcher(BattleItemEffectContext context)
    {
        this.context = context;

        reviveOnceHandler = new BattleReviveOnceHandler();
        periodicHealHandler = new BattlePeriodicHealMaxHpPercentHandler();
        lowHpHandler = new BattleLowHpStatPercentBonusHandler();
        killStackingHandler = new BattleKillStackingStatPercentHandler();
        teamDeathStackingHandler = new BattleTeamDeathStackingStatPercentHandler();
        deathExplosionHandler = new BattleDeathExplosionMaxHpPercentHandler();

        RegisterHandler(new BattleStatFlatBonusHandler());
        RegisterHandler(new BattleStatPercentBonusHandler());
    }

    public void RegisterHandler(IBattleItemEffectHandler handler)
    {
        if (handler == null) return;

        handlers[handler.EffectKind] = handler;
    }

    public void Dispatch(ItemEffectTrigger trigger)
    {
        IReadOnlyList<BattleItemEffectRuntime> effects = context.Registry.Effects;

        for (int i = 0; i < effects.Count; i++)
        {
            BattleItemEffectRuntime runtime = effects[i];
            if (!BattleItemEffectRules.IsBattleTrigger(runtime, trigger)) continue;

            Execute(runtime);
        }
    }

    public void UpdateTickEffects(float deltaTime)
    {
        IReadOnlyList<BattleItemEffectRuntime> effects = context.Registry.Effects;

        for (int i = 0; i < effects.Count; i++)
        {
            BattleItemEffectRuntime runtime = effects[i];
            if (!BattleItemEffectRules.IsBattleTrigger(runtime, ItemEffectTrigger.Tick)) continue;

            periodicHealHandler.Tick(runtime, context, deltaTime);
        }
    }

    public void UpdateLowHpEffects()
    {
        IReadOnlyList<BattleItemEffectRuntime> effects = context.Registry.Effects;

        for (int i = 0; i < effects.Count; i++)
        {
            BattleItemEffectRuntime runtime = effects[i];
            if (!BattleItemEffectRules.IsBattleTrigger(runtime, ItemEffectTrigger.LowHp)) continue;

            lowHpHandler.Update(runtime, context);
        }
    }

    public bool TryReviveOnDeath(StaticAICore owner)
    {
        if (!owner || !context.Registry.TryGetOwnerEffects(owner, out List<BattleItemEffectRuntime> ownerEffects))
            return false;

        for (int i = 0; i < ownerEffects.Count; i++)
        {
            BattleItemEffectRuntime runtime = ownerEffects[i];
            if (!BattleItemEffectRules.IsBattleTrigger(runtime, ItemEffectTrigger.Death)) continue;
            if (runtime.Effect.effectKind != ItemEffectKind.ReviveOnce) continue;

            if (reviveOnceHandler.TryRevive(runtime, context))
                return true;
        }

        return false;
    }

    public void NotifyUnitDeath(StaticAICore deadUnit, StaticAICore killer)
    {
        if (!deadUnit) return;

        ApplyOwnerDeathEffects(deadUnit);
        ApplyTeamDeathStackEffects(deadUnit);

        if (killer && killer != deadUnit)
        {
            NotifyUnitKill(killer, deadUnit);
        }
    }

    public void NotifyUnitKill(StaticAICore killer, StaticAICore victim)
    {
        if (!killer || !victim) return;
        if (!context.Registry.TryGetOwnerEffects(killer, out List<BattleItemEffectRuntime> ownerEffects))
            return;

        for (int i = 0; i < ownerEffects.Count; i++)
        {
            BattleItemEffectRuntime runtime = ownerEffects[i];
            if (!BattleItemEffectRules.IsBattleTrigger(runtime, ItemEffectTrigger.Kill)) continue;

            killStackingHandler.Execute(runtime, context);
        }
    }

    private void Execute(BattleItemEffectRuntime runtime)
    {
        if (handlers.TryGetValue(runtime.Effect.effectKind, out IBattleItemEffectHandler handler))
        {
            handler.Execute(runtime, context);
        }
    }

    private void ApplyOwnerDeathEffects(StaticAICore deadUnit)
    {
        if (!context.Registry.TryGetOwnerEffects(deadUnit, out List<BattleItemEffectRuntime> ownerEffects))
            return;

        for (int i = 0; i < ownerEffects.Count; i++)
        {
            BattleItemEffectRuntime runtime = ownerEffects[i];
            if (!BattleItemEffectRules.IsBattleTrigger(runtime, ItemEffectTrigger.Death)) continue;

            deathExplosionHandler.Execute(runtime, context);
        }
    }

    private void ApplyTeamDeathStackEffects(StaticAICore deadUnit)
    {
        IReadOnlyList<BattleItemEffectRuntime> effects = context.Registry.Effects;

        for (int i = 0; i < effects.Count; i++)
        {
            BattleItemEffectRuntime runtime = effects[i];
            if (!runtime.Owner || runtime.Owner == deadUnit || runtime.Owner.IsDead) continue;
            if (!context.TargetResolver.IsSameTeam(runtime.Owner, deadUnit)) continue;
            if (!BattleItemEffectRules.IsBattleTrigger(runtime, ItemEffectTrigger.Death)) continue;

            teamDeathStackingHandler.Execute(runtime, context);
        }
    }
}
