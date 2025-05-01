using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//날짜 표시

public class Date : MonoBehaviour
{
    public Text dateText;
    void Update()
    {
        dateText.text=$"{GameObject.Find("DataSaver").GetComponent<Money>().month}월 {GameObject.Find("DataSaver").GetComponent<Money>().date}일";
    }
}
