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
using Battle.Scripts.ImageManager;

//battle ai 값바꾸기(느림)
//json직접 변경(난이도 높음, 그러나 빠름)

public class State : MonoBehaviour
{
    public TextMeshProUGUI stateText;//공격력, 방어력 텍스트

    public int fighterCount;//어떤 선수 선택했는지
    int day;//몇 일에 훈련 선택

    public List<Button> fighterButtons;//선수 지정
    public List<Button> enemyButtons;

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
    
    public GameObject playerStatusText;
    public GameObject enemyStatusText;

    public SaveManager savemanage; 
    public SaveManager enemysavemanage;
    public TransparentScreenshot changeimage;

    public BattleAI[] statechange;

    public MoneyManager moneymanager;

    float hpresult;
    float damageresult;
    float defenseresult;

   // public RandomCharacter randomcharacter;
    //public SaveManager savemanage;
    public CharacterID characterid;
    public TextMeshProUGUI trainResultInfoText;
    public List<string> trainResultSave;
    List<string> trainResultInfo = new List<string> {"좋아, 몸이 반응하고 있어", "훈련의 성과가 나타나기 시작했다", "이게 바로 피와 땀의 결과지", "전보다 나아졌다… 확실히", "어깨가 가벼워진 느낌이야", "검이 내 일부가 되는 느낌이다", "지금 뭔가, 벽을 넘었다", "이걸로 전장을 바꿀 수 있겠군", "내 한계를 깨부쉈다", "이제 누구든 상대할 수 있어", "몸이, 가벼워… 아니, 날카로워졌어", "예상대로야", "역시 난 특별하지", "천천히, 하지만 확실히", "봤지?! 나 늘었다고!", "강해진다", "이런 게 진짜 전진이지", "이 감각… 오랜만이야", "한 걸음 더 다가갔다", "이 속도면 꼭대기까지 간다", "피가 끓어오른다", "날 막을 자는 없다", "몸이 기억하고 있어", "훈련이 아니라 각성이라고 해야겠군", "기술이 완성되어간다", "움직임이 정제되고 있어", "검이 날 따라온다", "집중이 달라졌어", "이 정도면 실전에서도 통하겠군", "적들이 나를 두려워하겠지", "이것이 내 길이다", "이대로라면 우승도 꿈이 아냐", "다시 태어난 기분이다", "경쾌하다… 좋아", "다음 단계로 가자", "강해졌다는 게 느껴져"};
    List<string> trainBadResultInfo = new List<string> {"제길, 아직 부족한 건가…", "생각만큼 늘진 않았군", "몸이 따라주질 않아…", "타격감이 흐트러졌어", "오늘은 이만큼이 한계인가", "이대로는 안 돼… 다시다!", "내가 이런 곳에서 주저앉을 줄 알아?!", "부족하다. 다시 처음부터다", "이건… 치욕이야", "이 손으로 뭘 하겠다는 건지… 모르겠다", "오차가 생겼군. 수정이 필요해", "실수도, 핑계도 필요 없는 실패야", "조급했나… 아니, 어쩌면 애초에 틀린 걸지도", "젠장… 다시 해!", "힘이 새는 느낌이야", "리듬이 안 맞아", "내 한계가 이 정도일 리 없어", "고작 이거냐", "내 훈련이 헛수고였던 건가", "집중조차 안 되는 걸 보면 끝난 건가", "너무 안일했군", "결과가 실망스럽군", "컨디션이 나쁘군", "이건 그냥 실패가 아니야", "수치스럽군", "다잡았는데 놓쳤어", "감각이 무뎌졌어", "머릿속이 복잡해", "집중이 흐트러졌군", "너무 급했다", "기본부터 다시 다져야겠어", "이래선 이길 수 없어", "시간이 부족했다", "아무리 해도 안 되는 건, 나 자신일지도"};
    List<string> trainInfo = new List<string>{"안녕, 전사여. 오늘도 싸울 준비는 되어 있겠지?", "반갑군. 함께 전장을 누비게 될 줄이야.", "좋은 날이군. 검이 근질거린다.", "또 만났군. 이번에도 내가 앞장서지.", "만나서 반갑다. 적들은 운이 없겠군.", "안녕이다. 오늘은 누가 쓰러질 차례지?", "오랜만이군. 그동안 검을 녹슬게 한 건 아니겠지?", "좋은 아침이다. 죽이기 딱 좋은 시간이지.", "인사하지. 곧 피로 물들 테니까.", "반가워. 오늘도 피의 무도회를 시작하지.", "살아 있군, 그것만으로 충분하지 않은가?", "또 봤군. 검이 널 기억하고 있다.", "왔는가, 전우여. 싸움은 이미 시작됐다.", "잘 왔다. 네 힘이 필요하다.", "안녕하신가. 검도 인사를 나누고 싶어 한다.", "좋은 얼굴이군. 오늘은 죽지 않을 거 같아.", "반갑다. 함께라면 어떤 적도 두렵지 않지.", "또 네 얼굴을 보게 되는군. 마음이 놓인다.", "인연인가? 다시 만났군.", "안녕. 목숨은 붙어 있나?"};

    void Start()
    {
        //randomcharacter = GetComponent<RandomCharacter>();
        //savemanage = GetComponent<SaveManager>();

        //randomcharacter.Randomize();

        
        savemanage.LoadFromButton();

        enemysavemanage.LoadFromButton();

        changeimage.LoadAllSprites();        

        trainSelect = trainSelect.ConvertAll(x => 0);
        trainSelectSave = trainSelectSave.ConvertAll(x => 0);

        characterid = GetComponent<CharacterID>();
        getPlayer = new List<int>{ Random.Range(0,11), Random.Range(0,11),Random.Range(0,11) };

        for (int i = 0; i < 9; i++)
        {
            int stresult = Random.Range(0, trainInfo.Count);
            trainResultSave[i] = trainInfo[stresult];
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

        trainResultInfoText.text = $"{trainResultSave[fighterCount]}";

        trainResultText.text = $"공격력 : {playerState[(fighterCount)*3]} + <color=#00ffff>{trainResult[fighterCount*3]}</color> \n방어력 : {playerState[(fighterCount)*3+1]} + <color=#00ffff>{trainResult[fighterCount*3+1]}</color>  \n체력    : {playerState[(fighterCount)*3+2]} + <color=#00ffff>{trainResult[fighterCount*3+2]}</color> ";//주넘기시 후 표시할 선수 능력치
    
        getPlayerStateText.text = $"공격력 : {getPlayer[0]} \n방어력 : {getPlayer[1]} \n체력 : {getPlayer[2]}";//공격력, 방어력 표시
    }

    public void selectPlayer(int playerIndex)//선수 선택
    {
        fighterCount = playerIndex;
        trainAdd = trainAdd.ConvertAll(x => 0);
        fighterButtons[fighterCount].GetComponent<RectTransform>().anchoredPosition = new Vector2(-40f, 10f);
        enemyButtons[fighterCount].GetComponent<RectTransform>().anchoredPosition = new Vector2(-40f, -20f);
        fighterButtons[fighterCount].GetComponent<RectTransform>().localScale = new Vector2(3f,3f);
        enemyButtons[fighterCount].GetComponent<RectTransform>().localScale = new Vector2(-3f,3f);
        playerStatusText.GetComponentInChildren<CharacterID>().characterKey = (fighterCount + 1).ToString();
        enemyStatusText.GetComponentInChildren<CharacterID>().characterKey = (fighterCount + 1).ToString();
        Debug.Log(statechange[fighterCount].hp);
        Debug.Log(statechange[fighterCount].defense);
        Debug.Log(statechange[fighterCount].damage);
    }

    public void playerRandomState(int trainschoice)
    {
        if (trainschoice == 0 && moneymanager.money > 100)
        {
            hpresult= Random.Range(-2.5f,3f);
            if (hpresult >= 0)
            {   
                int index = Random.Range(0, trainResultInfo.Count);
                trainResultSave[fighterCount] =  trainResultInfo[index];
            }
            if (hpresult < 0)
            {   
                int index = Random.Range(0, trainBadResultInfo.Count);
                trainResultSave[fighterCount] =  trainBadResultInfo[index];
            }
            moneymanager.AddMoney(-100);
            statechange[fighterCount].hp = Mathf.Round((statechange[fighterCount].hp+hpresult) * 10f) / 10f;
            if (statechange[fighterCount].hp < 1)
            {
                statechange[fighterCount].hp = 1;
            }
        }
        if (trainschoice == 1 && moneymanager.money > 100)
        {

            damageresult= Random.Range(-2.5f,3f);
            if (damageresult >= 0)
            {   
                int index = Random.Range(0, trainResultInfo.Count);
                trainResultSave[fighterCount] =  trainResultInfo[index];
            }
            if (damageresult < 0)
            {   
                int index = Random.Range(0, trainBadResultInfo.Count);
                trainResultSave[fighterCount] =  trainBadResultInfo[index];
            }
            moneymanager.AddMoney(-100);
            statechange[fighterCount].damage = Mathf.Round((statechange[fighterCount].damage+damageresult) * 10f) / 10f;
            if (statechange[fighterCount].damage < 1)
            {
                statechange[fighterCount].damage = 1;
            }
        }
        if (trainschoice == 2 && moneymanager.money > 100)
        {

           defenseresult = Random.Range(-2.5f,3f);
            if (defenseresult >= 0)
            {   
                int index = Random.Range(0, trainResultInfo.Count);
                trainResultSave[fighterCount] =  trainResultInfo[index];
            }
            if (damageresult < 0)
            {   
                int index = Random.Range(0, trainBadResultInfo.Count);
                trainResultSave[fighterCount] =  trainBadResultInfo[index];
            }
            moneymanager.AddMoney(-100);
            statechange[fighterCount].defense = Mathf.Round((statechange[fighterCount].defense+defenseresult) * 10f) / 10f;
            if (statechange[fighterCount].defense < 1)
            {
                statechange[fighterCount].defense = 1;
            }
        }
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
        fighterButtons[fighterCount].GetComponent<RectTransform>().localScale = new Vector2(1f,1f);
        enemyButtons[fighterCount].GetComponent<RectTransform>().localScale = new Vector2(1f,1f); 
        
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

        enemyButtons[0].GetComponent<RectTransform>().anchoredPosition = new Vector2(-90f, -15f);//선수 위치 원래대로
        enemyButtons[1].GetComponent<RectTransform>().anchoredPosition = new Vector2(50f, -15f);
        enemyButtons[2].GetComponent<RectTransform>().anchoredPosition = new Vector2(190f, -15f);
        enemyButtons[3].GetComponent<RectTransform>().anchoredPosition = new Vector2(-90f, -215f);
        enemyButtons[4].GetComponent<RectTransform>().anchoredPosition = new Vector2(50f, -215f);
        enemyButtons[5].GetComponent<RectTransform>().anchoredPosition = new Vector2(190f, -215f);
        enemyButtons[6].GetComponent<RectTransform>().anchoredPosition = new Vector2(-90f, -415f);
        enemyButtons[7].GetComponent<RectTransform>().anchoredPosition = new Vector2(50f, -415f);
        enemyButtons[8].GetComponent<RectTransform>().anchoredPosition = new Vector2(190f, -415f);
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
