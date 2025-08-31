using UnityEngine;
using System.IO;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class UserManager : MonoBehaviour
{
    public static UserManager Instance { get; private set; }

    public User user;

    private string savePath;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            savePath = Path.Combine(Application.persistentDataPath, "UserSave.json");
            LoadUser();

            Debug.Log("✅ UserManager 초기화 완료");
        }
        else
        {
            Destroy(gameObject);
        }
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

        SaveUser();
        Debug.Log($"✅ 새로운 유저 생성: {userName}");
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
    public void AddGold(int amount)
    {
        user.AddGold(amount);
        SaveUser();
    }

    public bool SpendGold(int amount)
    {
        bool success = user.SpendGold(amount);
        if (success) SaveUser();
        return success;
    }

    public string GetSavePath()
    {
        return savePath;
    }
}
