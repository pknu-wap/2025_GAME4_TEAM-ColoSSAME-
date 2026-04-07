using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FighterSlotShowStats : MonoBehaviour, IPointerClickHandler
{
    [Header("슬롯 데이터(유닛 ID)")]
    public FighterSlotData slotData;

    [Header("표시 대상 - 스탯")]
    public Text curLevelText;
    public Text curExpText;
    public Slider expSlider;

    [Header("표시 대상 - 선택된 캐릭터 정보")]
    public Text selectedNameText;
    public Image selectedPortraitImage;

    [Header("표시 대상 - EXP 획득 비용")]
    public Text expCostText;

    public SkillUpgradeManager upgradeManager;
    public int selectedSkillIndex = 0;

    [HideInInspector] public BuildingUpgradeManager buildingUpgradeManager;
    private int GetDiscountedTrainingCost(int baseCost)
    {
        if (buildingUpgradeManager == null)
            buildingUpgradeManager = FindFirstObjectByType<BuildingUpgradeManager>();

        if (buildingUpgradeManager != null)
            return buildingUpgradeManager.GetDiscountedTrainingCost(baseCost);

        return baseCost;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (UserManager.Instance == null || UserManager.Instance.user == null)
        {
            Debug.LogError("❌ UserManager 또는 user가 준비되지 않았습니다.");
            return;
        }

        var units = UserManager.Instance.user.myUnits;
        if (units == null)
        {
            Debug.LogError("❌ myUnits가 null입니다.");
            return;
        }

        if (slotData == null || string.IsNullOrEmpty(slotData.unitId))
        {
            Debug.LogWarning("⚠️ 슬롯에 unitId가 없습니다.");
            return;
        }

        Unit found = null;
        for (int i = 0; i < units.Count; i++)
        {
            if (units[i] != null && units[i].unitId == slotData.unitId)
            {
                found = units[i];
                break;
            }
        }

        if (found == null)
        {
            Debug.LogWarning($"⚠️ myUnits에서 unitId='{slotData.unitId}' 유닛을 찾지 못했습니다.");
            return;
        }

        UserManager.Instance.SetSelectedUnit(slotData.unitId);

        if (curLevelText != null) curLevelText.text = found.level.ToString();
        if (curExpText != null)   curExpText.text   = found.exp.ToString();
        if (expSlider != null)
        {
            expSlider.maxValue = 100f;
            expSlider.value = found.exp;
        }

        if (selectedNameText != null)
            selectedNameText.text = found.unitName;

        if (selectedPortraitImage != null)
        {
            Transform imgTr = transform.Find("playerImage");
            if (imgTr != null)
            {
                Image srcImage = imgTr.GetComponent<Image>();
                if (srcImage != null && srcImage.sprite != null)
                {
                    selectedPortraitImage.sprite = srcImage.sprite;
                    selectedPortraitImage.enabled = true;
                    selectedPortraitImage.preserveAspect = true;
                }
                else
                {
                    selectedPortraitImage.sprite = null;
                    selectedPortraitImage.enabled = false;
                }
            }
            else
            {
                Debug.LogWarning("⚠️ 슬롯에서 'playerImage'를 찾지 못했습니다.");
            }
        }
        int baseCost = UnitCostCalculator.CalculateGoldCost(found.level);
        int cost = GetDiscountedTrainingCost(baseCost);

        if (expCostText != null)
        {
            expCostText.text = $"레벨업 비용 {cost}골드";
        }

        Debug.Log($"✅ UI 갱신: {found.unitName} / Lv {found.level} / Exp {found.exp} / 비용: {cost}G");
    }

    public void OnUpgradeButtonClicked()
    {
        if (slotData == null || string.IsNullOrEmpty(slotData.unitId))
            return;

        if (upgradeManager == null)
        {
            Debug.LogError("❌ SkillUpgradeManager 연결 안됨");
            return;
        }

        upgradeManager.UpgradeSkill(slotData.unitId, selectedSkillIndex);
    }
}