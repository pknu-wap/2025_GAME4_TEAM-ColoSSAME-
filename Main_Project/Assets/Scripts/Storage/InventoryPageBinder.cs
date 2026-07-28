using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryPageBinder : MonoBehaviour
{
    [Header("Item DB")] public ItemDatabase itemDatabase;

    [Header("슬롯들이 들어있는 부모(예: storage/Potion/Items)")]
    public Transform slotsParent;
    
    private List<InventoryItemSlot> slots;
    [Header("페이지 상단 카테고리 이름 표시")]
    public Text categoryLabelText;
    public ItemCategory category { get; private set; }
    
    public void SetCategory(ItemCategory newCategory)
    {
        category = newCategory;
        if (categoryLabelText != null)
            categoryLabelText.text = ItemCategoryDisplay.GetName(newCategory);
        Refresh();
    }
    public void SetEmpty()
    {
        if (categoryLabelText != null) categoryLabelText.text = "";
        foreach (var slot in slots) slot.Clear();
    }
    private void Awake()
    {
        slots = new List<InventoryItemSlot>();

        for (int i = 0; i < slotsParent.childCount; i++)
        {
            var slot = slotsParent.GetChild(i).GetComponent<InventoryItemSlot>();
            if (slot != null)
            {
                slots.Add(slot);
            }
            else
            {
                Debug.LogError($"BookPageBinder: {slotsParent.GetChild(i).name}에 InventoryItemSlot이 없습니다.");
            }
        }
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (UserManager.Instance == null || UserManager.Instance.user == null)
        {
            Debug.LogError("UserManager or user is null");
            return;
        }

        if (itemDatabase == null)
        {
            Debug.LogError("itemDatabase is null");
            return;
        }

        var items = InventoryQueryService.GetItemsByCategory(
            UserManager.Instance.user.inventory,
            itemDatabase,
            category);

        foreach (var slot in slots)
        {
            slot.Clear();
        }

        for (int i = 0; i < items.Count; i++)
        {
            if (i >= slots.Count)
            {
                Debug.LogWarning($"InventoryPageBinder({category}): 슬롯이 부족합니다. {items.Count}개 중 {slots.Count}개만 표시됨.");
                break;
            }

            slots[i].Set(items[i].Item, items[i].Count);
        }
    }
}
