using Shop.Item;

internal interface IBattleItemEffectHandler
{
    ItemEffectKind EffectKind { get; }

    void Execute(BattleItemEffectRuntime runtime, BattleItemEffectContext context);
}
