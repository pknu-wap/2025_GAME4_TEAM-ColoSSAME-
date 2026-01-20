using System.Collections.Generic;
using UnityEngine;

public class InventoryUIBinder : MonoBehaviour
{
    [Header("Item DB")]
    public ItemDatabase itemDatabase;

    [Header("슬롯들이 들어있는 부모(예: storage/Potion/Items)")]
    public Transform itemsParent;

    [Header("이 페이지에 표시할 카테고리")]
    public ItemCategory showCategory = ItemCategory.Potion;

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (UserManager.Instance == null || UserManager.Instance.user == null)
        {
            Debug.LogError("❌ InventoryUIBinder: UserManager 또는 user가 준비되지 않았습니다.");
            return;
        }

        if (itemDatabase == null)
        {
            Debug.LogError("❌ InventoryUIBinder: itemDatabase가 연결되지 않았습니다.");
            return;
        }

        if (itemsParent == null)
        {
            Debug.LogError("❌ InventoryUIBinder: itemsParent가 연결되지 않았습니다.");
            return;
        }

        Dictionary<string, int> inv = UserManager.Instance.user.inventory;
        if (inv == null)
        {
            Debug.LogWarning("⚠️ InventoryUIBinder: inventory가 null입니다.");
            return;
        }

        // 1) 슬롯 전부 비우기
        for (int i = 0; i < itemsParent.childCount; i++)
        {
            var slot = itemsParent.GetChild(i).GetComponent<InventoryItemSlot>();
            if (slot != null) slot.Clear();
        }

        // 2) inventory 순회하면서 "해당 카테고리"만 슬롯에 채움
        int slotIndex = 0;

        foreach (var kv in inv)
        {
            if (slotIndex >= itemsParent.childCount) break;

            string key = kv.Key;
            int count = kv.Value;

            if (!int.TryParse(key, out int id))
            {
                Debug.LogWarning($"⚠️ inventory key가 숫자가 아닙니다: {key}");
                continue;
            }

            ItemData data = itemDatabase.GetById(id);
            if (data == null) continue;

            // ✅ 여기서 카테고리 필터
            if (data.category != showCategory) continue;

            var slotObj = itemsParent.GetChild(slotIndex);
            var slotComp = slotObj.GetComponent<InventoryItemSlot>();
            if (slotComp == null)
            {
                Debug.LogError($"❌ {slotObj.name}에 InventoryItemSlot이 없습니다.");
                continue;
            }

            slotComp.Set(data, count);
            slotIndex++;
        }

        Debug.Log($"✅ InventoryUIBinder: {showCategory} 페이지 갱신 완료");
    }
}
