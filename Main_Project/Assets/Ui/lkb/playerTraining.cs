using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerTraining : MonoBehaviour
{
    public void easyClick()
    {
        GameObject.Find("DataSaver").GetComponent<Data>().attackTraining[GameObject.Find("DataSaver").GetComponent<Data>().playerCount] = 1;
    }

    public void normalClick()
    {
        GameObject.Find("DataSaver").GetComponent<Data>().attackTraining[GameObject.Find("DataSaver").GetComponent<Data>().playerCount] = 2;
    }

    public void hardClick()
    {
        GameObject.Find("DataSaver").GetComponent<Data>().attackTraining[GameObject.Find("DataSaver").GetComponent<Data>().playerCount] = 3;
    }
}
