using System.Collections;
using BattleK.Scripts.Data.Stat;
using Colosseum.Character;
using UnityEngine;

namespace TeamManage
{
    /// <summary>
    /// 왼쪽 페이지: 치료소의 CharacterList(+CharacterItem)를 그대로 재사용한다.
    ///  - 초상화 Addressable 로드 / 슬롯 배치 / 선택 하이라이트는 CharacterList가 담당
    ///  - 이 클래스는 코루틴 호스트 지정, 갱신 시점, 선택 결과를 컨트롤러로 전달하는 일만 한다.
    /// </summary>
    public class UnitListPage : MonoBehaviour
    {
        [Tooltip("비워두면 같은 오브젝트의 CharacterList 사용")]
        [SerializeField] private CharacterList characterList;

        private TeamManageController controller;

        private TeamManageController Controller
        {
            get
            {
                if (controller == null) controller = GetComponentInParent<TeamManageController>(true);
                return controller;
            }
        }

        private void Awake()
        {
            if (characterList == null) characterList = GetComponent<CharacterList>();
            if (characterList == null)
            {
                Debug.LogError("[UnitListPage] CharacterList가 연결되지 않았습니다.", this);
                return;
            }

            characterList.CoroutineHost = this;
            characterList.OnCharacterSelected += HandleCharacterSelected;
        }

        private void OnDestroy()
        {
            if (characterList != null) characterList.OnCharacterSelected -= HandleCharacterSelected;
        }

        private void OnEnable()
        {
            // HealingCenterUI와 동일: 유닛 데이터/오브젝트 초기화 이후로 한 프레임 미룬다.
            StartCoroutine(RefreshNextFrame());
        }

        private IEnumerator RefreshNextFrame()
        {
            yield return null;
            if (characterList == null) yield break;

            characterList.Refresh();

            // 스킬/아이템 화면에서 돌아왔을 때는 기존 선택을 유지하고,
            // 선택이 없으면 HealingCenterUI와 같이 첫 번째 유닛을 자동 선택한다.
            Unit target = Controller != null ? Controller.SelectedUnit : null;
            if (target == null) target = characterList.GetFirstUnit();

            if (target != null) characterList.Select(target);   // → OnCharacterSelected → Controller.SelectUnit
            else if (Controller != null) Controller.ClearSelection();
        }

        private void HandleCharacterSelected(Unit unit)
        {
            if (unit != null && Controller != null) Controller.SelectUnit(unit.Id);
        }
    }
}
