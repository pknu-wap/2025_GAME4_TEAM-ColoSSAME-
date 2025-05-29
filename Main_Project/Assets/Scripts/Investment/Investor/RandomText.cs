using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RandomText : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI infoText;

    // 임의의 이름과 설명 리스트
    private string[] names = { "1", "2", "3", "4", "5", "6", "7", "8" };
    private string[] infos = {
        "A",
        "B",
        "C",
        "D",
        "E",
        "F",
        "G",
        "H"
    };

    void Start()
    {
        // 무작위 인덱스 추출
        int index = Random.Range(0, names.Length);

        // Text(TMP) 오브젝트에 텍스트 지정
        nameText.text = names[index];
        infoText.text = infos[index];
    }
}

