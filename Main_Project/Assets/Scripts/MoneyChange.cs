using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
//돈 표시

public class MoneyChange : MonoBehaviour
{   
    //텍스트 지정
    public TextMeshProUGUI moneyText;

    void Update()
    {
        moneyText.text=$"돈 : {GameObject.Find("DataSaver").GetComponent<Money>().money}원";
    }
}
