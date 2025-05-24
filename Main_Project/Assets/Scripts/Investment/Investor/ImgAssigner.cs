using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class AssinImg : MonoBehaviour
{
    public RandomImageSelector imageSelector; // 씬 내 RandomImageSelector 참조

    private Image img;

    void Start()
    {
        img = GetComponent<Image>();
        if (img == null)
        {
            Debug.LogWarning("Image 컴포넌트가 없습니다.");
            return;
        }

        if (imageSelector == null)
        {
            Debug.LogWarning("ImageSelector가 할당되지 않았습니다.");
            return;
        }

        int randIndex = imageSelector.GetUniqueRandomIndex();
        if (randIndex >= 0 && randIndex < imageSelector.sprites.Length)
        {
            img.sprite = imageSelector.sprites[randIndex];
        }
        else
        {
            Debug.LogWarning("유효하지 않은 랜덤 인덱스입니다.");
        }
    }
}
