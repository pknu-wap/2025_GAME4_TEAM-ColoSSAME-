using System.Collections;
using UnityEngine;

public class FighterSkillShow : MonoBehaviour
{
    public Transform fighterListParent;       // 슬롯 부모
    public SkillUpgradeManager upgradeManager;

    private FighterSlotData selectedSlot;     // 선택된 슬롯
    public int selectedSkillIndex = 0;        // 강화할 스킬 인덱스

    private IEnumerator Start()
    {
        yield return null;

        if (fighterListParent == null || upgradeManager == null)
        {
            Debug.LogError("fighterListParent 또는 upgradeManager 연결 필요");
            yield break;
        }

        // 첫 번째 슬롯을 기본 선택
        if (fighterListParent.childCount > 0)
        {
            Transform slot = fighterListParent.GetChild(0);
            FighterSlotData data = slot.GetComponent<FighterSlotData>();
            if (data == null) data = slot.gameObject.AddComponent<FighterSlotData>();

            selectedSlot = data;
        }
    }

    // 버튼 클릭 시 스킬 강화
    public void OnUpgradeButtonClicked(int selectedSkillIndex)
    {
        if (selectedSlot == null || string.IsNullOrEmpty(selectedSlot.unitId))
            return;

        upgradeManager.UpgradeSkill(selectedSlot.unitId, selectedSkillIndex);
    }

    // 스킬 선택 시 호출 (예: UI 버튼 클릭)
    public void OnSelectSkill(int skillIndex)
    {
        selectedSkillIndex = skillIndex;
    }

    public void OnSelectSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= fighterListParent.childCount)
            return;

        Transform slot = fighterListParent.GetChild(slotIndex);
        FighterSlotData data = slot.GetComponent<FighterSlotData>();
        if (data != null)
            selectedSlot = data;

        Debug.Log($"🎯 슬롯 {slotIndex} 선택, unitId={selectedSlot.unitId}");
    }
}