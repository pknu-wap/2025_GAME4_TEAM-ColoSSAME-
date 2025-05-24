using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OperatorButton : MonoBehaviour
{
   public char operatorSymbol;
   public CalculatorManager manager;

   public void OnClick()
   {
      char[] operators = { '+', '-', '*' };
      int index = Random.Range(0, operators.Length);
      operatorSymbol = operators[index];
      
      manager.SetOperator(operatorSymbol);
   }
}
