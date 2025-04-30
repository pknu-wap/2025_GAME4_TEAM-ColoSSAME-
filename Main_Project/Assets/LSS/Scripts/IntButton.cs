using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntButton : MonoBehaviour
{
    public int value;
    public CalculatorManager manager;
    
    public void OnClick()
    {
       manager.SetOperand(value);
    }
    
    
}