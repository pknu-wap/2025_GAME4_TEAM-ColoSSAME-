using System.Collections.Generic;
using BattleK.Scripts.AI;

internal sealed class BattleItemEffectRegistry
{
    private readonly List<BattleItemEffectRuntime> effects = new();
    private readonly Dictionary<StaticAICore, List<BattleItemEffectRuntime>> effectsByOwner = new();

    public IReadOnlyList<BattleItemEffectRuntime> Effects => effects;

    public void Clear()
    {
        effects.Clear();
        effectsByOwner.Clear();
    }

    public bool RegisterUnit(StaticAICore unit)
    {
        if (!unit || unit.Stat == null || unit.Stat.Item == null) return false;
        if (effectsByOwner.ContainsKey(unit)) return false;

        ItemData item = unit.Stat.Item;
        if (!item.HasEffectDomain(ItemEffectDomain.Battle)) return false;

        List<BattleItemEffectRuntime> ownerEffects = new List<BattleItemEffectRuntime>();

        foreach (ItemEffectDefinition effect in item.GetEffects(ItemEffectDomain.Battle))
        {
            if (effect == null) continue;

            BattleItemEffectRuntime runtime = new BattleItemEffectRuntime(item, effect, unit);
            effects.Add(runtime);
            ownerEffects.Add(runtime);
        }

        if (ownerEffects.Count <= 0) return false;

        effectsByOwner.Add(unit, ownerEffects);
        return true;
    }

    public bool TryGetOwnerEffects(
        StaticAICore owner,
        out List<BattleItemEffectRuntime> ownerEffects)
    {
        return effectsByOwner.TryGetValue(owner, out ownerEffects);
    }
}
