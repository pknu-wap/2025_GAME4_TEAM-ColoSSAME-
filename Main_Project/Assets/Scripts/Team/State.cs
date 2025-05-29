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
using Battle.Scripts.Ai;
using Battle.Scripts.Value.Data;

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

    public BattleAI[] statechange;

   // public RandomCharacter randomcharacter;
    //public SaveManager savemanage;
    public CharacterID characterid;
    public SaveManager saver;

    public string targetTag = "Player"; // 저장 대상 태그 ("Player" or "Enemy")
    private string SaveFileName => $"{targetTag}Save.json";
    private string savePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    

    void Start()
    {
        //randomcharacter = GetComponent<RandomCharacter>();
        //savemanage = GetComponent<SaveManager>();

        //randomcharacter.Randomize();

        
        //savemanage.Save(savemanage.SaveFromButton());
        CharacterData loaded = Load();
        

        trainSelect = trainSelect.ConvertAll(x => 0);
        trainSelectSave = trainSelectSave.ConvertAll(x => 0);        

        getPlayer = new List<int>{ Random.Range(0,11), Random.Range(0,11),Random.Range(0,11) };

        foreach (var s in statechange)
        {
            var id = s.GetComponent<CharacterID>();

            var info = loaded.characters[id.characterKey];

            s.damage = info.ATK;
            s.hp = info.CON;
            s.defense = info.DEF;
        }
        

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
        Debug.Log(statechange[fighterCount].hp);
        Debug.Log(statechange[fighterCount].defense);
        Debug.Log(statechange[fighterCount].damage);
    }
    
    public void selectWeek(int dayIndex)//훈련 날짜
    {
        day = dayIndex;
    }

    private CharacterData Load()
        {
            if (File.Exists(savePath))
            {
                string json = File.ReadAllText(savePath);
                CharacterData data = JsonConvert.DeserializeObject<CharacterData>(json);
                Debug.Log($"{targetTag} 불러오기 완료");
                return data;
            }
            Debug.LogWarning($"{targetTag} 저장 파일 없음");
            return new CharacterData();
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
        
        fighterButtons[0].GetComponent<RectTransform>().anchoredPosition = new Vector2(-90f, -15f);//선수 위치 원래대로
        fighterButtons[1].GetComponent<RectTransform>().anchoredPosition = new Vector2(50f, -15f);
        fighterButtons[2].GetComponent<RectTransform>().anchoredPosition = new Vector2(190f, -15f);
        fighterButtons[3].GetComponent<RectTransform>().anchoredPosition = new Vector2(-90f, -215f);
        fighterButtons[4].GetComponent<RectTransform>().anchoredPosition = new Vector2(50f, -215f);
        fighterButtons[5].GetComponent<RectTransform>().anchoredPosition = new Vector2(190f, -215f);
        fighterButtons[6].GetComponent<RectTransform>().anchoredPosition = new Vector2(-90f, -415f);
        fighterButtons[7].GetComponent<RectTransform>().anchoredPosition = new Vector2(50f, -415f);
        fighterButtons[8].GetComponent<RectTransform>().anchoredPosition = new Vector2(190f, -415f);
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
            if (i%3 == 0)
            {
                statechange[i/3].damage += trainResult[i];
                Debug.Log(statechange[i/3].damage);
            }
            if (i%3 == 1)
            {
                statechange[i/3].defense +=trainResult[i];
                Debug.Log(statechange[i/3].defense);
            }
            if (i%3 == 2)
            {
                statechange[i/3].hp += trainResult[i];
                Debug.Log(statechange[i/3].hp);
            }
        }


        trainSelectSave = trainSelectSave.ConvertAll(x => 0);

        trainResult = trainResult.ConvertAll(x => 0);
        fighterButtons[0].GetComponent<RectTransform>().anchoredPosition = new Vector2(-90f, -15f);//선수 위치 원래대로
        fighterButtons[1].GetComponent<RectTransform>().anchoredPosition = new Vector2(50f, -15f);
        fighterButtons[2].GetComponent<RectTransform>().anchoredPosition = new Vector2(190f, -15f);
        fighterButtons[3].GetComponent<RectTransform>().anchoredPosition = new Vector2(-90f, -215f);
        fighterButtons[4].GetComponent<RectTransform>().anchoredPosition = new Vector2(50f, -215f);
        fighterButtons[5].GetComponent<RectTransform>().anchoredPosition = new Vector2(190f, -215f);
        fighterButtons[6].GetComponent<RectTransform>().anchoredPosition = new Vector2(-90f, -415f);
        fighterButtons[7].GetComponent<RectTransform>().anchoredPosition = new Vector2(50f, -415f);
        fighterButtons[8].GetComponent<RectTransform>().anchoredPosition = new Vector2(190f, -415f);
        Debug.Log("주넘기기 완료");
    }
    public void playerGoodChoice()
    {
        playerCount += 1;

        playerState[(playerCount-1)*3] = getPlayer[0];
        playerState[(playerCount-1)*3+1] = getPlayer[1];
        playerState[(playerCount-1)*3+2] = getPlayer[2];
        
    }
}
