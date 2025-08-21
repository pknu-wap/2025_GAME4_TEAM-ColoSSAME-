using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class UnitDataManager : MonoBehaviour
{
    public static UnitDataManager Instance { get; private set; }

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
        unitDataDict = new Dictionary<string, CharacterData>();
        // Resources 폴더의 모든 JSON 파일을 불러온다.
        TextAsset[] allDataAssets = Resources.LoadAll<TextAsset>("CharacterData");

        foreach (TextAsset asset in allDataAssets)
        {
            // JSON을 FamilyData 객체로 변환 (역직렬화)한다.
            FamilyData familyData = JsonConvert.DeserializeObject<FamilyData>(asset.text);
            foreach (var character in familyData.Characters)
            {
                // 각 유닛 데이터를 딕셔너리에 추가한다. 키는 Unit_ID다.
                unitDataDict[character.Unit_ID] = character;
            }
        }
        Debug.Log($"✅ 총 {unitDataDict.Count}개의 유닛 데이터 로드 완료.");
    }

    /// <summary>
    /// 유닛 ID를 통해 상세 데이터를 가져오는 메서드.
    /// </summary>
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