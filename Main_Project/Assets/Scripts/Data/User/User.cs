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
    
    public User() { }
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
}
