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
using Battle.Scripts.Ai.CharacterCreator; 
using Scripts.Team.DeathManage;
using Scripts.Team.FighterRandomBuy;

//battle ai 값바꾸기(느림)
//json직접 변경(난이도 높음, 그러나 빠름)
namespace Scripts.Team.Fightermanage
{
    public class State : MonoBehaviour
    {
        public TextMeshProUGUI LevelText;
        string LevelTextContain = "검투사 레벨업";

        public int fighterCount;//어떤 선수 선택했는지

        public List<Button> fighterButtons;//선수 지정
        public List<Button> enemyButtons;

        public int playerCount = 9;//선수 몇명인지

        public GameObject playerStatusText;
        public GameObject enemyStatusText;

        List<int> LevelList = new List<int>{0,0,0,0,0,0,0,0,0};

        public SaveManager savemanage; 
        public SaveManager enemysavemanage;
        public TransparentScreenshot changeimage;
        public TransparentScreenshot trainimage;
        public TransparentScreenshot buyimage;

        public TransparentScreenshot deathchangeimage;

        public DeathSave deathsave;

        public BattleAI[] statechange;
        public RandomCharacter[] randomfighter;

        public MoneyManager moneymanager;

        float hpresult;
        float damageresult;
        float defenseresult;

        int deathCount;
        public List<int> deathList;
        public List<GameObject> deathfighterimage; 
        public List<Button> deathfighter;

        int sellCount;

        public int deleteCount;
        public List<int> deleteList;

        int playerCheck;
        public List<int> sellList;

        public List<GameObject> playerimage; 
        public List<int> playerIndexList = new List<int>{0,1,2,3,4,5,6,7,8};
        public List<int> playerIdLIst = new List<int> {0,1,2,3,4,5,6,7,8,9};

        public CharacterID characterid;
        public TextMeshProUGUI trainResultInfoText;
        public List<string> trainResultSave;
        List<string> trainResultInfo = new List<string> {"좋아, 몸이 반응하고 있어", "훈련의 성과가 나타나기 시작했다", "이게 바로 피와 땀의 결과지", "전보다 나아졌다… 확실히", "어깨가 가벼워진 느낌이야", "검이 내 일부가 되는 느낌이다", "지금 뭔가, 벽을 넘었다", "이걸로 전장을 바꿀 수 있겠군", "내 한계를 깨부쉈다", "이제 누구든 상대할 수 있어", "몸이, 가벼워… 아니, 날카로워졌어", "예상대로야", "역시 난 특별하지", "천천히, 하지만 확실히", "봤지?! 나 늘었다고!", "강해진다", "이런 게 진짜 전진이지", "이 감각… 오랜만이야", "한 걸음 더 다가갔다", "이 속도면 꼭대기까지 간다", "피가 끓어오른다", "날 막을 자는 없다", "몸이 기억하고 있어", "훈련이 아니라 각성이라고 해야겠군", "기술이 완성되어간다", "움직임이 정제되고 있어", "검이 날 따라온다", "집중이 달라졌어", "이 정도면 실전에서도 통하겠군", "적들이 나를 두려워하겠지", "이것이 내 길이다", "이대로라면 우승도 꿈이 아냐", "다시 태어난 기분이다", "경쾌하다… 좋아", "다음 단계로 가자", "강해졌다는 게 느껴져"};
        List<string> trainBadResultInfo = new List<string> {"제길, 아직 부족한 건가…", "생각만큼 늘진 않았군", "몸이 따라주질 않아…", "타격감이 흐트러졌어", "오늘은 이만큼이 한계인가", "이대로는 안 돼… 다시다!", "내가 이런 곳에서 주저앉을 줄 알아?!", "부족하다. 다시 처음부터다", "이건… 치욕이야", "이 손으로 뭘 하겠다는 건지… 모르겠다", "오차가 생겼군. 수정이 필요해", "실수도, 핑계도 필요 없는 실패야", "조급했나… 아니, 어쩌면 애초에 틀린 걸지도", "젠장… 다시 해!", "힘이 새는 느낌이야", "리듬이 안 맞아", "내 한계가 이 정도일 리 없어", "고작 이거냐", "내 훈련이 헛수고였던 건가", "집중조차 안 되는 걸 보면 끝난 건가", "너무 안일했군", "결과가 실망스럽군", "컨디션이 나쁘군", "이건 그냥 실패가 아니야", "수치스럽군", "다잡았는데 놓쳤어", "감각이 무뎌졌어", "머릿속이 복잡해", "집중이 흐트러졌군", "너무 급했다", "기본부터 다시 다져야겠어", "이래선 이길 수 없어", "시간이 부족했다", "아무리 해도 안 되는 건, 나 자신일지도"};
        List<string> trainInfo = new List<string>{"안녕, 전사여. 오늘도 싸울 준비는 되어 있겠지?", "반갑군. 함께 전장을 누비게 될 줄이야.", "좋은 날이군. 검이 근질거린다.", "또 만났군. 이번에도 내가 앞장서지.", "만나서 반갑다. 적들은 운이 없겠군.", "안녕이다. 오늘은 누가 쓰러질 차례지?", "오랜만이군. 그동안 검을 녹슬게 한 건 아니겠지?", "좋은 아침이다. 죽이기 딱 좋은 시간이지.", "인사하지. 곧 피로 물들 테니까.", "반가워. 오늘도 피의 무도회를 시작하지.", "살아 있군, 그것만으로 충분하지 않은가?", "또 봤군. 검이 널 기억하고 있다.", "왔는가, 전우여. 싸움은 이미 시작됐다.", "잘 왔다. 네 힘이 필요하다.", "안녕하신가. 검도 인사를 나누고 싶어 한다.", "좋은 얼굴이군. 오늘은 죽지 않을 거 같아.", "반갑다. 함께라면 어떤 적도 두렵지 않지.", "또 네 얼굴을 보게 되는군. 마음이 놓인다.", "인연인가? 다시 만났군.", "안녕. 목숨은 붙어 있나?"};

        void Start()
        {

            savemanage.LoadFromButton();

            enemysavemanage.LoadFromButton();

            changeimage.LoadAllSprites();  

            trainimage.LoadAllSprites(); 

            buyimage.LoadAllSprites();     


            for (int i = 0; i < 9; i++)
            {
                int stresult = Random.Range(0,trainInfo.Count);
                trainResultSave[i] = trainInfo[stresult];
            }

        }

        void Update()
        {
            trainResultInfoText.text = $"{trainResultSave[fighterCount]}";
            LevelText.text = LevelTextContain;
        }

        public void selectPlayer(int playerIndex)//선수 선택
        {
            playerCheck = playerIndex;
            fighterCount = playerIndexList[playerIndex];

            fighterButtons[playerIndex].GetComponent<RectTransform>().anchoredPosition = new Vector2(-40f, 10f);
            enemyButtons[playerIndex].GetComponent<RectTransform>().anchoredPosition = new Vector2(-40f, -20f);

            fighterButtons[playerIndex].GetComponent<RectTransform>().localScale = new Vector2(3f,3f);
            enemyButtons[playerIndex].GetComponent<RectTransform>().localScale = new Vector2(-3f,3f);

            playerStatusText.GetComponentInChildren<CharacterID>().characterKey = (fighterCount + 1).ToString();
            enemyStatusText.GetComponentInChildren<CharacterID>().characterKey = (playerCheck + 1).ToString();
            
        }

        public void PlayeLevelup(int trainschoice)
        {
            if (LevelList[fighterCount] < 10)
            {   
                statechange[fighterCount].hp += 1;
                statechange[fighterCount].damage += 1;
                statechange[fighterCount].defense += 1;

                statechange[fighterCount].hp = Mathf.Round((statechange[fighterCount].hp+hpresult) * 10f) / 10f;
                statechange[fighterCount].damage = Mathf.Round((statechange[fighterCount].damage+damageresult) * 10f) / 10f;
                statechange[fighterCount].defense = Mathf.Round((statechange[fighterCount].defense+defenseresult) * 10f) / 10f;

                LevelList[fighterCount] += 1;
                LevelTextContain = "검투사 레벨업";
            }
            if (LevelList[fighterCount] == 9)
            {
                LevelTextContain = "검투사 진화";
            }
            if (LevelList[fighterCount] >= 10)
            {   
                statechange[fighterCount].hp += 5;
                statechange[fighterCount].damage += 5;
                statechange[fighterCount].defense += 5;

                statechange[fighterCount].hp = Mathf.Round((statechange[fighterCount].hp+hpresult) * 10f) / 10f;
                statechange[fighterCount].damage = Mathf.Round((statechange[fighterCount].damage+damageresult) * 10f) / 10f;
                statechange[fighterCount].defense = Mathf.Round((statechange[fighterCount].defense+defenseresult) * 10f) / 10f;

                LevelList[fighterCount] = 0;
                LevelTextContain = "검투사 레벨업";
            }
        }
        


        public void trainBack()
        {   
            for (int i=0; i < playerIndexList.Count; i++ )
            {
                fighterButtons[i].GetComponent<RectTransform>().localScale = new Vector2(1f,1f);
                enemyButtons[i].GetComponent<RectTransform>().localScale = new Vector2(1f,1f); 
            }
            
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

            for (int k = 1; k <= deleteCount; k++)
            {
                fighterButtons[playerIndexList.Count-k].gameObject.SetActive(false);
            } 
        }   

        public void sellFighter()//선수팔기
        {   
            for (int i = 0; i< 9; i++)
            {
                playerIdLIst[i] = int.Parse(playerimage[i].GetComponentInChildren<CharacterID>().characterKey);
                fighterButtons[i].GetComponentInChildren<CharacterID>().characterKey = playerIndexList[i].ToString();
            }

            sellCount += 1;
            deleteCount += 1;
            sellList.Add(fighterCount);
            
            moneymanager.AddMoney((int)(statechange[fighterCount].hp + statechange[fighterCount].damage + statechange[fighterCount].defense));

            deleteList.Add(int.Parse(playerimage[playerCheck].GetComponentInChildren<CharacterID>().characterKey));

            for (int i = playerCheck; i < playerIndexList.Count; i++)
            {
                playerIndexList[i] = playerIdLIst[i];
                playerimage[i].GetComponentInChildren<CharacterID>().characterKey = (playerIndexList[i]+1).ToString();
                //playerimage[i].GetComponentInChildren<CharacterID>().characterKey = (int.Parse(playerimage[i].GetComponentInChildren<CharacterID>().characterKey) + 1).ToString();
            }
            changeimage.LoadAllSprites();

            Debug.Log("playerIndexList: " + string.Join(", ", playerIndexList));

            for (int k = 1; k <= deleteCount; k++)
            {
                fighterButtons[playerIndexList.Count-k].gameObject.SetActive(false);
                playerimage[playerIndexList.Count-k].GetComponentInChildren<CharacterID>().characterKey = (deleteList[deleteList.Count-k]).ToString();
            }    
        }


        public void deathFighter()//선수사망
        {   
            deathCount += 1;
            deleteCount += 1;
            deathList.Add(fighterCount);

            for (int i = 0; i< 9; i++)
            {
                playerIdLIst[i] = int.Parse(playerimage[i].GetComponentInChildren<CharacterID>().characterKey);
            }

            for (int i = 0; i< 9; i++)
            {
                playerIndexList[i] = playerIdLIst[i+1];
            }

            deleteList.Add(int.Parse(playerimage[playerCheck].GetComponentInChildren<CharacterID>().characterKey));

            for (int i = playerCheck; i < playerIndexList.Count; i++)
            {
                playerimage[i].GetComponentInChildren<CharacterID>().characterKey = playerIndexList[i].ToString();
            }
            changeimage.LoadAllSprites();

            Debug.Log("playerIndexList: " + string.Join(", ", playerIndexList));

            for (int k = 1; k <= deleteCount; k++)
            {
                fighterButtons[playerIndexList.Count-k].gameObject.SetActive(false);
                playerimage[playerIndexList.Count-k].GetComponentInChildren<CharacterID>().characterKey = (deleteList[deleteList.Count-k]).ToString();
            }

            for (int j = 1; j <= deathCount; j++)
            {
                deathfighter[j-1].gameObject.SetActive(true);
                deathfighterimage[j-1].GetComponentInChildren<CharacterID>().characterKey = (deathList[j-1]+1).ToString();
            } 
            deathchangeimage.LoadAllSprites();   
        }
    }
}