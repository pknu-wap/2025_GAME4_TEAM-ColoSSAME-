using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//날짜 계산(1월 31일 넘어가면 2월 1일 변경)

public class DateCal : MonoBehaviour
{   
    List<int> Month1 = new List<int> {1,3,5,7,8,10};//월 리스트(31일까지 있는)
    List<int> Month2 = new List<int> {4,6,9,11};//월 리스트트(30일까지 있는)
    
    public Text dateText;//텍스트 지정

    public int month;//월 변수 선언
    public int date;//일 변수 선언

    void Start()
    {
        month = GameObject.Find("DataSaver").GetComponent<Data>().month;//월 가져옴
        date = GameObject.Find("DataSaver").GetComponent<Data>().date;//일 가져옴
    }
    
    void Update()//날짜 보여줌
    {
        dateText.text = $"{month}월 {date}일";
    }


    public void next()
    {
        date += 7;//주 넘기기 시 7일추가
        //(아래 코드) 월에 따라 30,31일 등이 되면 월이 바뀜
        if (Month1.Contains(month) && date >= 32)//1,3,5,7,8,10월 일 때, 31일 까지
        {
            month += 1;
            date -= 31;
        }
        else if (Month2.Contains(month) && date >= 31)//4,6,9,11월 일 때, 30일까지
        {
            month += 1;
            date -= 30;
        }
        else if (month == 2 && date >= 29)//2월 일 때 28일 까지, 3월 변경
        {
            month += 1;
            date -= 28;
        }
        if (month >= 12 && date >= 32)//12월 넘어갈 경우 1월로 바꿈꿈
        {
            month = 1;
            date -= 31;
        }
    }
}
