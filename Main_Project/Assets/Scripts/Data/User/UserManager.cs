using UnityEngine;
using System.IO;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;

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
            inventory = new Dictionary<string, int>()
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
