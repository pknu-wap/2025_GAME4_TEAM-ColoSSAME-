using System.Collections.Generic;
using UnityEngine;

public class BookSpreadController : MonoBehaviour
{
    [Header("표시할 카테고리 순서 (인스펙터에서 드래그로 편집)")]
    public List<ItemCategory> categoryOrder;

    [Header("좌/우 페이지 (고정 2개)")]
    public InventoryPageBinder leftPage;
    public InventoryPageBinder rightPage;

    private int spreadIndex;

    private void Start() => ShowSpread(0);

    public void ShowSpread(int index)
    {
        spreadIndex = index;

        int leftIndex = index * 2;
        int rightIndex = index * 2 + 1;

        if (leftIndex < categoryOrder.Count) leftPage.SetCategory(categoryOrder[leftIndex]);
        else leftPage.SetEmpty();

        if (rightIndex < categoryOrder.Count) rightPage.SetCategory(categoryOrder[rightIndex]);
        else rightPage.SetEmpty();
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
        categoryOrder.Count == 0 ? 0 : Mathf.CeilToInt(categoryOrder.Count / 2f) - 1;
}