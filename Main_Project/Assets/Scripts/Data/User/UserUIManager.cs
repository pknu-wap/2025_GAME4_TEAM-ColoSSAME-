using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UserUIManager : MonoBehaviour
{
    public static UserUIManager Instance;

    public UserManager userManager;
    
    public TMP_Text name;
    public TMP_Text level;
    public TMP_Text userMoney;
    
    void Start()
    {
        if (userManager == null)
            userManager = UserManager.Instance;
        
        UserManager.Instance.OnMoneyChanged += SetUserUI;
        
        UpdateAllUI();

    }

    public void UpdateAllUI()
    {
        SetUserUI(userManager.user.money);
    }

    public void SetUserUI(int money)
    {
        name.text = userManager.user.userName;
        level.text = "레벨: " + userManager.user.level;
        userMoney.text = "돈: " + userManager.user.money;

    }
    
    private void OnDestroy()
    {
        if (UserManager.Instance != null)
            UserManager.Instance.OnMoneyChanged -= SetUserUI;
    }

    
}
