using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
//창 변경(ex)홈 -> 소식통)

public class Hide : MonoBehaviour
{
    public GameObject targetObject;
    public GameObject startObject;
    public GameObject otherObject1;
    public GameObject otherObject2;
    public GameObject otherObject3;
    public GameObject otherObject4;

    //글꼴 변경
    public TextMeshProUGUI mainText;
    public TextMeshProUGUI text1;
    public TextMeshProUGUI text2;
    public TextMeshProUGUI text3;
    public TextMeshProUGUI text4;

    public void hide()
    {
        targetObject.SetActive(true);
        startObject.SetActive(false);
        otherObject1.SetActive(false);
        otherObject2.SetActive(false);
        otherObject3.SetActive(false);
        otherObject4.SetActive(false);
        
        //글꼴 변경
        mainText.fontSize = 30;
        text1.fontSize = 25;
        text2.fontSize = 25;
        text3.fontSize = 25;
        text4.fontSize = 25;
    } 
}
