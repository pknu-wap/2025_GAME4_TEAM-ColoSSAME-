using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using BattleK.Scripts.Data;
using BattleK.Scripts.Manager;
using UnityEngine.UI;
using Scripts.Team.FighterRandomBuy;
using TMPro;

namespace Scripts.Team.FighterViewer
{
    public class UnitViewer : MonoBehaviour
    {
        public UserData userData;
        public FamilyStatsCollector familystat;

        public CharacterID characterid;
        public FamilyID familyid;

        public Image[] CharacterImage;
        public GameObject[] CharacterObject;
        public GameObject[] StarCount;

        public GetPlayer getplayer;
        public List<string> fiveStarIds = new();
        public List<string> fourStarIds = new();
        public List<string> oneStarIds = new();

        public TextMeshProUGUI NameText;
        string NameTextContain;
        public TextMeshProUGUI StatText;
        string StatTextContain;

        private string savePath;

        /*void Update()
        {
            NameText.text = NameTextContain;
            StatText.text = StatTextContain;
        }*/

        void Start()
        {
            LoadUserData();
            BuildRarityPools();   // 가문 기준 전체 풀
            RemoveOwnedUnits();   // 보유 유닛 제거
        }

        public void LoadUserData()
        {
            string savePath = Path.Combine(Application.persistentDataPath, "UserSave.json");
            
            
            if (!File.Exists(savePath))
            {
                Debug.LogError("UserSave.json not found");
            }
            
            string json = File.ReadAllText(savePath);
            userData = JsonConvert.DeserializeObject<UserData>(json);
        }

        void BuildRarityPools()
        {
            fiveStarIds.Clear();
            fourStarIds.Clear();
            oneStarIds.Clear();

            foreach (var c in getplayer.familyData.Characters)
            {
                switch (c.Rarity)
                {
                    case 5: fiveStarIds.Add(c.Unit_ID); break;
                    case 4: fourStarIds.Add(c.Unit_ID); break;
                    case 1: oneStarIds.Add(c.Unit_ID); break;
                }
            }
        }
        void RemoveOwnedUnits()
        {
            HashSet<string> ownedIds = new();

            foreach (var u in userData.myUnits)
                ownedIds.Add(u.unitId);

            fiveStarIds.RemoveAll(id => ownedIds.Contains(id));
            fourStarIds.RemoveAll(id => ownedIds.Contains(id));
            oneStarIds.RemoveAll(id => ownedIds.Contains(id));
        }

        /*public void LoadFamilyData()
        {
            for (int count = 0; count < userData.myUnits.Count; count++)
            {
                if (userData.myUnits[count].rarity == 5)
                {
                    fivestarunit.Remove(0); 
                }
                if (userData.myUnits[count].rarity == 4)
                {
                    for (int index = 1; index < 5; index++)
                    {
                        if (userData.myUnits[count].unitName == getplayer.familyData.Characters[index].Unit_Name)
                        {
                            fourstarunit.Remove(index);
                            break;
                        }
                    }
                }
                if (userData.myUnits[count].rarity == 3)
                {
                    fivestarunit.Remove(0); 
                }
                if (userData.myUnits[count].rarity == 2)
                {
                    fivestarunit.Remove(0); 
                }
                if (userData.myUnits[count].rarity == 1)
                {
                    for (int index = 5; index < 10; index++)
                    {
                        if (userData.myUnits[count].unitName == getplayer.familyData.Characters[index].Unit_Name)
                        {
                            onestarunit.Remove(index);
                            break;
                        }
                    } 
                }
                
            }

        }*/

        public void UnitShow()//Load사용 안하고 쓰면 오류 
        {
            int unitCount = userData.myUnits.Count;

            for (int count = 0; count < userData.myUnits.Count; count++)
            {
                string imageName = userData.myUnits[count].unitId;
                Sprite portraitSprite = Resources.Load<Sprite>($"CharacterData/{imageName}");

                CharacterID characterid = CharacterObject[count].GetComponent<CharacterID>();
                characterid.characterKey = userData.myUnits[count].unitId;

                FamilyID familyid = CharacterObject[count].GetComponent<FamilyID>();
                familyid.FamilyKey = getplayer.familyname;


                CharacterImage[count].sprite = portraitSprite;
                CharacterImage[count].preserveAspect = true; //비율유지
                CharacterObject[count].SetActive(true);
            }

            for (int count = userData.myUnits.Count; count < 15; count++)
            {
                CharacterObject[count].SetActive(false);
            }
        }
        public void selectPlayer(int playerIndex)//선수 선택
        {
            NameTextContain = userData.myUnits[playerIndex].unitName;

            //StatTextContain = $"HP: {familystat.PlayerStats[playerIndex].HP}\nATK: {familystat.PlayerStats[playerIndex].ATK}\nDEF: {familystat.PlayerStats[playerIndex].DEF}\nAGI: {familystat.PlayerStats[playerIndex].AGI}";

            CharacterObject[playerIndex].GetComponent<RectTransform>().anchoredPosition = new Vector2(-40f, 10f);
            
            CharacterObject[playerIndex].GetComponent<RectTransform>().localScale = new Vector2(3f,3f);

            for (int i = 0; i < 5; i++)
                {
                    StarCount[i].SetActive(false);
                }
            StarCount[userData.myUnits[playerIndex].rarity - 1].SetActive(true);

            NameText.text = NameTextContain;
            //StatText.text = StatTextContain;
  
        }

        public void trainBack()
        {   
            for (int i=0; i < 15; i++ )
            {
                CharacterObject[i].GetComponent<RectTransform>().localScale = new Vector2(1f,1f);
            }
            
            CharacterObject[0].GetComponent<RectTransform>().anchoredPosition = new Vector2(-90f, -15f);//선수 위치 원래대로
            CharacterObject[1].GetComponent<RectTransform>().anchoredPosition = new Vector2(50f, -15f);
            CharacterObject[2].GetComponent<RectTransform>().anchoredPosition = new Vector2(190f, -15f);
            CharacterObject[3].GetComponent<RectTransform>().anchoredPosition = new Vector2(-90f, -135f);
            CharacterObject[4].GetComponent<RectTransform>().anchoredPosition = new Vector2(50f, -135f);
            CharacterObject[5].GetComponent<RectTransform>().anchoredPosition = new Vector2(190f, -135f);
            CharacterObject[6].GetComponent<RectTransform>().anchoredPosition = new Vector2(-90f, -255f);
            CharacterObject[7].GetComponent<RectTransform>().anchoredPosition = new Vector2(50f, -255f);
            CharacterObject[8].GetComponent<RectTransform>().anchoredPosition = new Vector2(190f, -255f);
            CharacterObject[9].GetComponent<RectTransform>().anchoredPosition = new Vector2(-90f, -375f);
            CharacterObject[10].GetComponent<RectTransform>().anchoredPosition = new Vector2(50f, -375f);
            CharacterObject[11].GetComponent<RectTransform>().anchoredPosition = new Vector2(190f, -375f);
            CharacterObject[12].GetComponent<RectTransform>().anchoredPosition = new Vector2(-90f, -495f);
            CharacterObject[13].GetComponent<RectTransform>().anchoredPosition = new Vector2(50f, -495f);
            CharacterObject[14].GetComponent<RectTransform>().anchoredPosition = new Vector2(190f, -495f);

        }   
    }

    public class UserData
    {
        public string userName;
        public int level;
        public float exp;
        public int money;
        public Dictionary<string, int> inventory;
        public List<UnitData> myUnits;
    }

    public class UnitData
    {
        public string unitId; 
        public string unitName;
        public int rarity;
        public int level;
        public float exp;
    }
     
}

