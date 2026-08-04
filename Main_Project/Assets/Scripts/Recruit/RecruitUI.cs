using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecruitUI : MonoBehaviour
{
    private const int FiveStarRarity = 5;
    private const int FourStarRarity = 4;

    [Header("참조")]
    [SerializeField] private RecruitManager recruitManager;
    [SerializeField] private Button recruitButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_Text resultText;

    [SerializeField] private int recruitCost;

    private UserManager user = UserManager.Instance;
    private void Awake()
    {
        if (recruitButton != null)
        {
            recruitButton.onClick.AddListener(OnRecruitButtonClicked);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }
    }

    private void OnDestroy()
    {
        if (recruitButton != null)
        {
            recruitButton.onClick.RemoveListener(OnRecruitButtonClicked);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(OnBackButtonClicked);
        }
    }

    private void OnRecruitButtonClicked()
    {
        if (recruitManager == null)
        {
            Debug.LogWarning("[RecruitUI] RecruitManager가 연결되어 있지 않습니다.");
            return;
        }
        if (UserManager.Instance.SpendGold(recruitCost))
        {
            RecruitResult result = recruitManager.Recruit();
            DisplayResult(result);
        }
        else
        {
            // 돈 부족 Text
        }
    }

    private void DisplayResult(RecruitResult result)
    {
        if (resultText == null)
        {
            return;
        }

        if (result == null)
        {
            resultText.text = "뽑기 대상이 없습니다.";
            return;
        }

        if (result.IsDuplicate)
        {
            resultText.text = $"중복!\n승급석 {result.RewardStoneAmount}개 지급";
        }
        else
        {
            resultText.text = $"{GetRarityLabel(result.AcquiredRarity)}\n검투사 {result.Character.Unit_Name} 획득!";
        }
    }

    private void OnBackButtonClicked()
    {
        gameObject.SetActive(false);
    }
    
    private string GetRarityLabel(int rarity)
    {
        switch (rarity)
        {
            case FiveStarRarity:
                return "5성";
            case FourStarRarity:
                return "4성";
            default:
                return "3성";
        }
    }
}