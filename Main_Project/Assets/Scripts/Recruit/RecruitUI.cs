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
    [SerializeField] private TextToastUI toastUI;

    [Header("보유 한도 초과 시 교체 팝업")]
    [SerializeField] private RecruitReplaceUI replaceUI;

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
    private bool isRevealing;
    private RecruitResult pendingReplacement;

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
        if (UserManager.Instance != null)
        {
            UserManager.Instance.OnMoneyChanged += HandleMoneyChanged;
        }

        ShowIdleState();

        if (replaceUI != null)
        {
            StartCoroutine(HideReplaceUINextFrame());
        }
    }

    private IEnumerator HideReplaceUINextFrame()
    {
        yield return null;

        if (pendingReplacement == null)
        {
            replaceUI.Hide();
        }
    }

    private void OnDisable()
    {
        if (UserManager.Instance != null)
        {
            UserManager.Instance.OnMoneyChanged -= HandleMoneyChanged;
        }

        if (pendingReplacement != null)
        {
            RecruitResult pending = pendingReplacement;
            pendingReplacement = null;

            recruitManager.CancelReplacement(pending);

            if (replaceUI.transform.IsChildOf(transform)) replaceUI.Release();
            else replaceUI.Hide();
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

        portraitLoader.ReleaseAll();
    }
    
    private void OnRecruitButtonClicked()
    {
        if (recruitManager == null)
        {
            Debug.LogWarning("[RecruitUI] RecruitManager가 연결되어 있지 않습니다.");
            return;
        }

        if (pendingReplacement != null)
        {
            return;
        }

        RecruitResult result = recruitManager.Recruit(recruitCost);

        if (result.Status != RecruitStatus.Success)
        {
            ShowFailureMessage(result.Status);
            RefreshRecruitButton();
            return;
        }

        if (result.RequiresReplacement)
        {
            BeginReplacement(result);
            return;
        }

        ShowRecruitResult(result);
    }

    private void ShowRecruitResult(RecruitResult result)
    {
        DisplayResult(result);
        ShowResultState();
    }

    private void BeginReplacement(RecruitResult pending)
    {
        if (replaceUI == null)
        {
            Debug.LogWarning("[RecruitUI] RecruitReplaceUI가 연결되어 있지 않아 중복 보상으로 처리합니다.");
            recruitManager.CancelReplacement(pending);
            ShowRecruitResult(pending.AsDuplicateReward());
            return;
        }

        pendingReplacement = pending;
        RefreshRecruitButton();
        replaceUI.Show(pending.Character, HandleReplaceConfirmed, HandleReplaceCanceled);
    }

    private void HandleReplaceConfirmed(string replacedUnitId)
    {
        RecruitResult pending = pendingReplacement;
        pendingReplacement = null;

        if (pending == null)
        {
            return;
        }

        if (recruitManager.ConfirmReplacement(pending, replacedUnitId))
        {
            ShowRecruitResult(pending.AsAcquired());
            return;
        }

        Debug.LogWarning("[RecruitUI] 유닛 교체에 실패해 중복 보상으로 처리합니다.");
        recruitManager.CancelReplacement(pending);
        ShowRecruitResult(pending.AsDuplicateReward());
    }

    private void HandleReplaceCanceled()
    {
        RecruitResult pending = pendingReplacement;
        pendingReplacement = null;

        if (pending == null)
        {
            return;
        }

        recruitManager.CancelReplacement(pending);
        ShowRecruitResult(pending.AsDuplicateReward());
    }

    private void ShowFailureMessage(RecruitStatus status)
    {
        string message;

        switch (status)
        {
            case RecruitStatus.NotEnoughGold:
                message = Loc.Get("recruit_no_gold");
                break;
            case RecruitStatus.NoCandidates:
                message = Loc.Get("recruit_no_candidates");
                break;
            default:
                message = Loc.Get("recruit_invalid");
                break;
        }

        if (toastUI != null)
        {
            toastUI.Show(message, 1.5f);
        }
        else
        {
            Debug.Log($"[RecruitUI] {message}");
        }
    }

    private void HandleMoneyChanged(int money)
    {
        RefreshRecruitButton();
    }

    private void RefreshRecruitButton()
    {
        if (recruitButton == null)
        {
            return;
        }

        bool hasGold = UserManager.Instance != null
            && UserManager.Instance.user != null
            && UserManager.Instance.user.money >= recruitCost;

        recruitButton.interactable = hasGold && !isRevealing && pendingReplacement == null;
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

        isRevealing = false;
        revealRoutine = null;
        RefreshRecruitButton();
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

        RefreshRecruitButton();
    }
    
    private void DisplayResult(RecruitResult result)
    {
        if (resultText != null)
        {
            if (result.IsDuplicate)
            {
                string itemLabel = result.RewardItem != null ? Loc.ItemName(result.RewardItem) : Loc.Get("recruit_reward");
                resultText.text = Loc.Get("recruit_duplicate", itemLabel, result.RewardStoneAmount);
            }
            else
            {
                resultText.text = Loc.Get("recruit_acquired", GetRarityLabel(result.AcquiredRarity), Loc.UnitName(result.Character.Unit_ID, result.Character.Unit_Name));
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
            statAttackText.SetText(Loc.Get("stat_atk", stats.ATK));
        }

        if (statDefenseText != null)
        {
            statDefenseText.SetText(Loc.Get("stat_def", stats.DEF));
        }

        if (statAGIText != null)
        {
            statAGIText.SetText(Loc.Get("stat_agi", stats.AGI));
        }

        if (statHealthText != null)
        {
            statHealthText.SetText(Loc.Get("stat_hp", stats.HP));
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

        isRevealing = true;
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
        isRevealing = false;
        RefreshRecruitButton();
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
                return Loc.Get("rarity_5");
            case FourStarRarity:
                return Loc.Get("rarity_4");
            default:
                return Loc.Get("rarity_3");
        }
    }
}