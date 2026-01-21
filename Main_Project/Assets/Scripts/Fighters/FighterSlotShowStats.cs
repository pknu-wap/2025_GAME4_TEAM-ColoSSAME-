using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FighterSlotShowStats : MonoBehaviour, IPointerClickHandler
{
    [Header("슬롯 데이터(유닛 ID)")]
    public FighterSlotData slotData;

    [Header("표시 대상(Text Legacy)")]
    public Text curLevelText;
    public Text curExpText;
    public Slider expSlider;

    public void OnPointerClick(PointerEventData eventData)
    {
        // playerTrain 활성화는 다른 로직이 이미 처리한다고 가정
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
            Debug.LogWarning("⚠️ 슬롯에 unitId가 없습니다. (FighterSlotData 누락 또는 값 비어있음)");
            return;
        }

        // unitId로 유닛 찾기
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
        // 선택된 유닛 저장: GetEXP 버튼이 이 값을 사용함
        UserManager.Instance.SetSelectedUnit(slotData.unitId);
        
        // Level/Exp 표시
        if (curLevelText != null) curLevelText.text = found.level.ToString();
        if (curExpText != null) curExpText.text = found.exp.ToString();
        if (expSlider != null) expSlider.value = found.exp;

        Debug.Log($"✅ UI 갱신: {found.unitName} / Lv {found.level} / Exp {found.exp}");
    }
}