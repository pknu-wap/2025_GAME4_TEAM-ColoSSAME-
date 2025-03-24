using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
//돈 표시

public class DataChange : MonoBehaviour
{   
    //텍스트 지정
    public TextMeshProUGUI DataText;

    void Update()
    {
        DataText.text=$"돈 : {GameObject.Find("DataSaver").GetComponent<Data>().money}원";
    }
}
