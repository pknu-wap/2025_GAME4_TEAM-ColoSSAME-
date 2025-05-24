using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntButton : MonoBehaviour
{
    private int value;
    public CalculatorManager manager;
    
    public void OnClick()
    { 
        value = Random.Range(100, 1001);
       manager.SetOperand(value);
    }
    
    
}