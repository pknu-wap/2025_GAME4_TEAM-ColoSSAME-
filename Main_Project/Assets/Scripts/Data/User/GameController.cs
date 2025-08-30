using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameController : MonoBehaviour
{
    public UserManager userManager;
    
    void Start()
    {
        if (userManager == null)
            userManager = UserManager.Instance;
    }
    public void RewardGold(int amount)
    {
        userManager.AddGold(amount);  // UserManager 통해 골드 추가
        Debug.Log($"게임 보상 골드 {amount} 획득!");
    }
}