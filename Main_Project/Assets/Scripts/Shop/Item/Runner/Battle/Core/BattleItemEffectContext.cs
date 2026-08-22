using BattleK.Scripts.Manager;

internal sealed class BattleItemEffectContext
{
    public BattleItemEffectRegistry Registry { get; }
    public BattleItemTargetResolver TargetResolver { get; }
    public BattleItemStatApplier StatApplier { get; }
    public AI_Manager AiManager { get; private set; }

    public BattleItemEffectContext(
        BattleItemEffectRegistry registry,
        BattleItemTargetResolver targetResolver,
        BattleItemStatApplier statApplier)
    {
        Registry = registry;
        TargetResolver = targetResolver;
        StatApplier = statApplier;
    }

    public void SetAiManager(AI_Manager source)
    {
        AiManager = source;
        TargetResolver.SetAiManager(source);
    }
}
