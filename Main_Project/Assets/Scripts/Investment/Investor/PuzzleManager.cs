using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PuzzleManager : MonoBehaviour
{
    public GameObject investorprefab;
    public Transform[] spawnPoints;
    private Image[] images; // prefab img 저장
    public Image[]  completedInvestorImage; // 왼쪽 page의 완료된 img 배열
    public Transform spawnParent;

    public GameObject[] hidePanels;

    public GameObject negotiationPanelPrefab; // 저장된 nego 프리팹
    public Transform storyUIAnchor; // nego prefab이 생성될 위치
    
    public RandomImageSelector imageSelector;

    private int completedIndex = 0; // 왼쪽 page의 완료된 img 배열 index
    void Start()
    {
        if (imageSelector != null)
        {
            imageSelector.ResetUsedIndices(); // 초기화
        }
        images =  new Image[spawnPoints.Length];
        SpawnNewInvestor();
    }

    public void SpawnNewInvestor()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            // spawnParent 아래에 프리팹 생성
            GameObject obj = Instantiate(investorprefab, spawnParent);

            // UI 위치 설정: localPosition을 anchoredPosition으로 할당
            RectTransform objRect = obj.GetComponent<RectTransform>();
            objRect.anchoredPosition = ((RectTransform)spawnPoints[i]).localPosition;

            // InvestorUnit 스크립트 초기화
            InvestorUnit unit = obj.GetComponent<InvestorUnit>();
            unit.Init(this);

            // 이미지 Random 선택
            Image img = obj.GetComponentInChildren<Image>();
            if (img != null && imageSelector != null)
            {
                int randIndex = imageSelector.GetUniqueRandomIndex();
                if (randIndex >= 0)
                {
                    img.sprite = imageSelector.sprites[randIndex];
                }
            }
        }
    }

    public void HideOtherInvestors(GameObject exceptThis)
    {
        foreach (Transform child in spawnParent) // spawnParent = investors
        {
            Destroy(exceptThis);
            child.gameObject.SetActive(false); // 또는 Destroy(child.gameObject);
        }
    }

    public void ShowOtherInvestors(GameObject exceptThis)
    {
        foreach (Transform child in spawnParent)
        {
            if (child.gameObject != exceptThis)
                child.gameObject.SetActive(true);
        }
        
    }
}