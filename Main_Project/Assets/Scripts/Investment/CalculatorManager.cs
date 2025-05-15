using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
public class CalculatorManager : MonoBehaviour
{
    public int firstOperand;
    public int secondOperand;
    public string operatorSymbol;
    public int result;

    public bool isFirstSet = false;
    public bool isSecondSet = false;
    public bool isOperatorSet = false;

    public InvestorEvent investorRef;
    
    public TextMeshProUGUI resultText;
    
    private bool hasAddedMoney = false;
    
    [SerializeField] private MoneyManager moneyManager;
    public void SetOperand(int value){
        Debug.Log("SetOperand 함수 들어옴");
        if (!isFirstSet)
        {
            firstOperand = value;
            isFirstSet = true;
            Debug.Log("첫 번째 숫자 : " + firstOperand);
        }
        else if(!isSecondSet){
            secondOperand = value;
            isSecondSet = true;
            Debug.Log("두 번째 숫자 : " +  secondOperand);
        }
    }
    public void SetOperator(string op)
    {
        operatorSymbol = op;
        isOperatorSet = true;
        Debug.Log("연산자 : " + operatorSymbol);
        Debug.Log("현재 TMP 객체 이름: " + resultText.name);
        Calculate();
    }
    public void Calculate()
    {
        if (isFirstSet && isSecondSet && isOperatorSet)
        {
            switch (operatorSymbol)
            {
                case "+": result = firstOperand + secondOperand; break;
                case "-": result = firstOperand - secondOperand; break;
                case "*": result = firstOperand * secondOperand; break;
                case "/": result = firstOperand / secondOperand; break;
            } // first, second 정수에 '0'은 들어가지 않으므로 그냥 나눈다.
        }

        resultText.text = "Result : " + result;
        Debug.Log("결과 : " + result);
        
        isFirstSet = false;
        isSecondSet = false;
        isOperatorSet = false;
        hasAddedMoney = false;
        
        if (CanAddMoney() && moneyManager != null)
        {
            moneyManager.AddMoney(result);
            MarkAsAdded();
        }

    }
    public bool CanAddMoney()
    {
        return !hasAddedMoney;
    }

    public void MarkAsAdded()
    {
        hasAddedMoney = true;
    }

    public void ResetStatus()
    {
        hasAddedMoney = false;
    }
    public void OnPuzzleComplete()
    {
        investorRef.FinishPuzzle(); // 👈 완료 신호 보냄
    }
}
