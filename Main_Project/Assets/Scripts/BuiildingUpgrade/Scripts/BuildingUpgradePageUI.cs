using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// BuildingUpgrade 페이지 UI 담당
/// - Legacy Text 사용
/// - 시설 슬롯(F01, F02...)을 리스트로 받아 하위 Name/Level/SelectButton 자동 탐색
/// - 선택/업그레이드/상세 갱신 처리
/// </summary>
public class BuildingUpgradePageUI : MonoBehaviour
{
    [Header("필수")]
    [SerializeField] private BuildingUpgradeManager upgradeManager;

    [Header("LeftPage - 시설 슬롯 루트들 (F01, F02, F03...)")]
    [SerializeField] private List<Transform> facilitySlots = new List<Transform>();

    [Header("시설 슬롯 순서에 대응하는 BuildingType 매핑")]
    [Tooltip("facilitySlots[0]은 Shop, facilitySlots[1]은 Training ... 이런 식으로 매핑")]
    [SerializeField] private List<BuildingType> facilityTypes = new List<BuildingType>();

    [Header("RightPage - Detail (Legacy Text)")]
    [SerializeField] private Text detailNameText;

    [SerializeField] private Text curLevelText;
    [SerializeField] private Text curEffectText;

    [SerializeField] private Text nextLevelText;
    [SerializeField] private Text nextEffectText;

    [SerializeField] private Text costText;

    [Header("RightPage - Buttons")]
    [SerializeField] private Button upgradeButton;

    // 슬롯에서 자동으로 찾아 캐싱할 정보
    private readonly List<SlotCache> slotCaches = new List<SlotCache>();

    private BuildingType selectedType;

    private void Awake()
    {
        CacheSlots();
        BindSlotButtons();
        BindUpgradeButton();
    }

    private void OnEnable()
    {
        if (upgradeManager != null)
        {
            upgradeManager.OnBuildingUpgraded -= OnBuildingUpgraded;
            upgradeManager.OnBuildingUpgraded += OnBuildingUpgraded;
        }

        // 기본 선택: 첫 슬롯이 있으면 첫 슬롯 타입
        if (facilitySlots.Count > 0)
            SelectByIndex(0);
        else
            selectedType = BuildingType.Shop;

        RefreshAll();
    }

    private void OnDisable()
    {
        if (upgradeManager != null)
            upgradeManager.OnBuildingUpgraded -= OnBuildingUpgraded;
    }

    // =============================
    // 슬롯 자동 캐싱/바인딩
    // =============================

    private void CacheSlots()
    {
        slotCaches.Clear();

        for (int i = 0; i < facilitySlots.Count; i++)
        {
            Transform slotRoot = facilitySlots[i];
            if (slotRoot == null)
            {
                slotCaches.Add(new SlotCache(null, null, null));
                continue;
            }

            Text name = null;
            Text level = null;
            Button selectBtn = null;

            // ✅ Panel 제거 → slotRoot 바로 아래에서 찾는다
            Transform nameTf = slotRoot.Find("Name");
            Transform levelTf = slotRoot.Find("Level");
            Transform btnTf   = slotRoot.Find("SelectButton");

            if (nameTf != null) name = nameTf.GetComponent<Text>();
            if (levelTf != null) level = levelTf.GetComponent<Text>();
            if (btnTf != null) selectBtn = btnTf.GetComponent<Button>();

            slotCaches.Add(new SlotCache(name, level, selectBtn));
        }
    }

    private void BindSlotButtons()
    {
        for (int i = 0; i < slotCaches.Count; i++)
        {
            int idx = i;
            Button btn = slotCaches[idx].selectButton;
            if (btn == null) continue;

            // 기존 리스너 건드리지 않기 위해 RemoveAllListeners는 안 씀.
            // 이 UI가 버튼을 독점한다면 RemoveAllListeners 써도 됨.
            btn.onClick.AddListener(() => SelectByIndex(idx));
        }
    }

    private void BindUpgradeButton()
    {
        if (upgradeButton == null) return;
        upgradeButton.onClick.RemoveListener(OnClickUpgrade);
        upgradeButton.onClick.AddListener(OnClickUpgrade);
    }

    // =============================
    // 선택/갱신
    // =============================

    private void SelectByIndex(int index)
    {
        selectedType = GetTypeByIndex(index);
        RefreshDetail();
    }

    private BuildingType GetTypeByIndex(int index)
    {
        if (facilityTypes != null && index >= 0 && index < facilityTypes.Count)
            return facilityTypes[index];

        // 매핑이 부족하면 안전 기본값
        return BuildingType.Shop;
    }

    private void RefreshAll()
    {
        RefreshLeftList();
        RefreshDetail();
    }

    private void RefreshLeftList()
    {
        if (upgradeManager == null) return;

        for (int i = 0; i < slotCaches.Count; i++)
        {
            BuildingType t = GetTypeByIndex(i);

            int lv = upgradeManager.GetCurrentLevel(t);

            // 표시명 (원하는 이름이면 여기만 바꾸면 됨)
            string displayName = GetDisplayName(t);

            if (slotCaches[i].nameText != null)
                slotCaches[i].nameText.text = displayName;

            if (slotCaches[i].levelText != null)
                slotCaches[i].levelText.text = $"Lv {lv}";
        }
    }

    private void RefreshDetail()
    {
        if (upgradeManager == null) return;

        // 시설명
        if (detailNameText != null)
            detailNameText.text = GetDisplayName(selectedType);

        // 현재
        int curLv = upgradeManager.GetCurrentLevel(selectedType);
        float curDiscount = upgradeManager.GetCurrentDiscountRate(selectedType);

        if (curLevelText != null) curLevelText.text = $"현재 레벨 : Lv {curLv}";
        if (curEffectText != null) curEffectText.text = $"현재 효과 : 할인 {(curDiscount * 100f):0}%";

        // 다음
        if (upgradeManager.TryGetNextUpgradeInfo(selectedType, out int nextLv, out int cost, out float nextDiscount))
        {
            if (nextLevelText != null) nextLevelText.text = $"다음 레벨 : Lv {nextLv}";
            if (nextEffectText != null) nextEffectText.text = $"다음 효과 : 할인 {(nextDiscount * 100f):0}%";
            if (costText != null) costText.text = $"비용 : {cost}";

            if (upgradeButton != null) upgradeButton.interactable = true;
        }
        else
        {
            if (nextLevelText != null) nextLevelText.text = "다음 레벨 : MAX";
            if (nextEffectText != null) nextEffectText.text = "다음 효과 : -";
            if (costText != null) costText.text = "비용 : -";

            if (upgradeButton != null) upgradeButton.interactable = false;
        }
    }

    private void OnClickUpgrade()
    {
        if (upgradeManager == null) return;

        bool ok = upgradeManager.Upgrade(selectedType);
        if (ok) RefreshAll();
    }

    private void OnBuildingUpgraded(BuildingType type, int level)
    {
        // 어떤 시설이든 업그레이드되면 전체 갱신
        RefreshAll();
    }

    private string GetDisplayName(BuildingType t)
    {
        switch (t)
        {
            case BuildingType.Shop: return "상점";
            case BuildingType.Training: return "훈련소";
            default: return t.ToString();
        }
    }

    // =============================
    // 내부 캐시 구조
    // =============================
    private class SlotCache
    {
        public Text nameText;
        public Text levelText;
        public Button selectButton;

        public SlotCache(Text nameText, Text levelText, Button selectButton)
        {
            this.nameText = nameText;
            this.levelText = levelText;
            this.selectButton = selectButton;
        }
    }
}