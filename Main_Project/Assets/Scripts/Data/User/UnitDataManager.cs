using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class UnitDataManager : MonoBehaviour
{
    public static UnitDataManager Instance { get; private set; }

    // 모든 유닛 데이터를 Family_Name을 키로 하는 딕셔너리에 저장
    public Dictionary<string, List<CharacterData>> familyUnitDataDict;
    // 유닛 ID를 키로, 유닛 데이터를 값으로 하는 딕셔너리 (추가)
    public Dictionary<string, CharacterData> unitDataDict;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllUnitData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadAllUnitData()
    {
        familyUnitDataDict = new Dictionary<string, List<CharacterData>>();
        // Resources 폴더의 모든 JSON 파일을 불러온다.
        TextAsset[] allDataAssets = Resources.LoadAll<TextAsset>("CharacterData");

        foreach (TextAsset asset in allDataAssets)
        {
            // JSON을 FamilyData 객체로 변환 (역직렬화)한다.
            FamilyData familyData = JsonConvert.DeserializeObject<FamilyData>(asset.text);
            string familyName = familyData.Family_Name;
            
            // Family_Name에 해당하는 리스트가 없으면 새로 생성
            if (!familyUnitDataDict.ContainsKey(familyName))
            {
                familyUnitDataDict[familyName] = new List<CharacterData>();
            }

            // 각 캐릭터 데이터를 리스트에 추가 (이때 순서가 유지된다)
            foreach (var character in familyData.Characters)
            {
                character.Family_Name = familyName;
                familyUnitDataDict[familyName].Add(character);
                unitDataDict[character.Unit_ID] = character; // Unit_ID로 개별 유닛 저장
            }
        }
        Debug.Log($"✅ 총 {familyUnitDataDict.Count}개의 유닛 데이터 로드 완료.");
    }
    
    /// 패밀리 ID를 통해 상세 데이터를 가져오는 메서드.
    public List<CharacterData> GetFamilyUnits(string familyName)
    {
        if (familyUnitDataDict.ContainsKey(familyName))
        {
            return familyUnitDataDict[familyName];
        }
        Debug.LogWarning($"❌ 가문 데이터 '{familyName}'를 찾을 수 없습니다.");
        return null;
    }
    
    /// 유닛 ID를 통해 상세 데이터를 가져오는 메서드.
    public CharacterData GetCharacterData(string unitId)
    {
        if (unitDataDict.ContainsKey(unitId))
        {
            return unitDataDict[unitId];
        }
        Debug.LogWarning($"❌ 유닛 데이터 ID를 찾을 수 없습니다: {unitId}");
        return null;
    }
}