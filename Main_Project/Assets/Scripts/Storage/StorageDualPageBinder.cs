using System.Collections.Generic;
using UnityEngine;

public class StorageDualPageBinder : MonoBehaviour
{
    [Header("DB")]
    public ItemDatabase itemDatabase;

    [Header("왼쪽(물약) Items 부모")]
    public Transform potionItemsParent;

    [Header("오른쪽(강화석) Items 부모")]
    public Transform materialItemsParent;

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (UserManager.Instance == null || UserManager.Instance.user == null)
        {
            Debug.LogError("❌ StorageDualPageBinder: UserManager 또는 user가 준비되지 않았습니다.");
            return;
        }

        if (itemDatabase == null)
        {
            Debug.LogError("❌ StorageDualPageBinder: itemDatabase가 연결되지 않았습니다.");
            return;
        }

        if (potionItemsParent == null || materialItemsParent == null)
        {
            Debug.LogError("❌ StorageDualPageBinder: ItemsParent(좌/우) 연결이 필요합니다.");
            return;
        }

        Dictionary<string, int> inv = UserManager.Instance.user.inventory;
        if (inv == null)
        {
            Debug.LogWarning("⚠️ StorageDualPageBinder: inventory가 null입니다.");
            return;
        }

        // 1) 좌/우 슬롯 먼저 전부 비우기
        ClearAllSlots(potionItemsParent);
        ClearAllSlots(materialItemsParent);

        // 2) 좌/우 채우기 인덱스
        int potionIndex = 0;
        int materialIndex = 0;

        // 3) inventory 순회하면서 카테고리에 따라 좌/우에 분배
        foreach (var kv in inv)
        {
            string key = kv.Key;
            int count = kv.Value;

            // key가 "1" 같은 id 문자열이라는 전제
            if (!int.TryParse(key, out int id))
            {
                Debug.LogWarning($"⚠️ inventory key가 숫자가 아닙니다: {key}");
                continue;
            }

            ItemData data = itemDatabase.GetById(id);
            if (data == null) continue;

            if (data.category == ItemCategory.Potion)
            {
                SetToSlot(potionItemsParent, potionIndex, data, count);
                potionIndex++;
            }
            else if (data.category == ItemCategory.Material)
            {
                SetToSlot(materialItemsParent, materialIndex, data, count);
                materialIndex++;
            }
            else
            {
                // Etc는 지금은 표시 안 하거나, 추후 별도 영역으로 분리 가능
            }
        }

        Debug.Log("✅ StorageDualPageBinder: 좌(물약)/우(강화석) 갱신 완료");
    }

    private void ClearAllSlots(Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            var slot = parent.GetChild(i).GetComponent<InventoryItemSlot>();
            if (slot != null) slot.Clear();
        }
    }

    private void SetToSlot(Transform parent, int index, ItemData data, int count)
    {
        if (index >= parent.childCount) return; // 슬롯 부족하면 그냥 멈춤

        var slotObj = parent.GetChild(index);
        var slot = slotObj.GetComponent<InventoryItemSlot>();
        if (slot == null)
        {
            Debug.LogError($"❌ {slotObj.name}에 InventoryItemSlot이 없습니다.");
            return;
        }

        slot.Set(data, count);
    }
}
