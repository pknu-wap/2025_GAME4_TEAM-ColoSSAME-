using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvestorUImanager : MonoBehaviour
{
    public GameObject[] allPanels; // 숨기고 보여줄 UI들
    public GameObject[] initialSubPanels;
    
    public void ShowOnlyThis(GameObject panelToShow)
    {
        if (allPanels != null)
        {
            foreach (GameObject panel in allPanels)
            {
                if (panel != null)
                    panel.SetActive(false);
            }
        }

        if (panelToShow != null)
            panelToShow.SetActive(true);
    }

    public void ShowInitialPanel()
    {
        if (initialSubPanels != null)
        {
            foreach (GameObject panel in initialSubPanels)
            {
                if (panel != null)
                    panel.SetActive(true);
            }
        }
    }
    
}
