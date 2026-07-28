using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InventoryQueryService
{
    public static List<InventorySlotData> GetItemsByCategory(
        Dictionary<string, int> inventory,
        ItemDatabase itemDatabase,
        ItemCategory category)
    {
        var result = new List<InventorySlotData>();

        if (inventory == null || itemDatabase == null)
        {
            Debug.LogWarning("Inventory or ItemDatabase is null.");
            return result;
        }

        foreach (var kv in inventory)
        {
            string key = kv.Key;
            int count = kv.Value;

            if (!int.TryParse(key, out int id))
            {
                Debug.LogWarning($"inventory key is not number {key}");
                continue;
            }
            
            ItemData data = itemDatabase.GetById(id);
            if (data == null) continue;

            if (data.category != category) continue;

            result.Add(new InventorySlotData(data, count));
        }
        result.Sort((a, b) => a.Item.id.CompareTo(b.Item.id));
        return result;
    }
}