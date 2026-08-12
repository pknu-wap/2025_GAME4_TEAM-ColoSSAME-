using BattleK.Scripts.AI;

internal sealed class BattleItemEffectRuntime
{
    public ItemData Item { get; }
    public ItemEffectDefinition Effect { get; }
    public StaticAICore Owner { get; }
    public float TickTimer;
    public int TriggerCount;
    public bool IsActive;

    public BattleItemEffectRuntime(ItemData item, ItemEffectDefinition effect, StaticAICore owner)
    {
        Item = item;
        Effect = effect;
        Owner = owner;
    }
}
