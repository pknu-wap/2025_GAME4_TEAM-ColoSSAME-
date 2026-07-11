using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
//창 변경(ex)홈 -> 소식통)

public class Hide : MonoBehaviour
{
    public GameObject[] allPanels; //ui 설정

    public void ShowPanel(GameObject panelToShow)//ui 창 변경
    {
        foreach (GameObject panel in allPanels)
        {
            panel.SetActive(false);
        }
        panelToShow.SetActive(true);
    }


}
