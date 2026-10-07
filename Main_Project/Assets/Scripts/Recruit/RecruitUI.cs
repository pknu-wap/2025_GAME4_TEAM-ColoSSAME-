using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BattleK.Scripts.Data;
using BattleK.Scripts.Manager;
using BattleK.UI.Book;

public class RecruitUI : MonoBehaviour
{
    private const int FiveStarRarity = 5;
    private const int FourStarRarity = 4;
    private BookPageController pageController;
    [Header("ContentObject")]
    [SerializeField] private GameObject content;
    
    [Header("참조")]
    [SerializeField] private RecruitManager recruitManager;
    [SerializeField] private Button recruitButton;
    [SerializeField] private Button backButton;

    [Header("화면 상태 전환용 오브젝트")]
    [SerializeField] private GameObject idlePrompt;
    [SerializeField] private GameObject resultGroup;

    [Header("결과 표시")]
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Image characterPortraitImage;

    [Header("스탯 표시 (StatA/StatD/StatG/StatH)")]
    [SerializeField] private TMP_Text statAttackText;
    [SerializeField] private TMP_Text statDefenseText;
    [SerializeField] private TMP_Text statAGIText;
    [SerializeField] private TMP_Text statHealthText;

    [Header("등장 연출 (CharacterImg / CharacterStats)")]
    [SerializeField] private CanvasGroup characterImageGroup;
    [SerializeField] private CanvasGroup characterStatsGroup;
    [SerializeField] private float revealFadeDuration;

    private Coroutine revealRoutine;

    [Header("뽑기 비용")]
    [SerializeField] private int recruitCost;

    private readonly AddressableAssetLoader<Sprite> portraitLoader = new AddressableAssetLoader<Sprite>();

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

    private void OnEnable()
    {
        ShowIdleState();
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

        portraitLoader.ReleaseAll();
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
            ShowResultState();
        }
        else
        {
            // TODO: 돈 부족 안내 Text 표시
        }
    }
    
    private void OnBackButtonClicked()
    {
        if (pageController == null)
            pageController = GetComponentInParent<BookPageController>();

        if (pageController != null)
        {
            pageController.GoBack();
            return;
        }

        if (content != null) content.SetActive(false);
    }

    private void ShowIdleState()
    {
        if (idlePrompt != null)
        {
            idlePrompt.SetActive(true);
        }

        if (resultGroup != null)
        {
            resultGroup.SetActive(false);
        }

        if (recruitButton != null)
        {
            recruitButton.interactable = true;
        }
    }

    private void ShowResultState()
    {
        if (idlePrompt != null)
        {
            idlePrompt.SetActive(false);
        }

        if (resultGroup != null)
        {
            resultGroup.SetActive(true);
        }

        if (recruitButton != null)
        {
            recruitButton.interactable = false;
        }
    }
    
    private void DisplayResult(RecruitResult result)
    {
        if (result == null)
        {
            if (resultText != null)
            {
                resultText.text = "뽑기 대상이 없습니다.";
            }

            return;
        }

        if (resultText != null)
        {
            if (result.IsDuplicate)
            {
                string itemLabel = result.RewardItem != null ? result.RewardItem.itemName : "보상";
                resultText.text = $"중복!\n{itemLabel} {result.RewardStoneAmount}개 지급";
            }
            else
            {
                resultText.text = $"{GetRarityLabel(result.AcquiredRarity)}\n검투사 {result.Character.Unit_Name} 획득!";
            }
        }

        ResetRevealAlpha();
        DisplayCharacterPortrait(result.Character);
        DisplayStats(result.Character);
        PlayRevealAnimation();
    }

    private void DisplayStats(CharacterData characterData)
    {
        if (characterData == null)
        {
            return;
        }

        Stat_Distribution stats = characterData.Stat_Distribution;

        if (stats == null)
        {
            Debug.LogWarning($"[RecruitUI] Stat_Distribution이 없습니다: {characterData.Unit_ID}");
            return;
        }

        if (statAttackText != null)
        {
            statAttackText.SetText($"공격력 : {stats.ATK}");
        }

        if (statDefenseText != null)
        {
            statDefenseText.SetText($"방어력 : {stats.DEF}");
        }

        if (statAGIText != null)
        {
            statAGIText.SetText($"민첩성 : {stats.AGI}");
        }

        if (statHealthText != null)
        {
            statHealthText.SetText($"체력 : {stats.HP}");
        }
    }

    private void ResetRevealAlpha()
    {
        if (characterImageGroup != null)
        {
            characterImageGroup.alpha = 0f;
        }

        if (characterStatsGroup != null)
        {
            characterStatsGroup.alpha = 0f;
        }
    }

    private void PlayRevealAnimation()
    {
        if (revealRoutine != null)
        {
            StopCoroutine(revealRoutine);
        }

        revealRoutine = StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        float elapsed = 0f;

        while (elapsed < revealFadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / revealFadeDuration);

            if (characterImageGroup != null)
            {
                characterImageGroup.alpha = alpha;
            }

            if (characterStatsGroup != null)
            {
                characterStatsGroup.alpha = alpha;
            }

            yield return null;
        }

        if (characterImageGroup != null)
        {
            characterImageGroup.alpha = 1f;
        }

        if (characterStatsGroup != null)
        {
            characterStatsGroup.alpha = 1f;
        }

        revealRoutine = null;
    }

    private void DisplayCharacterPortrait(CharacterData characterData)
    {
        if (characterPortraitImage == null || characterData == null)
        {
            return;
        }
        Vector3 scale = characterPortraitImage.rectTransform.localScale;
        characterPortraitImage.rectTransform.localScale = new Vector3(-Mathf.Abs(scale.x), scale.y, scale.z);
        StartCoroutine(LoadPortraitRoutine(characterData));
    }

    private IEnumerator LoadPortraitRoutine(CharacterData characterData)
    {
        yield return CharacterInfoProvider.LoadPortraitAsync(
            portraitLoader,
            characterData,
            sprite => characterPortraitImage.sprite = sprite,
            () => Debug.LogWarning($"[RecruitUI] 캐릭터 초상화 로드 실패: {characterData.Unit_ID}"));
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