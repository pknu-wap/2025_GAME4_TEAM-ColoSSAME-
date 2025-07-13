using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainBack : MonoBehaviour
{
    public GameObject[] allPanels;

    public void backList()
    {
        foreach (GameObject panel in allPanels)
        {
            panel.SetActive(true);
        }
    }

}
