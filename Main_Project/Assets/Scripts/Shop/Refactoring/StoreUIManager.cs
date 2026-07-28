using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StoreUIManager : MonoBehaviour
{
    [Header("DB")]
    public ItemDatabase itemDatabase;

    [Header("슬롯 UI")]
    public StoreItemUI leftSlot;
    public StoreItemUI rightSlot;

    [Header("기본 카테고리")]
    public ItemCategory defaultCategory;

    private PagedSlotFiller<ItemData> pager;

    private void Awake()
    {
        pager = new PagedSlotFiller<ItemData>(new List<ISlotView<ItemData>> { leftSlot, rightSlot });
    }

    private void Start() => SelectCategory(defaultCategory);

    public void SelectCategory(ItemCategory category)
    {
        var items = itemDatabase.items.Where(x => x.category == category).ToList();
        pager.SetItems(items);
    }

    public void NextPage() => pager.NextPage();
    public void PrevPage() => pager.PrevPage();
}