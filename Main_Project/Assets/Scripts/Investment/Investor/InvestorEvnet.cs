using UnityEngine;
using System;

public class InvestorEvent : MonoBehaviour
{
    public Action OnInvestorDone;

    public void FinishPuzzle()
    {
        OnInvestorDone?.Invoke();
    }
}