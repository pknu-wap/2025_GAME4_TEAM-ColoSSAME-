using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerViewer : MonoBehaviour
{
    public GameObject training;
    private bool isActive;

    public void player1()
    {
        GameObject.Find("DataSaver").GetComponent<Data>().playerCount = 0;
    }
    public void player2()
    {
        GameObject.Find("DataSaver").GetComponent<Data>().playerCount = 1;
    }
    public void click()
    {   
        //training.SetActive(isActive);
        //isActive = !isActive;
    }
}
