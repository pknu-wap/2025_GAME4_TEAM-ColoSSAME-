using UnityEngine;
using UnityEngine.UI;

public class GetExpButton : MonoBehaviour
{
    [Header("버튼 클릭 시 획득 경험치")]
    public float expGain = 10f;

    [Header("playerTrain 표시 텍스트(Text Legacy)")]
    public Text curLevelText; // playerTrain/CurLevel/Text(Legacy)
    public Text curExpText;   // playerTrain/CurEXP/Text(Legacy)

    /// <summary>
    /// GetEXP 버튼 OnClick에 연결할 함수
    /// </summary>
    public void OnClickGetExp()
    {
        Debug.Log("🟦 GetEXP 버튼 클릭됨");
        // 1) 매니저 준비 확인
        if (UserManager.Instance == null || UserManager.Instance.user == null)
        {
            Debug.LogError("❌ UserManager 또는 user가 준비되지 않았습니다.");
            return;
        }

        // 2) 선택된 유닛 확인
        string unitId = UserManager.Instance.selectedUnitId;
        if (string.IsNullOrEmpty(unitId))
        {
            Debug.LogWarning("⚠️ 선택된 유닛이 없습니다. fighter 슬롯을 먼저 클릭하세요.");
            return;
        }

        // 3) 유닛 EXP 증가 요청 (저장까지 UserManager가 처리)
        bool success = UserManager.Instance.AddUnitExp(unitId, expGain);
        if (!success) return;

        // 4) 변경된 유닛 다시 가져와서 UI 갱신
        Unit unit = UserManager.Instance.GetMyUnitById(unitId);
        if (unit == null) return;

        if (curLevelText != null) curLevelText.text = unit.level.ToString();
        if (curExpText != null) curExpText.text = unit.exp.ToString();

        Debug.Log($"✅ GetEXP 완료: {unit.unitName} / Lv {unit.level} / Exp {unit.exp}");
    }
}