using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISlotView<TData>
{
    void SetItem(TData data);
    void Clear();
}