using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using UnityEngine.UI;

public class GetPlayer : MonoBehaviour
{
    private FamilyData familyData;
    public Image[] CharacterImage;
    public List<int> CharacterGetCheck = new List<int> {0,0,0,0,0,0,0,0,0,0};
    public List<string> CharaterIDList = new List<string> {"0", "0", "0", "0","0", "0", "0", "0" ,"0", "0" };

    /*private Dictionary<int, float> rarityWeights = new Dictionary<int, float>()
    {
        { 5, 0.05f }, // 5성 
        { 4, 0.25f }, // 4성 
    };*/

    void Start()
    {
        Debug.Log("[GetPlayer] Start 호출됨. JSON 로드 시작");
        TextAsset Characterjson = Resources.Load<TextAsset>("CharacterData/Caelus");
      
        familyData = JsonConvert.DeserializeObject<FamilyData>(Characterjson.text);

    }

    public void RandomSelect(int count)
    {
        if (CharacterGetCheck[count] == 0)
        {
            int index = UnityEngine.Random.Range(0,familyData.Characters.Count); 
            CharacterData randomCharacter = familyData.Characters[index];
            Debug.Log($"[GetPlayer] 선택 인덱스: {index} / 총 {familyData.Characters.Count}개 | Unit_ID: {randomCharacter.Unit_ID}, Unit_Name: {randomCharacter.Unit_Name}, Rarity: {randomCharacter.Rarity}");

            string ImageName = Path.GetFileNameWithoutExtension(randomCharacter.Visuals.Portrait);
            Sprite portraitSprite = Resources.Load<Sprite>($"CharacterData/{ImageName}");
            Debug.Log($"[GetPlayer] 스프라이트 적용 완료: {ImageName}");

            CharaterIDList[count] = randomCharacter.Unit_ID;
            CharacterGetCheck[count] = 1;

            CharacterImage[count].sprite = portraitSprite;
            CharacterImage[count].preserveAspect = true;//비율 유지
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
