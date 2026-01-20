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
}