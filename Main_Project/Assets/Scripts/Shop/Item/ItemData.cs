using System.Collections.Generic;
using Shop.Item;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Game/Item", fileName = "NewItem")]

public class ItemData : ScriptableObject
{
    [Header("고유 ID (중복 금지)")]
    public int id;

    [Header("표시용 이름")]
    public string itemName;

    [Header("아이콘")]
    public Sprite icon;

    [Header("가격")]
    public int price;

    [Header("설명")]
    [TextArea(3, 8)]
    public string description;
    
    public ItemCategory category;

    [Header("Use Rules")]
    [FormerlySerializedAs("lifetimeType")]
    [SerializeField] private ItemUseType useType = ItemUseType.OneBattleConsumable;

    [Header("Effects")]
    [SerializeField] private List<ItemEffectDefinition> effects = new List<ItemEffectDefinition>();

    public ItemUseType UseType => useType;
    public IReadOnlyList<ItemEffectDefinition> Effects => effects;

    public bool ShouldConsumeAfterBattle()
    {
        return useType == ItemUseType.OneBattleConsumable;
    }

    public bool CanEquipTo(ItemUseType targetUseType)
    {
        return useType == targetUseType;
    }

    public bool HasEffectDomain(ItemEffectDomain domain)
    {
        if (effects == null) return false;

        for (int i = 0; i < effects.Count; i++)
        {
            if (effects[i] != null && effects[i].domain == domain)
                return true;
        }

        return false;
    }

    public IEnumerable<ItemEffectDefinition> GetEffects(ItemEffectDomain domain)
    {
        if (effects == null) yield break;

        for (int i = 0; i < effects.Count; i++)
        {
            ItemEffectDefinition effect = effects[i];
            if (effect != null && effect.domain == domain)
                yield return effect;
        }
    }

    public IEnumerable<ItemEffectDefinition> GetEffects(ItemEffectDomain domain, ItemEffectTrigger trigger)
    {
        if (effects == null) yield break;

        for (int i = 0; i < effects.Count; i++)
        {
            ItemEffectDefinition effect = effects[i];
            if (effect != null && effect.domain == domain && effect.trigger == trigger)
                yield return effect;
        }
    }
}
