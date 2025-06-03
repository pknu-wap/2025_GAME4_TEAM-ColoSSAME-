using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    [SerializeField] public int money = 0;
    [SerializeField] private TextMeshProUGUI moneyText;

    [SerializeField] private CalculatorManager[] resultValues;
    
    private HashSet<CalculatorManager> alreadyAdded = new HashSet<CalculatorManager>(); // 중복 방지용
    private void Start()
    {
        Debug.Log("eneterMoney.cs");
        UpdateMoneyUI();
    }
    public void AddMoneyFromCalculators()
    {
        Debug.Log("AddMoneyFromCalculators");
        foreach (CalculatorManager calc in resultValues)
        {
            if (calc != null && calc.CanAddMoney())
            {
                AddMoney(calc.result);
                calc.MarkAsAdded(); // 오브젝트가 직접 중복 처리하도록 위임
            }
        }
    }
    public void AddMoney(int amount)
    {
        Debug.Log("In AddMoney");
        money += amount;
        Debug.Log("현재 소지금: " + money);
        UpdateMoneyUI();
        
    }
    private void UpdateMoneyUI()
    {
        if (moneyText != null)
        {
            moneyText.text = "₩ " + money.ToString("N0"); // 'N'은 숫자 형식, '0'은 소수점 0자리
        }
    }
}