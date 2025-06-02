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
    private List<int> nameIndexes = new List<int>();
    public MoneyManager moneyManager;
    public GameObject loadingPanel;
    private readonly string[] names_investor =
    {
        "플라비우스", "율리우스", "클라우디우스", "플라미니누스",
        "프리스쿠스", "루킬리우스", "브루투스", "카이킬리우스"
    };
    void Start()
    {
        if (imageSelector != null)
        {
            imageSelector.ResetUsedIndices(); // 초기화
        }
        nameIndexes.Clear();
        for(int i = 0; i < names_investor.Length; i++)
            nameIndexes.Add(i);
        Shuffle(nameIndexes);
        
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
            int nameIndex = nameIndexes[i];
            InvestorUnit unit = obj.GetComponent<InvestorUnit>();
            unit.moneyManager = moneyManager;
            unit.Init(this, names_investor[nameIndex], nameIndex);
        }
    }

    void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }
    public void MoveInvestorImageToCompletedSlot(GameObject investorGO)
    {
        if (completedIndex >= completedInvestorImage.Length)
            return;

        // 투자자에서 이미지 가져오기
        Image originalImage = investorGO.GetComponentInChildren<Image>();
        if (originalImage == null || originalImage.sprite == null)
            return;

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
        completedIndex++;
    }
    public void HideOtherInvestors(GameObject exceptThis = null)
    {
        foreach (Transform child in spawnParent)
        {
            if (child.gameObject != exceptThis) // 자기 자신은 끄지 않음!
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