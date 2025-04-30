using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
//창 변경(ex)홈 -> 소식통)

public class Hide : MonoBehaviour
{
    public GameObject[] allPanels; //ui 설정
    public TextMeshProUGUI[] allTexts; //글자 설정

    public void ShowPanel(GameObject panelToShow)//ui 창 변경
    {
        foreach (GameObject panel in allPanels)
        {
            panel.SetActive(false);
        }
        panelToShow.SetActive(true);
    }

    public void ChangeText(TextMeshProUGUI textToChange)//클릭 시 글씨 크기 변경
    {
        foreach (TextMeshProUGUI text in allTexts)
        {
            text.fontSize = 25;
        }
        textToChange.fontSize = 30;
    } 
}
