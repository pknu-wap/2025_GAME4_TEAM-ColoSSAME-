using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//날짜 계산(1월 31일 넘어가면 2월 1일 변경)

public class DateCal : MonoBehaviour
{   
    //GameObject.Find("DataSaver").GetComponent<Data>().month;
    //GameObject.Find("DataSaver").GetComponent<Data>().date;
    List<int> Month1 = new List<int> {1,3,5,7,8,10};
    List<int> Month2 = new List<int> {4,6,9,11};

    public void next()
    {
        GameObject.Find("DataSaver").GetComponent<Data>().date += 7;
        if (Month1.Contains(GameObject.Find("DataSaver").GetComponent<Data>().month) && GameObject.Find("DataSaver").GetComponent<Data>().date >= 32)
        {
            GameObject.Find("DataSaver").GetComponent<Data>().month += 1;
            GameObject.Find("DataSaver").GetComponent<Data>().date -= 31;
        }
        else if (Month2.Contains(GameObject.Find("DataSaver").GetComponent<Data>().month) && GameObject.Find("DataSaver").GetComponent<Data>().date >= 31)
        {
            GameObject.Find("DataSaver").GetComponent<Data>().month += 1;
            GameObject.Find("DataSaver").GetComponent<Data>().date -= 30;
        }
        else if (GameObject.Find("DataSaver").GetComponent<Data>().month == 2 && GameObject.Find("DataSaver").GetComponent<Data>().date >= 29)
        {
            GameObject.Find("DataSaver").GetComponent<Data>().month += 1;
            GameObject.Find("DataSaver").GetComponent<Data>().date -= 28;
        }
        if (GameObject.Find("DataSaver").GetComponent<Data>().month >= 12 && GameObject.Find("DataSaver").GetComponent<Data>().date >= 32)
        {
             GameObject.Find("DataSaver").GetComponent<Data>().month = 1;
            GameObject.Find("DataSaver").GetComponent<Data>().date -= 31;
        }
    }
}
