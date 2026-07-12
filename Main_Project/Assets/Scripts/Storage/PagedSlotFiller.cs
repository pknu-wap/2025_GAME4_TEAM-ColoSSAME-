using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PagedSlotFiller<TData>
{
    private readonly IReadOnlyList<ISlotView<TData>> slots;
    private List<TData> items = new List<TData>();
    private int currentPage;

    public PagedSlotFiller(IReadOnlyList<ISlotView<TData>> slots)
    {
        this.slots = slots;
    }

    public void SetItems(List<TData> newItems)
    {
        items = newItems ?? new List<TData>();
        currentPage = 0;
        RefreshPage();
    }

    public void NextPage()
    {
        if (currentPage < MaxPage) { currentPage++; RefreshPage(); }
    }

    public void PrevPage()
    {
        if (currentPage > 0) { currentPage--; RefreshPage(); }
    }

    private int MaxPage =>
        slots.Count == 0 ? 0 : Mathf.Max(0, Mathf.CeilToInt((float)items.Count / slots.Count) - 1);

    private void RefreshPage()
    {
        int start = currentPage * slots.Count;

        for (int i = 0; i < slots.Count; i++)
        {
            int idx = start + i;
            if (idx < items.Count) slots[i].SetItem(items[idx]);
            else slots[i].Clear();
        }
    }
}
