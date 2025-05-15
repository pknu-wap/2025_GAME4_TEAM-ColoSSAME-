using System;
using UnityEngine;

public class NegoController : MonoBehaviour
{
    public Action OnPuzzleComplete;

    public void CompletePuzzle()
    {
        // 이 메서드를 퍼즐 3단계 완료 시 호출
        OnPuzzleComplete?.Invoke();
        gameObject.SetActive(false); // UI 닫기
    }
}