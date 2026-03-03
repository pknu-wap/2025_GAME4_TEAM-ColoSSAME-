using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using Scripts.Team.FighterViewer;

public class UnitSellManager : MonoBehaviour
{
    public UnitViewer unitViewer;

    [SerializeField]private int baseSellPrice = 100;

    public void SellSelectedUnit()
    {
        int index = unitViewer.selectedIndex;

        var userManager = UserManager.Instance;
        var myUnits = userManager.user.myUnits;

        Unit unit = myUnits[index];

        int sellPrice = CalculateSellPrice(unit);

        userManager.AddGold(sellPrice);

        //선수 삭제
        myUnits.RemoveAt(index);

        SaveUserData();


        unitViewer.UnitShow();

        unitViewer.selectedIndex = -1;
    }

    //판매가격
    int CalculateSellPrice(Unit unit)
    {
        return baseSellPrice * unit.rarity;
    }

    void SaveUserData()
    {
        UserManager.Instance.SaveUser();
    }
}
