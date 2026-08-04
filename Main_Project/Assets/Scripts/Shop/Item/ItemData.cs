using System.Collections.Generic;
using UnityEngine;

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
    public ItemLifetimeType lifetimeType = ItemLifetimeType.OneBattle;
    public ItemSlotKind slotKind = ItemSlotKind.Consumable;

    [Header("Effects")]
    public List<ItemEffectDefinition> effects = new List<ItemEffectDefinition>();

    public bool IsOneBattleItem => lifetimeType == ItemLifetimeType.OneBattle;
    public bool IsPermanentItem => lifetimeType == ItemLifetimeType.Permanent;

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
}
