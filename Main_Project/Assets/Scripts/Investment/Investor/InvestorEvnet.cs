using UnityEngine;
using System;
using Unity.VisualScripting;

public class InvestorEvent : MonoBehaviour
{
    public Action OnInvestorDone;
    public void FinishPuzzle()
    {
        OnInvestorDone?.Invoke();
    }
    
}