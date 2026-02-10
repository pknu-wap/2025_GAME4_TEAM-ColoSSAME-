using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scripts.Team.FighterViewer;

public class GetStateShow : MonoBehaviour
{
    public GameObject canvas;
    public GameObject drawfight;
    public GameObject[] allPanels; //ui 설정
   

    [SerializeField] private UnitViewer unitViewer;
    private const int MAX_UNIT_COUNT = 15;

    public void BackButton()
    {
        drawfight.SetActive(false);
        canvas.SetActive(true);
    }

    //아직 테스트는 X
    public void EnterButton()
    {
        if (unitViewer.userData.myUnits.Count >= MAX_UNIT_COUNT)
        {
            Debug.Log("유닛 보유 수가 최대입니다.");
            return;
        }

        canvas.SetActive(false);
        drawfight.SetActive(true);
    }

     public void ShowPanel()//ui 창 변경
    {
        foreach (GameObject panel in allPanels)
        {
            panel.SetActive(true);
        }
        
    }

    

}
