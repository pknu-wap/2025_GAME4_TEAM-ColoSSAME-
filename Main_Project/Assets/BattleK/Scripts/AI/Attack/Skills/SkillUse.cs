using System.Collections;
using UnityEngine;

/// <summary>
/// 스킬 실행/정지의 단일 관문.
/// - AICore.StopAllActionsHard()에서 CancelAll()을 호출해 모든 스킬 동작을 원자적으로 중단 가능
/// - 기존 방식(Logic 프리팹 Instantiate 후 Execute 호출)과 100% 호환
/// - 스킬 로직이 코루틴/이펙트 등을 내부에서 돌려도 안전하게 중단되도록 Cancel 경로 제공
/// </summary>
public class SkillUse : MonoBehaviour
{
    /// <summary>현재 활성화된 스킬 로직 인스턴스(있다면).</summary>
    private SkillLogic _activeLogic;

    /// <summary>스킬 준비/딜레이/타임아웃 등 외부 제어용(필요 시 확장) 코루틴.</summary>
    private Coroutine _useRoutine;

    /// <summary>스킬 사용: 기존 API 유지. 즉시 로직을 인스턴스화하고 Execute 호출.</summary>
    public void UseSkill(SkillData skillData, AICore user, Transform target)
    {
        if (skillData == null || skillData.skillLogicPrefab == null)
        {
            Debug.LogError("SkillData 또는 SkillLogicPrefab이 null입니다.");
            return;
        }
        if (user == null || target == null || !target.gameObject.activeInHierarchy)
            return;

        // 중복 사용 방지
        CancelAll();

        // 로직 인스턴스 생성 (원래 구현과 동일)
        _activeLogic = Instantiate(skillData.skillLogicPrefab, user.transform);

        // 안전성: 코루틴으로 한 프레임 양보(초기화/바인딩 타이밍 보정) 후 Execute
        _useRoutine = StartCoroutine(Co_RunSkill(_activeLogic, user, target));
    }

    private IEnumerator Co_RunSkill(SkillLogic logic, AICore user, Transform target)
    {
        yield return null; // 바인딩/애니 트리거 등 한 프레임 유예

        if (logic == null || user == null) yield break;
        if (target == null || !target.gameObject.activeInHierarchy) { CancelAll(); yield break; }

        // 기존과 동일하게 로직 실행 (원본 준수)
        // 참고: 원본은 즉시 Execute(user, target)를 호출했음
        logic.Execute(user, target);

        // 여기서는 로직 수명 관리를 로직 쪽에 위임하되, 외부 Cancel에 대비해 참조만 유지
        _useRoutine = null;
    }

    /// <summary>
    /// ▶ 모든 스킬 동작을 즉시 중단. (AICore.StopAllActionsHard에서 호출)
    /// - 현재 코루틴 중단
    /// - 활성 로직이 남아있다면 즉시 정리(가능하면 Cancel, 아니면 Destroy)
    /// </summary>
    public void CancelAll()
    {
        // 코루틴 중지
        if (_useRoutine != null)
        {
            try { StopCoroutine(_useRoutine); } catch { }
            _useRoutine = null;
        }

        // 활성 로직 정리
        if (_activeLogic != null)
        {
            try
            {
                // 스킬 로직이 Cancel 같은 정리 API를 제공하면 우선 호출
                var cancelMethod = _activeLogic.GetType().GetMethod("Cancel", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (cancelMethod != null)
                {
                    cancelMethod.Invoke(_activeLogic, null);
                }
            }
            catch { /* 정리 과정 예외 방어 */ }

            // 파괴(풀 사용 시 로직 쪽에서 Despawn 하도록 바꿔도 됨)
            Destroy(_activeLogic.gameObject);
            _activeLogic = null;
        }

        // 예약된 Invoke가 있다면 정리
        try { CancelInvoke(); } catch { }
    }

    private void OnDisable()
    {
        // 비활성화/사망/씬 전환 시에도 안전하게 중단
        CancelAll();
    }
}
