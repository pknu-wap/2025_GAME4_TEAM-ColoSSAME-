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

        public int selectedIndex = -1;

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

        private Vector2[] originalPositions;
        private Vector3[] originalScales;

        void Start()
        {
            LoadUserData();
            BuildRarityPools();   // 가문 기준 전체 풀
            RemoveOwnedUnits();   // 보유 유닛 제거
        }

        void Awake()
        {
            savePath = Path.Combine(Application.persistentDataPath, "UserSave.json");
            originalPositions = new Vector2[CharacterObject.Length];
            originalScales = new Vector3[CharacterObject.Length];

            for (int i = 0; i < CharacterObject.Length; i++)
            {
                RectTransform rt = CharacterObject[i].GetComponent<RectTransform>();
                originalPositions[i] = rt.anchoredPosition;
                originalScales[i] = rt.localScale;
            }
        }

        public void LoadUserData()
        {   
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

        public void RebuildGachaPools() //선수 판매시 뽑기 재구성
        {
            BuildRarityPools();   
            RemoveOwnedUnits();   
        }


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

            for (int count = userData.myUnits.Count; count < CharacterObject.Length; count++)
            {
                CharacterObject[count].SetActive(false);
            }
        }
        public void selectPlayer(int playerIndex)//선수 선택
        {
            selectedIndex = playerIndex; 

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
            for (int i = 0; i < CharacterObject.Length; i++)
            {
                RectTransform rt = CharacterObject[i].GetComponent<RectTransform>();
                rt.anchoredPosition = originalPositions[i];
                rt.localScale = originalScales[i];
            }

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

