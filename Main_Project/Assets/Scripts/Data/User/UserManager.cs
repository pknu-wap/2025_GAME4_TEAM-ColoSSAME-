using UnityEngine;
using System.IO;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class UserManager : MonoBehaviour
{
    public static UserManager Instance { get; private set; }

    public User user;

    private string savePath;

    [Header("Achievement")]
    [SerializeField] private AchievementManager achievementManager;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            savePath = Path.Combine(Application.persistentDataPath, "UserSave.json");
            LoadUser();

            // ✅ 씬 로드될 때마다 도감 매니저 자동 초기화
            SceneManager.sceneLoaded += OnSceneLoaded;

            // ✅ 시작 씬에도 도감이 있을 수 있으니 한 번 시도
            InitAchievementInScene();

            Debug.Log("✅ UserManager 초기화 완료");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitAchievementInScene();
    }

    private void InitAchievementInScene()
    {
        AchievementManager am = FindFirstObjectByType<AchievementManager>();
        if (am == null)
        {
            Debug.Log("ℹ️ 현재 씬에 AchievementManager가 없음 (도감 UI 없는 씬일 수 있음)");
            return;
        }

        if (user == null)
        {
            Debug.LogWarning("⚠️ user가 null이라 AchievementManager.Init을 할 수 없음");
            return;
        }

        am.Init(user);
        Debug.Log("✅ 씬의 AchievementManager.Init(user) 완료");
    }
    // 현재 UI에서 선택된 유닛 ID (fighter 클릭 시 저장)
    public string selectedUnitId { get; private set; }

// 선택된 유닛을 저장하는 함수
    public void SetSelectedUnit(string unitId)
    {
        selectedUnitId = unitId;
        Debug.Log($"🟩 선택 유닛 저장됨: {selectedUnitId}");
    }
    /// <summary>
    /// unitId로 유저의 myUnits에서 유닛을 찾아 반환
    /// </summary>
    public Unit GetMyUnitById(string unitId)
    {
        if (user == null || user.myUnits == null) return null;
        if (string.IsNullOrEmpty(unitId)) return null;

        for (int i = 0; i < user.myUnits.Count; i++)
        {
            Unit u = user.myUnits[i];
            if (u != null && u.unitId == unitId)
                return u;
        }
        return null;
    }

    /// <summary>
    /// 선택(또는 지정) 유닛의 경험치 추가 + 레벨업 처리 + 저장
    /// </summary>
    public bool AddUnitExp(string unitId, float amount)
    {
        Unit unit = GetMyUnitById(unitId);
        if (unit == null)
        {
            Debug.LogWarning($"⚠️ AddUnitExp 실패: 유닛을 찾지 못함 (unitId={unitId})");
            return false;
        }

        unit.exp += amount;
        Debug.Log($"✨ 유닛 경험치 추가: {unit.unitName} +{amount}, 총합: {unit.exp}");
        
        while (unit.exp >= 100)
        {
            unit.exp -= 100;
            unit.level++;
            Debug.Log($"🎉 유닛 레벨업! {unit.unitName} -> Lv {unit.level}");
        }

        SaveUser();
        return true;
    }


    /// <summary>
    /// 유저 데이터 로드
    /// </summary>
    private void LoadUser()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            user = JsonConvert.DeserializeObject<User>(json);
            
            user.EnsureDictionaries();
            
            Debug.Log("✅ 유저 데이터 로드 완료");
        }
        else
        {
            Debug.Log("🆕 저장된 유저 데이터 없음, 새로 생성 필요");
            user = null;
        }
    }

    /// <summary>
    /// 유저 데이터 저장
    /// </summary>
    public void SaveUser()
    {
        string json = JsonConvert.SerializeObject(user, Formatting.Indented);
        File.WriteAllText(savePath, json);
        Debug.Log("💾 유저 데이터 저장 완료");
    }

    /// <summary>
    /// 새로운 유저 생성 (버튼 등에서 호출)
    /// </summary>
    public void NewUser(string userName)
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("🗑️ 기존 유저 데이터 삭제 완료");
        }

        user = new User
        {
            userName = userName,
            level = 1,
            exp = 0f,
            inventory = new Dictionary<string, int>(),
            myUnits = new List<Unit>()
        };
        user.EnsureDictionaries(); // 도감 딕셔너리 포함 null 방지

        SaveUser();
        Debug.Log($"✅ 새로운 유저 생성: {userName}");
        // 새 유저 기준으로 도감 매니저 재초기화
        if (achievementManager != null)
        {
            achievementManager.Init(user);
        }

    }

    /// <summary>
    /// 경험치 추가 + 레벨업 확인
    /// </summary>
    public void AddExp(float amount)
    {
        user.exp += amount;
        Debug.Log($"✨ 경험치 추가: {amount}, 총합: {user.exp}");

        float required = RequiredExpForLevel(user.level);
        while (user.exp >= required)
        {
            user.exp -= required;
            user.level++;
            Debug.Log($"🎉 레벨업! 현재 레벨: {user.level}");
            required = RequiredExpForLevel(user.level);
        }

        SaveUser();
    }

    private float RequiredExpForLevel(int level)
    {
        return 100 + (level - 1) * 50;  // 레벨마다 증가
    }

    /// <summary>
    /// 아이템 추가
    /// </summary>
    public void AddItem(string itemName, int count = 1)
    {
        if (!user.inventory.ContainsKey(itemName))
            user.inventory[itemName] = 0;

        user.inventory[itemName] += count;
        Debug.Log($"🎒 아이템 추가: {itemName} x{count}");
        SaveUser();
    }

    /// <summary>
    /// 아이템 제거
    /// </summary>
    public void RemoveItem(string itemName, int count = 1)
    {
        if (user.inventory.ContainsKey(itemName))
        {
            user.inventory[itemName] -= count;
            if (user.inventory[itemName] <= 0)
                user.inventory.Remove(itemName);

            Debug.Log($"🗑️ 아이템 제거: {itemName} x{count}");
            SaveUser();
        }
        else
        {
            Debug.LogWarning($"❌ 제거할 아이템이 없습니다: {itemName}");
        }
    }
    /*public void GetRandomUnit()
    {
        if (UnitDataManager.Instance == null)
        {
            Debug.LogError("❌ DataManager가 초기화되지 않았습니다.");
            return;
        }

        // DataManager에서 로드된 모든 유닛 ID 목록을 가져온다.
        List<string> allUnitIds = new List<string>(UnitDataManager.Instance.unitDataDict.Keys);
        if (allUnitIds.Count == 0)
        {
            Debug.LogWarning("❌ 로드된 유닛 데이터가 없습니다.");
            return;
        }

        // 목록에서 무작위로 유닛 ID를 하나 선택한다.
        string randomUnitId = allUnitIds[Random.Range(0, allUnitIds.Count)];
        
        // 선택된 ID로 새로운 Unit 인스턴스를 생성한다.
        Unit newUnit = new Unit(randomUnitId);

        // 생성된 유닛을 유저의 myUnits 리스트에 추가한다.
        AddUnit(newUnit);
        
        // 획득한 유닛의 이름을 DataManager를 통해 가져와 로그를 출력한다.
        CharacterData acquiredData = UnitDataManager.Instance.GetCharacterData(randomUnitId);
        Debug.Log($"🎉 새로운 유닛 획득! 이름: {acquiredData.Unit_Name}, 희귀도: {acquiredData.Rarity}");
    }*/
    
    public void AddUnit(Unit newUnit)
    {
        user.myUnits.Add(newUnit);
        Debug.Log($"🗡️ 새로운 유닛 영입: {newUnit.unitId}");
        SaveUser();
    }
    public void AddInitialUnitsByFamily(string familyName)
    {
        if (UnitDataManager.Instance == null)
        {
            Debug.LogError("❌ DataManager가 초기화되지 않았습니다.");
            return;
        }

        // DataManager에서 해당 가문의 유닛 리스트를 직접 가져온다.
        List<CharacterData> familyUnits = UnitDataManager.Instance.GetFamilyUnits(familyName);

        if (familyUnits == null || familyUnits.Count < 10)
        {
            Debug.LogError($"❌ 가문 '{familyName}'의 유닛 데이터가 부족합니다.");
            return;
        }

        // JSON 파일의 순서대로 5번부터 10번까지의 유닛을 추가한다.
        int start = 5; // 6번째 캐릭터 (인덱스 5)
        int end = start + 5; // 5개를 추가 (5, 6, 7, 8, 9, 10)

        for (int i = start; i < end; i++)
        {
            Unit newUnit = new Unit(familyUnits[i].Unit_ID, familyUnits[i].Rarity, familyUnits[i].Unit_Name);
            user.myUnits.Add(newUnit);
            Debug.Log($"✅ 초기 유닛 추가: {familyUnits[i].Unit_Name}");
        }
        SaveUser();
    }
    
    // 돈 추가/차감 기능도 여기서 호출할 수 있음
    
    public event Action<int> OnMoneyChanged;
    public void AddGold(int amount)
    {
        user.AddGold(amount);
        SaveUser();
        OnMoneyChanged?.Invoke(user.money);
    }

    public bool SpendGold(int amount)
    {
        bool success = user.SpendGold(amount);
        if (success) SaveUser();
        OnMoneyChanged?.Invoke(user.money);
        return success;
        
    }

    public string GetSavePath()
    {
        return savePath;
    }
}
