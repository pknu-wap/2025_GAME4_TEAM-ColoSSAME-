using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetStateShow : MonoBehaviour
{
    public GameObject canvas;
    public GameObject drawfight;

    public void BackButton()
    {
        drawfight.SetActive(false);
        canvas.SetActive(true);
    }

    public void EnterButton()
    {
        canvas.SetActive(false);
        drawfight.SetActive(true);
    }
}
