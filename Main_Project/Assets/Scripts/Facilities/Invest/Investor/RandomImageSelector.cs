using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomImageSelector : MonoBehaviour
{
// 스프라이트 배열은 외부에서 할당
    public Sprite[] sprites;

    // 중복 방지를 위한 사용된 인덱스 목록
    private List<int> usedIndices = new List<int>();

    // 중복 없는 랜덤 인덱스 반환
    public int GetUniqueRandomIndex()
    {
        if (usedIndices.Count >= sprites.Length)
        {
            // 모든 인덱스를 사용한 경우 초기화(재사용 가능하도록)
            usedIndices.Clear();
        }

        int randIndex;

        do
        {
            randIndex = Random.Range(0, sprites.Length);
        } while (usedIndices.Contains(randIndex));

        usedIndices.Add(randIndex);

        return randIndex;
    }

    // 필요시 사용된 인덱스 초기화 함수
    public void ResetUsedIndices()
    {
        usedIndices.Clear();
    }
}
