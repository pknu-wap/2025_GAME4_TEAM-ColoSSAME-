using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public GameObject prefab;
    public Transform[] spawnPoints;
    public Transform spawnParent;
    void Start()
    {
        SpawnNewInvestor();
    }

    public void SpawnNewInvestor()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            // spawnParent 아래에 프리팹 생성
            GameObject obj = Instantiate(prefab, spawnParent);

            // UI 위치 설정: localPosition을 anchoredPosition으로 할당
            RectTransform objRect = obj.GetComponent<RectTransform>();
            objRect.anchoredPosition = ((RectTransform)spawnPoints[i]).localPosition;

            // InvestorUnit 스크립트 초기화
            InvestorUnit unit = obj.GetComponent<InvestorUnit>();
            unit.Init(this);
        }
    }
}