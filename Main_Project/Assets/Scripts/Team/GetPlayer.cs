using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using Scripts.Team.IsAnimStopClick;
using Scripts.Team.CardAnimcontrol;
using Scripts.Team.FighterViewer;

//나중에 가지고 있는 유닛 수가 최대일때 못뽑게하기 

namespace Scripts.Team.FighterRandomBuy
{
    public class GetPlayer : MonoBehaviour
    {
        public UserManager userManager;
        private Unit unit;

        public FamilyData familyData;

        public string familyname;

        public GameObject[] CharacterGather;
        public Image[] CharacterImage;
        public Image SelectCharaterImage;
        public SpriteRenderer[] FlipedCard;
        
        public List<int> CharacterGetCheck = new List<int> {0,0,0,0,0,0,0,0,0,0};
        public List<string> CharacterIDList;
        public int count;
        public GameObject[] StarCount;

        public UnitViewer unitviewer;
        
        public GameObject cards;
        public GameObject cardsstate;

        public TextMeshProUGUI StateText;
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI ClassText;
        public TextMeshProUGUI StoryText;

        public Animator[] anim;
        public Sprite cardback; 

        public CardClickStop blockclick;

        private League league; //가문

        private Dictionary<string, CharacterData> characterDict;//캐릭터 dict

        private static readonly Dictionary<int, string> FamilyMap = new()   //가문
        {
            {1, "Caelus"},
            {2, "Flora"},
            {3, "Ignis"},
            {4, "Lumen"},
            {5, "Nox"},
            {6, "Mors"},
            {7, "Fulger"},
            {8, "Mare"},
            {9, "Terra"},
            {10, "Astra"},
        };

        private Dictionary<string, Sprite> spriteCache = new(); //sprite저장

        ///뽑기용
        private List<string> gachaFiveStarIds = new();
        private List<string> gachaFourStarIds = new();
        private List<string> gachaOneStarIds = new();
        
        private bool isAnyCardAnimating = false; //카드 뒤집기 애니메이션 구분(겹치지않게)

        void Start()
        {
            league = LoadLeague();
            SelectFamily();
            BuildCharacterDict();  
        }

        void Awake()
        {
            CharacterIDList = new List<string>(new string[10]);
        }

        void BuildCharacterDict()
        {
            characterDict = new Dictionary<string, CharacterData>();

            foreach (var c in familyData.Characters)
            {
                characterDict[c.Unit_ID] = c;
            }
        }

        

        private void SelectFamily()
        {
            int teamId = league.settings.playerTeamId;

            if (!FamilyMap.TryGetValue(teamId, out string family))
            {
                Debug.LogError($"잘못된 가문 id: {teamId}");
                return;
            }

            TextAsset json = Resources.Load<TextAsset>($"CharacterData/{family}");
            familyData = JsonConvert.DeserializeObject<FamilyData>(json.text);
            familyname = family;
        }
        private League LoadLeague()
        {
            string path = Path.Combine(Application.persistentDataPath, "LeagueSave.json");

            if (!File.Exists(path))
            {
                Debug.LogError("LeagueSave.json 없음");
                return null;
            }

            string json = File.ReadAllText(path);
            League league = JsonConvert.DeserializeObject<League>(json);

            return league;
        }

        private string GetRandomCharacterId()   //뽑기 확률
        {
            int rand = Random.Range(0, 100);

            if (rand < 1 && unitviewer.fiveStarIds.Count > 0)
                return unitviewer.fiveStarIds[Random.Range(0, unitviewer.fiveStarIds.Count)];

            if (rand < 21 && unitviewer.fourStarIds.Count > 0)
                return unitviewer.fourStarIds[Random.Range(0, unitviewer.fourStarIds.Count)];

            if (unitviewer.oneStarIds.Count > 0)
                return unitviewer.oneStarIds[Random.Range(0, unitviewer.oneStarIds.Count)];

            /// 1성 없으면 낮은 순 차례로
            if (unitviewer.fourStarIds.Count > 0)
                return unitviewer.fourStarIds[Random.Range(0, unitviewer.fourStarIds.Count)];
            
            if (unitviewer.fiveStarIds.Count > 0)
                return unitviewer.fiveStarIds[Random.Range(0, unitviewer.fiveStarIds.Count)];

            return null;
        }

        public void RandomSetting()
        {
            unitviewer.RebuildGachaPools();
            
            for (int i = 0; i < 10; i++)
            {
                string id = GetRandomCharacterId();

                if (id == null)
                {
                    Debug.LogError("뽑을 수 있는 캐릭터가 없습니다.");
                    return;
                }

                CharacterIDList[i] = id;
            }
        }

        private Sprite GetPortrait(string imageName)    //sprite 로드 저장
        {
            if (!spriteCache.TryGetValue(imageName, out Sprite sprite))
            {
                sprite = Resources.Load<Sprite>($"CharacterData/{imageName}");

                if (sprite == null)
                {
                    Debug.LogError($"[GetPlayer] Portrait 없음: {imageName}");
                    return null;
                }

                spriteCache.Add(imageName, sprite);
            }

            return sprite;
        }

        public void OnCardClick(int index)
        {
            if (CharacterGetCheck[index] == 0)
            {
                RandomSelect(index);
            }

            if (isAnyCardAnimating)
                return; 
                

            if (CharacterGetCheck[index] == 2)
            {
                ShowExplain(index);
            }

        }

        private void RandomSelect(int index)
        {
            isAnyCardAnimating = true;

            CharacterGetCheck[index] = 1;

            CardAnim cardAnim = anim[index].GetComponent<CardAnim>();
            cardAnim.SetIndex(index);
            anim[index].SetTrigger("Iscardclick");

            CharacterData randomCharacter = characterDict[CharacterIDList[index]];
            string imageName = Path.GetFileNameWithoutExtension(randomCharacter.Visuals.Portrait);
            CharacterImage[index].sprite = GetPortrait(imageName);
            CharacterImage[index].preserveAspect = true;
        }

        private void ShowExplain(int index)
        {
            
            CharacterData characterdata = characterDict[CharacterIDList[index]];

            string imageName = Path.GetFileNameWithoutExtension(characterdata.Visuals.Portrait);
            SelectCharaterImage.sprite = GetPortrait(imageName);
            SelectCharaterImage.preserveAspect = true;

            unit = new Unit(characterdata.Unit_ID, characterdata.Rarity, characterdata.Unit_Name);

            NameText.text = $"이름:{characterdata.Unit_Name}";
            ClassText.text = $"직업:{characterdata.Class}";
            StoryText.text = characterdata.Story;
            StateText.text = $"ATK: {characterdata.Stat_Distribution.ATK}\nDEF: {characterdata.Stat_Distribution.DEF}\nHP: {characterdata.Stat_Distribution.HP}\nAGI: {characterdata.Stat_Distribution.AGI}";

            for (int i = 0; i < 5; i++)
            {
                StarCount[i].SetActive(false);
            }
            StarCount[characterdata.Rarity - 1].SetActive(true);

            cardsstate.SetActive(true);
            cards.SetActive(false);

        }

        public void OnAnyCardAnimEnd()
        {
            isAnyCardAnimating = false;
        }

        public void BuyRandomUnit()
        {
            for (int i = 0; i < 10; i++)
            {
                anim[i].Rebind();           //애니메이션 처음으로
                anim[i].Update(0f);         //늦어질 가능성 줄이기
                FlipedCard[i].sprite = cardback;
                CharacterGather[i].SetActive(false);
            }
            CharacterGetCheck = CharacterGetCheck.ConvertAll(x => 0);
            UserManager.Instance.AddUnit(unit);
        }

        public void BackExplain()
        {
            cardsstate.SetActive(false);
            cards.SetActive(true);
        }

    }
    public class FamilyData
    {
        public string Family_Name;
        public string Family_Description;
        public int id;
        public List<CharacterData> Characters;
    }

    public class CharacterData
    {
        public string Unit_ID;
        public string Unit_Name;
        public int Rarity;
        public int Level;
        public string Class;
        public string Description;
        public string Story;
        public StatDistribution Stat_Distribution;
        public Visuals Visuals;
    }

    public class StatDistribution
    {
        public int ATK;
        public int DEF;
        public int HP;
        public int AGI;
    }

    public class Visuals
    {
        public string Portrait;
        public string Prefab;
    }

    ///리그
    public class League
    {
        public LeagueSettings settings;
    }

    public class LeagueSettings
    {
        public int playerTeamId;
    }
}