using System.Collections.Generic;
using BattleK.UI.Book;
using UnityEngine;

public class BookSpreadController : MonoBehaviour
{
    [Header("표시할 카테고리 순서 (인스펙터에서 드래그로 편집)")]
    public List<ItemCategory> categoryOrder;

    [Header("좌/우 페이지 (고정 2개)")]
    public InventoryPageBinder leftPage;
    public InventoryPageBinder rightPage;

    private class PageData
    {
        public ItemCategory category;
        public int pageIndex;
        public int pageCount;
        public List<InventorySlotData> items;
    }

    private readonly List<PageData> pages = new List<PageData>();
    private BookPage bookPage;
    private int spreadIndex;

    private void Awake()
    {
        bookPage = GetComponentInParent<BookPage>();
        if (bookPage != null) bookPage.Opened += HandlePageOpened;
    }

    private void OnDestroy()
    {
        if (bookPage != null) bookPage.Opened -= HandlePageOpened;
    }

    private void Start()
    {
        RebuildPages();
        ShowSpread(0);
    }

    // 다른 곳(상점 등)에서 구매한 아이템이 반영되도록 페이지가 열릴 때마다 다시 구성한다.
    private void HandlePageOpened()
    {
        RebuildPages();
        ShowSpread(spreadIndex);
    }

    public void ShowSpread(int index)
    {
        spreadIndex = Mathf.Clamp(index, 0, MaxSpreadIndex);

        ShowPage(leftPage, spreadIndex * 2);
        ShowPage(rightPage, spreadIndex * 2 + 1);
    }

    public void NextSpread()
    {
        if (spreadIndex < MaxSpreadIndex) ShowSpread(spreadIndex + 1);
    }

    public void PrevSpread()
    {
        if (spreadIndex > 0) ShowSpread(spreadIndex - 1);
    }

    private int MaxSpreadIndex =>
        pages.Count == 0 ? 0 : Mathf.CeilToInt(pages.Count / 2f) - 1;

    private void ShowPage(InventoryPageBinder binder, int pageNumber)
    {
        if (binder == null) return;

        if (pageNumber >= pages.Count)
        {
            binder.SetEmpty();
            return;
        }

        PageData page = pages[pageNumber];
        string label = ItemCategoryDisplay.GetName(page.category);
        if (page.pageCount > 1) label += $" ({page.pageIndex + 1}/{page.pageCount})";

        binder.ShowPage(label, page.items);
    }

    // 카테고리 순서대로, 카테고리별 아이템을 슬롯 수만큼 나눈 평면 페이지 목록을 만든다.
    private void RebuildPages()
    {
        pages.Clear();

        if (leftPage == null) return;

        if (UserManager.Instance == null || UserManager.Instance.user == null)
        {
            Debug.LogError("UserManager or user is null");
            return;
        }

        ItemDatabase itemDatabase = leftPage.itemDatabase;
        if (itemDatabase == null)
        {
            Debug.LogError("itemDatabase is null");
            return;
        }

        int slotsPerPage = leftPage.SlotCount;
        if (slotsPerPage <= 0)
        {
            Debug.LogError("BookSpreadController: 페이지에 슬롯이 없습니다.");
            return;
        }

        foreach (ItemCategory category in categoryOrder)
        {
            List<InventorySlotData> items = InventoryQueryService.GetItemsByCategory(
                UserManager.Instance.user.inventory,
                itemDatabase,
                category);

            int pageCount = Mathf.Max(1, Mathf.CeilToInt(items.Count / (float)slotsPerPage));

            for (int i = 0; i < pageCount; i++)
            {
                int start = i * slotsPerPage;
                int count = Mathf.Clamp(items.Count - start, 0, slotsPerPage);

                pages.Add(new PageData
                {
                    category = category,
                    pageIndex = i,
                    pageCount = pageCount,
                    items = items.GetRange(start, count)
                });
            }
        }
    }
}
