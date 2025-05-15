using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class State : MonoBehaviour
{
    public TextMeshProUGUI stateText;//공격력, 방어력 텍스트

    public int fighterCount;//어떤 선수 선택했는지
    public int day;//몇 일에 훈련 선택

    public List<Button> fighterButtons;//선수 지정

    public List<int> playerState;//능력치

    public List<TextMeshProUGUI> trainShow;//훈련 창에서 선택한 훈련 보여줌
    public List<int> trainSelect;//훈련 종류 저장
    public List<int> trainResult;//훈련 결과 저장
    public List<string> trainSelectSave;//훈련 선택 저장
    public List<string> trainList = new List<string>{ "공격력", "수비력", "체력" };//선수 훈련 종류

    public int playerCount = 5;//선수 몇명인지

    public TextMeshProUGUI trainResultText;//선수 훈련 값 텍스트
    public List<int> trainAdd = new List<int>{0,0,0};//훈련 더한 값 일시적 저장

    public System.Random trains = new System.Random();//선수 훈련 값 랜덤

    void Start()
    {
        trainSelect = trainSelect.ConvertAll(x => 0);
        trainSelectSave = trainSelectSave.ConvertAll(x => "공격력");
    }

    void Update()
    {
        stateText.text = $"공격력 : {playerState[(fighterCount)*3]} \n방어력 : {playerState[(fighterCount)*3+1]} \n체력 : {playerState[(fighterCount)*3+2]}";//공격력, 방어력 표시
        trainShow[0].text = $"{trainSelectSave[0+fighterCount*7]}";//훈련 할 능력치
        trainShow[1].text = $"{trainSelectSave[1+fighterCount*7]}";
        trainShow[2].text = $"{trainSelectSave[2+fighterCount*7]}";
        trainShow[3].text = $"{trainSelectSave[3+fighterCount*7]}";
        trainShow[4].text = $"{trainSelectSave[4+fighterCount*7]}";
        trainShow[5].text = $"{trainSelectSave[5+fighterCount*7]}";
        trainShow[6].text = $"{trainSelectSave[6+fighterCount*7]}";

        trainResultText.text = $"공격력 : {playerState[(fighterCount)*3]} + <color=#00ffff>{trainResult[fighterCount*3]}</color> \n방어력 : {playerState[(fighterCount)*3+1]} + <color=#00ffff>{trainResult[fighterCount*3+1]}</color>  \n체력    : {playerState[(fighterCount)*3+2]} + <color=#00ffff>{trainResult[fighterCount*3+2]}</color> ";//주넘기시 후 표시할 선수 능력치
    }

    public void fighter1()//어떤 선수를 골랐는지
    {
        fighterCount = 0;
        Debug.Log(trainResult[fighterCount*3]);
        trainAdd = trainAdd.ConvertAll(x => 0);
        fighterButtons[fighterCount].GetComponent<RectTransform>().anchoredPosition = new Vector3(-90f, -10f);
    }
    public void fighter2()
    {
        fighterCount = 1;
        trainAdd = trainAdd.ConvertAll(x => 0);
        fighterButtons[fighterCount].GetComponent<RectTransform>().anchoredPosition = new Vector3(-90f, -10f);
    }
    public void fighter3()
    {
        fighterCount = 2;
        trainAdd = trainAdd.ConvertAll(x => 0);
        fighterButtons[fighterCount].GetComponent<RectTransform>().anchoredPosition = new Vector3(-90f, -10f);
    }
    public void fighter4()
    {
        fighterCount = 3;
        trainAdd = trainAdd.ConvertAll(x => 0);
        fighterButtons[fighterCount].GetComponent<RectTransform>().anchoredPosition = new Vector3(-90f, -10f);
    }
    public void fighter5()
    {
        fighterCount = 4;
        trainAdd = trainAdd.ConvertAll(x => 0);
        fighterButtons[fighterCount].GetComponent<RectTransform>().anchoredPosition = new Vector3(-90f, -10f);
    }
    public void fighter6()
    {
        fighterCount = 5;
        trainAdd = trainAdd.ConvertAll(x => 0);
        fighterButtons[fighterCount].GetComponent<RectTransform>().anchoredPosition = new Vector3(-90f, -10f);
    }

    public void week1()//1일차 훈련
    {
        day = 0;
    }

    public void week2()//2일차 훈련
    {
        day = 1;
    }

    public void week3()//3일차 훈련
    {
        day = 2;
    }

    public void week4()//4일차 훈련
    {
        day = 3;
    }

    public void week5()//5일차 훈련
    {
        day = 4;
    }

    public void week6()//6일차 훈련
    {
        day = 5;
    }

    public void week7()//7일차 훈련
    {
        day = 6;
    }

    public void leftClick()//훈련 왼쪽 클릭
    {   
        if (trainSelect[day] == 0)
        {
            trainSelect[day] = 2;
        }
        else
        {
            trainSelect[day] -= 1;
        }
        trainSelectSave[day+fighterCount*7] = trainList[trainSelect[day]];
    }

    public void rightClick()//오른쪽 클릭
    {
           if (trainSelect[day] == 2)
        {
            trainSelect[day] = 0;
        }
        else
        {
            trainSelect[day] += 1;
        }
        trainSelectSave[day+fighterCount*7] = trainList[trainSelect[day]];
    }

    public void trainBack()
    {   
        foreach (int s in trainSelect)
        {
            trainAdd[s] += trains.Next(4);
            trainResult[s+3*(fighterCount)] = trainAdd[s];
        }
        
        trainSelect = trainSelect.ConvertAll(x => 0);//리스트 안 값 다시 0으로 변경
        trainSelectSave = trainSelectSave.ConvertAll(x => "공격력");
        
        fighterButtons[0].GetComponent<RectTransform>().anchoredPosition = new Vector2(-10f, 0f);//선수 위치 원래대로
        fighterButtons[1].GetComponent<RectTransform>().anchoredPosition = new Vector2(90f, 0f);
        fighterButtons[2].GetComponent<RectTransform>().anchoredPosition = new Vector2(-10f, -100f);
        fighterButtons[3].GetComponent<RectTransform>().anchoredPosition = new Vector2(90f, -100f);
        fighterButtons[4].GetComponent<RectTransform>().anchoredPosition = new Vector2(-10f, -200f);
        if (playerCount > 5)
        {
            fighterButtons[5].GetComponent<RectTransform>().anchoredPosition = new Vector2(90f, -200f);
        }
    }   

    public void nextWeek()//다음주로 넘김
    {
        //for (int i = 0; i < playerState.Count; i ++)
        {
           // playerState[i] += attackTrainingSave[i];//주를 넘겨 훈련량량 더해줌
        }
        //trainSelect = trainSelect.ConvertAll(x => 0);//리스트 안 값 다시 0으로 변경
        //trainSelectSave = trainSelectSave.ConvertAll(x => "공격력");
    }

    public void goHome()//넘어가기
    {
        foreach (int s in trainResult)
        {
            playerState[s] = trainResult[s];
        }
        fighterButtons[0].GetComponent<RectTransform>().anchoredPosition = new Vector2(-10f, 0f);//선수 위치 원래대로
        fighterButtons[1].GetComponent<RectTransform>().anchoredPosition = new Vector2(90f, 0f);
        fighterButtons[2].GetComponent<RectTransform>().anchoredPosition = new Vector2(-10f, -100f);
        fighterButtons[3].GetComponent<RectTransform>().anchoredPosition = new Vector2(90f, -100f);
        fighterButtons[4].GetComponent<RectTransform>().anchoredPosition = new Vector2(-10f, -200f);
    }
    public void playerGoodChoice()
    {
        playerCount += 1;
    }
}
