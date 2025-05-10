using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OperatorButton : MonoBehaviour
{
   public string operatorSymbol;
   public CalculatorManager manager;

   public void OnClick()
   {
      manager.SetOperator(operatorSymbol);
   }
}
