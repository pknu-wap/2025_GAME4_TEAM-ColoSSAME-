using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Game/ItemDatabase", fileName="ItemDatabase")]
public class ItemDatabase : ScriptableObject {
    public List<ItemData> items = new();
    public ItemData GetById(int id) => items.Find(x => x.id == id);
}
