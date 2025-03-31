using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class stateViewer : MonoBehaviour
{
    public Text stateText;
    void Update()
    {
        stateText.text=$"공격력 : {GameObject.Find("DataSaver").GetComponent<Data>().attack[GameObject.Find("DataSaver").GetComponent<Data>().playerCount]} 방어력 : {GameObject.Find("DataSaver").GetComponent<Data>().defense[GameObject.Find("DataSaver").GetComponent<Data>().playerCount]}";
    }
}
