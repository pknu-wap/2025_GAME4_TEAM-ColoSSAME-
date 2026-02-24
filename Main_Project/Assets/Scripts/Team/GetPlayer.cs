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
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

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
            {1, "카이루스"},
            {2, "플로라"},
            {3, "이그니스"},
            {4, "루멘"},
            {5, "녹스"},
            {6, "모르스"},
            {7, "폴그르"},
            {8, "마레"},
            {9, "테라"},
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
            //BuildCharacterDict();  
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
            
            familyname = family;
            TextAsset json = Resources.Load<TextAsset>($"CharacterData/{family}");

            List<CharacterData> units = UnitDataManager.Instance.GetFamilyUnits(familyname);

            // FamilyData 객체로 변환
            familyData = new FamilyData
            {
                Family_Name = familyname,
                Characters = units
            };
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

        /*private Sprite GetPortrait(string imageName)    //sprite 로드 저장
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
        }*/

        public void OnCardClick(int index)  //카드 클릭
        {
            if (CharacterGetCheck[index] == 0)
            {
                StartCoroutine(LoadSprite(CharacterImage[index], CharacterIDList[index]));
            }

            if (isAnyCardAnimating)
                return; 
                

            if (CharacterGetCheck[index] == 2)
            {
                ShowExplain(index);
            }

        }

        public void AllCardOpen(int index)
        {
            for(int i = 0; i < 10; i++)
            {
                if (CharacterGetCheck[i] == 0)
                {
                    RandomSelect(i);
                }
                
            }
        }

        private void RandomSelect(int index)    //카드 열기
        {
            isAnyCardAnimating = true;

            CharacterGetCheck[index] = 1;

            CardAnim cardAnim = anim[index].GetComponent<CardAnim>();
            cardAnim.SetIndex(index);
            anim[index].SetTrigger("Iscardclick");

            CharacterData randomCharacter = UnitDataManager.Instance.GetCharacterData(CharacterIDList[index]);

             StartCoroutine(LoadSprite(CharacterImage[index], randomCharacter.Unit_ID));
        }

        private void ShowExplain(int index)
        {
            
            CharacterData characterdata = UnitDataManager.Instance.GetCharacterData(CharacterIDList[index]);

            StartCoroutine(LoadSprite(SelectCharaterImage, characterdata.Unit_ID));

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

        public void BuyRandomUnit(int count)
        {
            for (int i = 0; i < 10; i++)
            {
                anim[i].Rebind();           //애니메이션 처음으로
                anim[i].Update(0f);         //늦어질 가능성 줄이기
                FlipedCard[i].sprite = cardback;
                CharacterGather[i].SetActive(false);
            }
            CharacterGetCheck = CharacterGetCheck.ConvertAll(x => 0);
            if (count == 0)
            {
                UserManager.Instance.AddUnit(unit);
            }
        }

        public void BackExplain()
        {
            cardsstate.SetActive(false);
            cards.SetActive(true);
        }

        private IEnumerator LoadSprite(Image img, string unitId)
        {
            var handle = Addressables.LoadAssetAsync<Sprite>(unitId);
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                img.sprite = handle.Result;
                img.preserveAspect = true;
            }
            else
            {
                Debug.LogError($"로드 실패: {unitId}");
            }
        }

    }
    public class FamilyData
    {
        public string Family_Name;
        public string Family_Description;
        public int id;
        public List<CharacterData> Characters;
    }

    /*public class CharacterData
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
    }*/

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