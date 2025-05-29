using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PuzzleManager : MonoBehaviour
{
    public GameObject investorprefab;
    public Transform[] spawnPoints;
    public Image[] completedInvestorImage;
    public Transform spawnParent;
    
    public GameObject[] hidePanels;

    public GameObject negotiationPanelPrefab; // 저장된 nego 프리팹
    public Transform storyUIAnchor; // nego prefab이 생성될 위치
    
    public RandomImageSelector imageSelector;

    private int completedIndex = 0; // 왼쪽 page의 완료된 img 배열 index
    private List<Sprite> usedSprites = new List<Sprite>();
    void Start()
    {
        if (imageSelector != null)
        {
            imageSelector.ResetUsedIndices(); // 초기화
        }
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
            // InvestorUnit 스크립트 초기화
            InvestorUnit unit = obj.GetComponent<InvestorUnit>();
            unit.Init(this);
        }
    }
    public void MoveInvestorImageToCompletedSlot(GameObject investorGO)
    {
        if (completedIndex >= completedInvestorImage.Length)
        {
            Debug.LogWarning("✅ [PuzzleManager] 모든 슬롯이 채워졌습니다.");
            return;
        }

        // 투자자에서 이미지 가져오기
        Image originalImage = investorGO.GetComponentInChildren<Image>();
        if (originalImage == null || originalImage.sprite == null)
        {
            Debug.LogError("❌ [PuzzleManager] 원본 이미지 또는 스프라이트가 null입니다.");
            return;
        }
        else
        {
            Debug.Log($"✅ 원본 스프라이트: {originalImage.sprite.name}");
        }

        // 슬롯 이미지 설정
        Image slotImage = completedInvestorImage[completedIndex];
        slotImage.sprite = originalImage.sprite;

        // UI 설정 보정
        slotImage.enabled = true;
        slotImage.preserveAspect = true;
        slotImage.rectTransform.sizeDelta = new Vector2(120, 120);

        // 혹시 투명도 문제 있을 경우 강제 보정
        Color color = slotImage.color;
        color.a = 1f;
        slotImage.color = color;

        Debug.Log($"✅ [PuzzleManager] 슬롯 {completedIndex} 에 이미지 할당 완료: {originalImage.sprite.name}");

        completedIndex++;
    }

    public void HideOtherInvestors()
    {
        foreach (Transform child in spawnParent)
        {
                child.gameObject.SetActive(false);
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