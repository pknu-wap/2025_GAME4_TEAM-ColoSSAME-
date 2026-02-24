using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.AddressableAssets;               
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.TextCore.Text;
using TextAsset = UnityEngine.TextAsset;


public class UnitDataManager : MonoBehaviour
{
    public static UnitDataManager Instance { get; private set; }

    // 모든 유닛 데이터를 Family_ID을 키로 하는 딕셔너리에 저장
    public Dictionary<string, List<CharacterData>> familyUnitDataDict;
    // 유닛 ID를 키로, 유닛 데이터를 값으로 하는 딕셔너리 (추가)
    public Dictionary<string, CharacterData> unitDataDict;
    
    private AsyncOperationHandle<IList<TextAsset>> loadHandle; 
    public bool IsLoaded { get; private set; }   
    
    //public System.Action OnDataLoaded; 

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
        this.familyUnitDataDict = new Dictionary<string, List<CharacterData>>();
        this.unitDataDict = new Dictionary<string, CharacterData>(); 
        
        // Label 기반 전체 JSON 로드
        this.loadHandle = Addressables.LoadAssetsAsync<TextAsset>(
            "FamilyData",
            null
        );
        this.loadHandle.Completed += this.OnAllDataLoaded;
        
        Debug.Log($"✅ 총 {familyUnitDataDict.Count}개의 유닛 데이터 로드 완료.");
    }

    private void OnAllDataLoaded(
        AsyncOperationHandle<IList<TextAsset>> handle)
    {
        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError("유닛 데이터 로드 실패");
            return;
        }

        foreach (TextAsset asset in handle.Result)
        {
            FamilyData familyData = JsonConvert.DeserializeObject<FamilyData>(asset.text);

            string familyId = familyData.Family_ID;
            string familyName = familyData.Family_Name;


            if (!this.familyUnitDataDict.ContainsKey(familyId))
            {
                this.familyUnitDataDict[familyId] = new List<CharacterData>();
            }

            foreach (var character in familyData.Characters)
            {
                character.Family_Name = familyName;
                character.Family_ID = familyId;
                
                this.familyUnitDataDict[familyId].Add(character);
                this.unitDataDict[character.Unit_ID] = character;
            }
            
        }
        
        this.IsLoaded = true;
            
        Debug.Log($" 총 {this.familyUnitDataDict.Count}개의 가문 데이터 로드 완료.");
        
        foreach (var key in this.familyUnitDataDict.Keys)
        {
            Debug.Log($"[Loaded Family Key] = '{key}'");
        }

    }
    
    private void OnDestroy()
    {
        if (this.loadHandle.IsValid())
        {
            Addressables.Release(this.loadHandle);
        }
    }
    
    /// 패밀리 ID를 통해 상세 데이터를 가져오는 메서드.
    public List<CharacterData> GetFamilyUnits(string familyId)
    {
        if (!this.IsLoaded)
        {
            Debug.LogWarning("아직 데이터 로딩이 완료되지 않았습니다.");
            return null;
        }

        if (familyUnitDataDict.ContainsKey(familyId))
        {
            return familyUnitDataDict[familyId];
        }
        
        Debug.Log($"[Requested Family Key] = '{familyId}'");
        Debug.LogWarning($"가문 데이터 '{familyId}'를 찾을 수 없습니다.");
        return null;
    }
    
    /// 유닛 ID를 통해 상세 데이터를 가져오는 메서드.
    public CharacterData GetCharacterData(string unitId)
    {
        if (!this.IsLoaded)
        {
            Debug.LogWarning("❗ 아직 데이터 로딩이 완료되지 않았습니다.");
            return null;
        }

        if (this.unitDataDict.ContainsKey(unitId))
        {
            return this.unitDataDict[unitId];
        }

        Debug.LogWarning($"❌ 유닛 데이터 ID를 찾을 수 없습니다: {unitId}");
        return null;
    }
}