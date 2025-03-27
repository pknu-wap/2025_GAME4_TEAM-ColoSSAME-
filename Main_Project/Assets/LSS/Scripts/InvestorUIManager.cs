using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvestorUImanager : MonoBehaviour
{
    public GameObject[] allPanels; // 숨기고 보여줄 UI들

    public void ShowOnlyThis(GameObject panelToShow)
    {
        foreach (GameObject panel in allPanels)
        {
            panel.SetActive(false);
        }
        panelToShow.SetActive(true);
    }
}
