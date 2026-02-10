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
        //private UserData userData;
        public UserManager userManager;
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

        //private string savePath;

        private Vector2[] originalPositions;
        private Vector3[] originalScales;

        void Start()
        {
            //LoadUserData();
            //BuildRarityPools();   // 가문 기준 전체 풀
            //RemoveOwnedUnits();   // 보유 유닛 제거
        }

        void Awake()
        {
            userManager = UserManager.Instance;

            originalPositions = new Vector2[CharacterObject.Length];
            originalScales = new Vector3[CharacterObject.Length];

            for (int i = 0; i < CharacterObject.Length; i++)
            {
                RectTransform rt = CharacterObject[i].GetComponent<RectTransform>();
                originalPositions[i] = rt.anchoredPosition;
                originalScales[i] = rt.localScale;
            }
        }

        /*public void LoadUserData()
        {   
            if (!File.Exists(savePath))
            {
                Debug.LogError("UserSave.json not found");
            }
            
            string json = File.ReadAllText(savePath);
            userData = JsonConvert.DeserializeObject<UserData>(json);
        }*/

        void BuildRarityPools()
        {
            fiveStarIds.Clear();
            fourStarIds.Clear();
            oneStarIds.Clear();

            var units = UnitDataManager.Instance.GetFamilyUnits(getplayer.familyname);
            Debug.Log("Units count: " + (units?.Count ?? 0));

            foreach (var c in units)
            {
                switch (c.Rarity)
                {
                    case 5: fiveStarIds.Add(c.Unit_ID); break;
                    case 4: fourStarIds.Add(c.Unit_ID); break;
                    case 1: oneStarIds.Add(c.Unit_ID); break;
                }
            }

            Debug.Log($"5성: {fiveStarIds.Count}, 4성: {fourStarIds.Count}, 1성: {oneStarIds.Count}");
        }

        void RemoveOwnedUnits()
        {
            HashSet<string> ownedIds = new();

            foreach (var u in userManager.user.myUnits)
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
            var myUnits = userManager.user.myUnits;

            for (int i = 0; i < myUnits.Count; i++)
            {
                Unit unit = myUnits[i];

                var unitSO = UnitDataManager.Instance.GetCharacterData(unit.unitId);;

                //CharacterImage[i].preserveAspect = true;

                CharacterID characterid = CharacterObject[i].GetComponent<CharacterID>();
                characterid.characterKey = unit.unitId;

                FamilyID familyid = CharacterObject[i].GetComponent<FamilyID>();
                familyid.FamilyKey = getplayer.familyname;

                //CharacterImage[i].sprite = portraitSprite;
                CharacterImage[i].sprite =  Resources.Load<Sprite>($"CharacterData/{unitSO.Unit_ID}");
                CharacterImage[i].preserveAspect = true;

                CharacterObject[i].SetActive(true);
            }

            for (int i = myUnits.Count; i < CharacterObject.Length; i++)
                CharacterObject[i].SetActive(false);

        }

        public void selectPlayer(int playerIndex)//선수 선택
        {
            selectedIndex = playerIndex;

            Unit unit = userManager.user.myUnits[playerIndex];

            var unitData = UnitDataManager.Instance.GetCharacterData(unit.unitId);
            if (unitData == null) return;

            NameText.text = unitData.Unit_Name; // 또는 JSON에 있는 이름 필드

            RectTransform rt = CharacterObject[playerIndex].GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(-40f, 10f);
            rt.localScale = Vector3.one * 3f;

            for (int i = 0; i < StarCount.Length; i++)
                StarCount[i].SetActive(false);

            StarCount[unit.rarity - 1].SetActive(true);

            userManager.SetSelectedUnit(unit.unitId);
  
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

    /*public class UserData
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
    }*/
     
}

