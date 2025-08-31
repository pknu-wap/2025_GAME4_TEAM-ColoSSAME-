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

namespace Scripts.Team.FighterRandomBuy
{
    public class GetPlayer : MonoBehaviour
    {
        public UserManager userManager;
        private Unit unit;

        private FamilyData familyData;

        public string familyname;

        public GameObject[] CharacterGather;
        public Image[] CharacterImage;
        public Image SelectCharaterImage;
        public SpriteRenderer[] FlipedCard;
        
        public List<int> CharacterGetCheck = new List<int> {0,0,0,0,0,0,0,0,0,0};
        public List<int> CharacterIDList = new List<int> {0,0,0,0,0,0,0,0,0,0};
        public int count;
        public GameObject[] StarCount;
        
        public GameObject cards;
        public GameObject cardsstate;

        public TextMeshProUGUI StateText;
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI ClassText;
        public TextMeshProUGUI StoryText;

        public Animator[] anim;
        public Sprite cardback; 

        public CardClickStop blockclick;

        /*private Dictionary<int, float> rarityWeights = new Dictionary<int, float>()
        {
            { 5, 0.05f }, // 5성 
            { 4, 0.25f }, // 4성 
        };*/

        public void SelectFamily(int index)
        {
            if (index == 1)
            {
                TextAsset Characterjson = Resources.Load<TextAsset>("CharacterData/Caelus");
                familyData = JsonConvert.DeserializeObject<FamilyData>(Characterjson.text);
                familyname = "Caelus";
            }
            if (index == 2)
            {
                TextAsset Characterjson = Resources.Load<TextAsset>("CharacterData/Flora");
                familyData = JsonConvert.DeserializeObject<FamilyData>(Characterjson.text);
                familyname = "Flora";
            }
            if (index == 3)
            {
                TextAsset Characterjson = Resources.Load<TextAsset>("CharacterData/Ignis");
                familyData = JsonConvert.DeserializeObject<FamilyData>(Characterjson.text);
                familyname = "Ignis";
            }
            if (index == 4)
            {
                TextAsset Characterjson = Resources.Load<TextAsset>("CharacterData/Lumen");
                familyData = JsonConvert.DeserializeObject<FamilyData>(Characterjson.text);
                familyname = "Lumen";
            }
            if (index == 5)
            {
                TextAsset Characterjson = Resources.Load<TextAsset>("CharacterData/Nox");
                familyData = JsonConvert.DeserializeObject<FamilyData>(Characterjson.text);
                familyname = "Nox";
            }
            if (index == 6)
            {
                TextAsset Characterjson = Resources.Load<TextAsset>("CharacterData/Mors");
                familyData = JsonConvert.DeserializeObject<FamilyData>(Characterjson.text);
                familyname = "Mors";
            }
            if (index == 7)
            {
                TextAsset Characterjson = Resources.Load<TextAsset>("CharacterData/Fulger");
                familyData = JsonConvert.DeserializeObject<FamilyData>(Characterjson.text);
                familyname = "Fulger";
            }
            if (index == 8)
            {
                TextAsset Characterjson = Resources.Load<TextAsset>("CharacterData/Mare");
                familyData = JsonConvert.DeserializeObject<FamilyData>(Characterjson.text);
                familyname = "Mare";
            }
            if (index == 9)
            {
                TextAsset Characterjson = Resources.Load<TextAsset>("CharacterData/Terra");
                familyData = JsonConvert.DeserializeObject<FamilyData>(Characterjson.text);
                familyname = "Terra";
            }
            if (index == 10)
            {
                TextAsset Characterjson = Resources.Load<TextAsset>("CharacterData/Astra");
                familyData = JsonConvert.DeserializeObject<FamilyData>(Characterjson.text);
                familyname = "Astra";
            }
        }

        public void RandomSetting()
        {
            List<string> SwordClass = new List<string> {"검투사","군단병"};
            List<string> WizardClass = new List<string> {"주술사","사제"};
            List<string> ArcherClass = new List<string> {"척후병"};
            List<string> AssassinClass = new List<string> {"암살자"};

            for (int i = 0; i < 10; i++)
            {
                int rand = UnityEngine.Random.Range(0, 100); // 0~99

                if (rand < 1) // 1%
                {
                    CharacterIDList[i] = 0;
                }
                else if (rand < 21) // 20% (1~20)
                {
                    CharacterIDList[i] = UnityEngine.Random.Range(1, 6); // 1~5
                }
                else // 나머지 79%
                {
                    CharacterIDList[i] = UnityEngine.Random.Range(5, familyData.Characters.Count); // 5~끝
                }

                CharacterData characterdata = familyData.Characters[CharacterIDList[i]];
            }
        }

        public void RandomSelect(int index)
        {
            count = index;
            if (CharacterGetCheck[count] == 0)
            {   
                blockclick.IsAnimStopClick();
                anim[count].SetTrigger("Iscardclick");

                CharacterData randomCharacter = familyData.Characters[CharacterIDList[count]];
                Debug.Log($"총 {familyData.Characters.Count}개 | Unit_ID: {randomCharacter.Unit_ID}, Unit_Name: {randomCharacter.Unit_Name}, Rarity: {randomCharacter.Rarity}");

                string ImageName = Path.GetFileNameWithoutExtension(randomCharacter.Visuals.Portrait);
                Sprite portraitSprite = Resources.Load<Sprite>($"CharacterData/{ImageName}");
                Debug.Log($"[GetPlayer] 스프라이트 적용 완료: {ImageName}");

                CharacterGetCheck[count] = 1;

                CharacterImage[count].sprite = portraitSprite;
                CharacterImage[count].preserveAspect = true;//비율 유지
            }

        }

        public void ShowExplain(int index)
        {
            if (CharacterGetCheck[index] == 1)
            {
                CharacterData characterdata = familyData.Characters[CharacterIDList[index]];

                string ImageName = Path.GetFileNameWithoutExtension(characterdata.Visuals.Portrait);
                Sprite portraitSprite = Resources.Load<Sprite>($"CharacterData/{ImageName}");
                SelectCharaterImage.sprite = portraitSprite;
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
}