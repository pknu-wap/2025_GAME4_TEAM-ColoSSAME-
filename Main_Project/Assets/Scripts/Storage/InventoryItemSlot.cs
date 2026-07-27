using UnityEngine;
using UnityEngine.UI;

public class InventoryItemSlot : MonoBehaviour, ISlotView<InventorySlotData>
{
    [Header("슬롯 UI")]
    public Image iconImage;
    public Text nameText;
    public Text countText;
    
    public void SetItem(InventorySlotData data) => Set(data.Item, data.Count);

    public void Set(ItemData data, int count)
    {
        if (data == null)
        {
            Clear();
            return;
        }

        if (iconImage != null)
        {
            iconImage.sprite = data.icon;
            iconImage.enabled = (data.icon != null);
        }

        if (nameText != null) nameText.text = data.itemName;

        if (countText != null) countText.text = $"x {count}";
        else if (nameText != null) nameText.text = $"{data.itemName} x {count}";
    }
    
    public void Clear()
    {
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        if (nameText != null) nameText.text = "";
        if (countText != null) countText.text = "";
    }
}