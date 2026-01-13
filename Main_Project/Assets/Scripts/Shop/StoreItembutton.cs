using UnityEngine;
using UnityEngine.UI;

public class StoreItemButton : MonoBehaviour
{
    [Header("아이템 ID")]
    public int itemId;

    [Header("DB")]
    public ItemDatabase itemDatabase;

    [Header("버튼 UI")]
    public Image iconImage;
    public Text priceText;

    [Header("상세 패널")]
    public ItemDetailPanel detailPanel;

    private void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (itemDatabase == null) return;

        ItemData data = itemDatabase.GetById(itemId);
        if (data == null) return;

        if (iconImage != null) iconImage.sprite = data.icon;
        if (priceText != null) priceText.text = data.price.ToString();
    }

    public void OnClickItem()
    {
        if (detailPanel == null) return;
        detailPanel.Show(itemId);
    }
}