using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
//돈 표시

public class MoneyChange : MonoBehaviour
{   
    //텍스트 지정
    public TextMeshProUGUI DataText;
    
    void Start()
    {
        //money = GameObject.Find("DataSaver").GetComponent<Data>().money;
    }

    void Update()
    {
        //DataText.text=$"돈 : {money}원";
    }
}
