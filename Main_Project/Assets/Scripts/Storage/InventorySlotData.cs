using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public readonly struct InventorySlotData
{
    public readonly ItemData Item;
    public readonly int Count;

    public InventorySlotData(ItemData item, int count)
    {
        Item = item;
        Count = count;
    }
}