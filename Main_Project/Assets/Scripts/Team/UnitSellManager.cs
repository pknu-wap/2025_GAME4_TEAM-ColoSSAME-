using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using Scripts.Team.FighterViewer;

public class UnitSellManager : MonoBehaviour
{
    public UnitViewer unitViewer;
    public UserData userData;

    [SerializeField]private int baseSellPrice = 100;

    public void SellSelectedUnit()
    {
        userData = unitViewer.userData;

        int index = unitViewer.selectedIndex;

        UnitData unit = userData.myUnits[index];
        int sellPrice = CalculateSellPrice(unit);

        userData.money += sellPrice;

        //선수 삭제
        userData.myUnits.RemoveAt(index);

        SaveUserData();


        unitViewer.UnitShow();

        unitViewer.selectedIndex = -1;
    }

    //판매가격
    int CalculateSellPrice(UnitData unit)
    {
        return baseSellPrice * unit.rarity;
    }

    void SaveUserData()
    {
        string path = Path.Combine(Application.persistentDataPath, "UserSave.json");
        File.WriteAllText(path, JsonConvert.SerializeObject(userData, Formatting.Indented));
    }
}
