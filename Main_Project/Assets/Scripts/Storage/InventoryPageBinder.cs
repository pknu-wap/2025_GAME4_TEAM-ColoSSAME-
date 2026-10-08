using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 슬롯 표시 전용 뷰. 어떤 페이지를 보여줄지는 BookSpreadController가 결정한다.
public class InventoryPageBinder : MonoBehaviour
{
    [Header("Item DB")] public ItemDatabase itemDatabase;

    [Header("슬롯들이 들어있는 부모(예: storage/Potion/Items)")]
    public Transform slotsParent;
    
    private List<InventoryItemSlot> slots;
    [Header("페이지 상단 카테고리 이름 표시")]
    public Text categoryLabelText;

    // Awake 실행 순서(비활성 content)에 의존하지 않도록 처음 필요할 때 수집한다.
    private List<InventoryItemSlot> Slots
    {
        get
        {
            if (slots != null) return slots;

            slots = new List<InventoryItemSlot>();

            if (slotsParent == null)
            {
                Debug.LogError("InventoryPageBinder: slotsParent가 연결되어 있지 않습니다.", this);
                return slots;
            }

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

            return slots;
        }
    }

    public int SlotCount => Slots.Count;

    public void ShowPage(string label, IReadOnlyList<InventorySlotData> items)
    {
        if (categoryLabelText != null) categoryLabelText.text = label;

        var currentSlots = Slots;
        foreach (var slot in currentSlots) slot.Clear();

        for (int i = 0; i < items.Count && i < currentSlots.Count; i++)
        {
            currentSlots[i].Set(items[i].Item, items[i].Count);
        }
    }

    public void SetEmpty()
    {
        if (categoryLabelText != null) categoryLabelText.text = "";
        foreach (var slot in Slots) slot.Clear();
    }
}
