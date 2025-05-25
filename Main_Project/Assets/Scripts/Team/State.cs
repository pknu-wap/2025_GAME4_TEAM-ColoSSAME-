using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using Battle.Scripts.Value.Data;
using UnityEngine.EventSystems;

//battle ai 값바꾸기(느림)
//json직접 변경(난이도 높음, 그러나 빠름)

public class State : MonoBehaviour
{
    public TextMeshProUGUI stateText;//공격력, 방어력 텍스트

    public int fighterCount;//어떤 선수 선택했는지
    int day;//몇 일에 훈련 선택

    public List<Button> fighterButtons;//선수 지정

    public List<int> playerState;//능력치

    public List<TextMeshProUGUI> trainShow;//훈련 창에서 선택한 훈련 보여줌
    public List<int> trainSelect;//훈련 종류 저장
    public List<int> trainResult;//훈련 결과 저장
    public List<int> trainSelectSave;//훈련 선택 저장
    public List<string> trainList = new List<string>{ "공격력", "수비력", "체력" };//선수 훈련 종류

    public List<int> getPlayer;//선수 영입 능력치
    public TextMeshProUGUI getPlayerStateText;//영입에 뜨는 선수 능력치 텍스트 표시시

    public int playerCount = 9;//선수 몇명인지

    public TextMeshProUGUI trainResultText;//선수 훈련 값 텍스트
    public List<int> trainAdd = new List<int>{0,0,0};//훈련 더한 값 일시적 저장

    public List<int> playerRole;//선수 직업
    
    public GameObject EnemyStatusText;

    private string SaveFileName => $"playerStateSave.json";
    private string savePath => Path.Combine(Application.persistentDataPath, SaveFileName);

   // public RandomCharacter randomcharacter;
    //public SaveManager savemanage;
    public CharacterID characterid;

    void Start()
    {
        //randomcharacter = GetComponent<RandomCharacter>();
        //savemanage = GetComponent<SaveManager>();

        //randomcharacter.Randomize();

        
        //savemanage.Save(savemanage.SaveFromButton());

        trainSelect = trainSelect.ConvertAll(x => 0);
        trainSelectSave = trainSelectSave.ConvertAll(x => 0);

        playerRole = new List<int>{ Random.Range(0,4), Random.Range(0,4), Random.Range(0,4), Random.Range(0,4), Random.Range(0,4),Random.Range(0,4),Random.Range(0,4),Random.Range(0,4),Random.Range(0,4)};//선수 직업

        for (int i = 0; i < 9; i++)
        {
            if (playerRole[i] == 0)//전사
            {
                playerState[i*3] = Random.Range(1,10);//공격력
                playerState[i*3+1] = Random.Range(1,10);//방어력
                playerState[i*3+2] = Random.Range(50,100);//체체력
            }
            if (playerRole[i] == 1)//도적
            {
                playerState[i*3] = Random.Range(1,10);//공격력
                playerState[i*3+1] = Random.Range(1,10);//방어력
                playerState[i*3+2] = Random.Range(50,100);//체체력
            }
            if (playerRole[i] == 2)//궁수
            {
                playerState[i*3] = Random.Range(1,10);//공격력
                playerState[i*3+1] = Random.Range(1,10);//방어력
                playerState[i*3+2] = Random.Range(30,50);//체체력
            }
            if (playerRole[i] == 3)//마법사
            {
                playerState[i*3] = Random.Range(1,10);//공격력
                playerState[i*3+1] = Random.Range(1,10);//방어력
                playerState[i*3+2] = Random.Range(30,50);//체체력
            }
        }

        getPlayer = new List<int>{ Random.Range(0,11), Random.Range(0,11),Random.Range(0,11) };

        string json = JsonConvert.SerializeObject(playerState, Formatting.Indented);
        File.WriteAllText(savePath, json);

        Debug.Log(savePath);
    }

    void Update()
    {
        //stateText.text = $"공격력 : {playerState[(fighterCount)*3]} \n방어력 : {playerState[(fighterCount)*3+1]} \n체력 : {playerState[(fighterCount)*3+2]}";//공격력, 방어력 표시
        trainShow[0].text = $"{trainList[trainSelectSave[0+fighterCount*7]]}";//훈련 할 능력치
        trainShow[1].text = $"{trainList[trainSelectSave[1+fighterCount*7]]}";
        trainShow[2].text = $"{trainList[trainSelectSave[2+fighterCount*7]]}";
        trainShow[3].text = $"{trainList[trainSelectSave[3+fighterCount*7]]}";
        trainShow[4].text = $"{trainList[trainSelectSave[4+fighterCount*7]]}";
        trainShow[5].text = $"{trainList[trainSelectSave[5+fighterCount*7]]}";
        trainShow[6].text = $"{trainList[trainSelectSave[6+fighterCount*7]]}";
        trainResultText.text = $"공격력 : {playerState[(fighterCount)*3]} + <color=#00ffff>{trainResult[fighterCount*3]}</color> \n방어력 : {playerState[(fighterCount)*3+1]} + <color=#00ffff>{trainResult[fighterCount*3+1]}</color>  \n체력    : {playerState[(fighterCount)*3+2]} + <color=#00ffff>{trainResult[fighterCount*3+2]}</color> ";//주넘기시 후 표시할 선수 능력치
    
        getPlayerStateText.text = $"공격력 : {getPlayer[0]} \n방어력 : {getPlayer[1]} \n체력 : {getPlayer[2]}";//공격력, 방어력 표시
    }

    public void selectPlayer(int playerIndex)//선수 선택
    {
        fighterCount = playerIndex;
        trainAdd = trainAdd.ConvertAll(x => 0);
        fighterButtons[fighterCount].GetComponent<RectTransform>().anchoredPosition = new Vector3(-90f, -10f);
        EnemyStatusText.GetComponentInChildren<CharacterID>().characterKey = (fighterCount + 1).ToString();
    }
    
    public void selectWeek(int dayIndex)//훈련 날짜
    {
        day = dayIndex;
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
        trainSelectSave[day+fighterCount*7] = trainSelect[day];

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
        trainSelectSave[day+fighterCount*7] = trainSelect[day];
    }

    public void trainBack()
    {   
        foreach (int s in trainSelect)
        {
            trainAdd[s] += Random.Range(0,4);
            trainResult[s+3*(fighterCount)] = trainAdd[s];
        }
        
        trainSelect = trainSelect.ConvertAll(x => 0);//리스트 안 값 다시 0으로 변경
        
        fighterButtons[0].GetComponent<RectTransform>().anchoredPosition = new Vector2(-10f, 0f);//선수 위치 원래대로
        fighterButtons[1].GetComponent<RectTransform>().anchoredPosition = new Vector2(90f, 0f);
        fighterButtons[2].GetComponent<RectTransform>().anchoredPosition = new Vector2(-10f, -100f);
        fighterButtons[3].GetComponent<RectTransform>().anchoredPosition = new Vector2(90f, -100f);
        fighterButtons[4].GetComponent<RectTransform>().anchoredPosition = new Vector2(-10f, -200f);
        if (playerCount > 5)
        {
            fighterButtons[5].GetComponent<RectTransform>().anchoredPosition = new Vector2(90f, -200f);
        }
        if (playerCount > 6)
        {
            fighterButtons[6].GetComponent<RectTransform>().anchoredPosition = new Vector2(10f, -300f);
        }
        if (playerCount > 7)
        {
            fighterButtons[7].GetComponent<RectTransform>().anchoredPosition = new Vector2(90f, -300f);
        }
        if (playerCount > 8)
        {
            fighterButtons[8].GetComponent<RectTransform>().anchoredPosition = new Vector2(-10f, -400f);
        }
        if (playerCount > 9)
        {
            fighterButtons[9].GetComponent<RectTransform>().anchoredPosition = new Vector2(90f, -400f);
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
        for (int i = 0; i < trainResult.Count; i++)
        {
            playerState[i] += trainResult[i];
        }

        trainSelectSave = trainSelectSave.ConvertAll(x => 0);

        trainResult = trainResult.ConvertAll(x => 0);
        fighterButtons[0].GetComponent<RectTransform>().anchoredPosition = new Vector2(-10f, 0f);//선수 위치 원래대로
        fighterButtons[1].GetComponent<RectTransform>().anchoredPosition = new Vector2(90f, 0f);
        fighterButtons[2].GetComponent<RectTransform>().anchoredPosition = new Vector2(-10f, -100f);
        fighterButtons[3].GetComponent<RectTransform>().anchoredPosition = new Vector2(90f, -100f);
        fighterButtons[4].GetComponent<RectTransform>().anchoredPosition = new Vector2(-10f, -200f);
    }
    public void playerGoodChoice()
    {
        playerCount += 1;

        playerState[(playerCount-1)*3] = getPlayer[0];
        playerState[(playerCount-1)*3+1] = getPlayer[1];
        playerState[(playerCount-1)*3+2] = getPlayer[2];
        
    }
}
