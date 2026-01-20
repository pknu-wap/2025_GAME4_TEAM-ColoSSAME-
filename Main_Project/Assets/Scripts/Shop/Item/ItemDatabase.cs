using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/ItemDatabase", fileName = "ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    [Header("아이템 목록")]
    public List<ItemData> items = new();

    private Dictionary<int, ItemData> cache;

    private void OnEnable()
    {
        BuildCache();
    }

    private void BuildCache()
    {
        cache = new Dictionary<int, ItemData>();

        foreach (var item in items)
        {
            if (item == null)
            {
                Debug.LogWarning("⚠️ ItemDatabase: null ItemData가 있습니다.");
                continue;
            }

            if (cache.ContainsKey(item.id))
            {
                Debug.LogWarning($"⚠️ ItemDatabase: id 중복 발견: {item.id}");
                continue;
            }

            cache.Add(item.id, item);
        }
    }

    /// <summary>
    /// id로 아이템 데이터 조회
    /// </summary>
    public ItemData GetById(int id)
    {
        if (cache == null || cache.Count != items.Count)
            BuildCache();

        cache.TryGetValue(id, out ItemData result);
        return result;
    }
}