using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class User
{
    public string userName;
    public int level = 1;
    public float exp = 0;
    public int money = 100;
    public Dictionary<string, int> inventory; 
    
    public List<Unit> myUnits;
    
    public Dictionary<string, int> achievementProgress; // 그룹ID -> 현재 단계 인덱스(0=1단계)
    public Dictionary<string, int> usedItemCounts;      // 아이템ID(string) 사용 누적 횟수
    public List<BuildingLevelSave> buildingLevels = new List<BuildingLevelSave>();
    
    [Serializable]
    public class BuildingLevelSave
    {
        public BuildingType type;
        public int level;
    }
    
    public User()
    {
        EnsureDictionaries();
    }
    public User(string nickname)
    {
        this.userName = nickname;
        level = 1;
        exp = 0;
        money = 0;
        inventory = new Dictionary<string, int>();
    }
    
    // 골드 추가
    public void AddGold(int amount)
    {
        money += amount;
        if (money < 0) money = 0;
        Debug.Log($"✅ 골드 +{amount}, 총 골드: {money}");
    }

    // 골드 차감
    public bool SpendGold(int amount)
    {
        if (money >= amount)
        {
            money -= amount;
            Debug.Log($"✅ 골드 -{amount}, 남은 골드: {money}");
            return true;
        }
        else
        {
            Debug.LogWarning($"❌ 골드 부족: {money} / {amount}");
            return false;
        }
    }
    public void EnsureDictionaries()
    {
        if (inventory == null) inventory = new Dictionary<string, int>();
        if (myUnits == null) myUnits = new List<Unit>();

        if (achievementProgress == null) achievementProgress = new Dictionary<string, int>();
        if (usedItemCounts == null) usedItemCounts = new Dictionary<string, int>();
    }
}
