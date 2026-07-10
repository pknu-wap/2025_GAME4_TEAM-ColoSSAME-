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

    private List<ItemData> currentItems = new List<ItemData>();

    private int currentPage = 0;

    private const int ITEMS_PER_PAGE = 2;
    
    private ItemCategory defaultCategory;

    private void Start()
    {
        SelectCategory(defaultCategory);
    }
    
    // 카테고리 선택
    public void SelectCategory(ItemCategory category)
    {
        currentPage = 0;

        currentItems = itemDatabase.items
            .Where(x => x.category == category)
            .ToList();

        RefreshPage();
    }

    /// 다음 페이지
    public void NextPage()
    {
        int maxPage = Mathf.CeilToInt((float)currentItems.Count / ITEMS_PER_PAGE) - 1;

        if (currentPage < maxPage)
        {
            currentPage++;
            RefreshPage();
        }
        
        Debug.Log($"[StoreUIManager] NextPage -> Current Page : {currentPage}");

    }

    /// 이전 페이지
    public void PrevPage()
    {
        int maxPage = Mathf.CeilToInt((float)currentItems.Count / ITEMS_PER_PAGE) - 1;

        if (currentPage > 0)
        {
            currentPage--;
            RefreshPage();
        }
        
        Debug.Log($"[StoreUIManager] PrevPage -> Current Page : {currentPage}");

    }

    /// 현재 페이지 UI 갱신
    private void RefreshPage()
    {
        int startIndex = currentPage * ITEMS_PER_PAGE;

        // Left
        if (startIndex < currentItems.Count)
            leftSlot.SetItem(currentItems[startIndex]);
        else
            leftSlot.SetItem(null);

        // Right
        if (startIndex + 1 < currentItems.Count)
            rightSlot.SetItem(currentItems[startIndex + 1]);
        else
            rightSlot.SetItem(null);
    }
}