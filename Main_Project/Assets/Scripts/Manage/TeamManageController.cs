using System;
using BattleK.Scripts.Data.Stat;
using Colosseum.Character;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TeamManage
{
    /// <summary>
    /// 팀 관리 화면의 상태 조율자 (선택 유닛 / 장착 결과 저장 / 안내 메시지).
    /// Manage 루트(항상 활성)에 붙인다 — 하위 페이지(SkillPage, ItemPage)가 GetComponentInParent로 찾는다.
    /// 페이지 전환과 content 토글은 Book 시스템(BookPage, BookPageButton, BookBackButton)이 담당한다.
    /// </summary>
    public class TeamManageController : MonoBehaviour
    {
        [Header("메인 오른쪽 페이지")]
        [Tooltip("치료소와 공용인 CharacterDetail (레벨/스탯 텍스트 연결)")]
        [SerializeField] private CharacterDetail detailPanel;
        [Tooltip("유닛을 선택하기 전에는 비활성으로 두는 버튼 (이동은 BookPageButton이 처리)")]
        [SerializeField] private Button skillButton;
        [SerializeField] private Button itemButton;

        [Header("안내 메시지 (선택 사항)")]
        [SerializeField] private TMP_Text messageText;

        public string SelectedUnitId { get; private set; }

        public Unit SelectedUnit =>
            UserManager.Instance != null ? UserManager.Instance.GetMyUnitById(SelectedUnitId) : null;

        /// <summary>유닛을 선택했을 때 (왼쪽 목록 클릭).</summary>
        public event Action<Unit> UnitSelected;

        /// <summary>장착 상태가 바뀌고 저장까지 끝났을 때.</summary>
        public event Action<Unit> EquipmentChanged;

        /// <summary>안내 문구 (스킬/아이템 페이지가 각자 표시).</summary>
        public event Action<string> MessageRaised;

        private void OnEnable()
        {
            RefreshMainDetail();
            SetDetailButtonsInteractable(SelectedUnit != null);
        }

        public void SelectUnit(string unitId)
        {
            Unit unit = UserManager.Instance != null ? UserManager.Instance.GetMyUnitById(unitId) : null;
            if (unit == null) return;

            SelectedUnitId = unit.Id;
            UserManager.Instance.SetSelectedUnit(unit.Id);   // 기존 훈련/스킬 화면과 선택 상태 공유

            RefreshMainDetail();
            SetDetailButtonsInteractable(true);
            ShowMessage(string.Empty);
            UnitSelected?.Invoke(unit);
        }

        /// <summary>보유 유닛이 없을 때 선택/상세/버튼 상태를 비운다.</summary>
        public void ClearSelection()
        {
            SelectedUnitId = null;
            RefreshMainDetail();
            SetDetailButtonsInteractable(false);
        }

        /// <summary>장착/해제가 성공했으면 즉시 저장하고 화면 갱신 이벤트를 보낸다.</summary>
        public void CommitEquipment(Unit unit, EquipResult result)
        {
            switch (result)
            {
                case EquipResult.Equipped:
                case EquipResult.Unequipped:
                    UserManager.Instance.SaveUser();
                    ShowMessage(string.Empty);
                    EquipmentChanged?.Invoke(unit);
                    break;
                case EquipResult.NoFreeSlot:
                    ShowMessage("스킬 슬롯이 가득 찼습니다.");
                    break;
                case EquipResult.ItemUnavailable:
                    ShowMessage("다른 유닛이 사용 중인 아이템입니다.");
                    break;
                case EquipResult.NotOwned:
                    ShowMessage("보유하지 않은 항목입니다.");
                    break;
            }
        }

        private void RefreshMainDetail()
        {
            if (detailPanel == null) return;

            detailPanel.CoroutineHost = this;
            Unit unit = SelectedUnit;
            if (unit != null) detailPanel.ShowCharacter(unit);
            else detailPanel.Clear();
        }

        private void SetDetailButtonsInteractable(bool interactable)
        {
            if (skillButton != null) skillButton.interactable = interactable;
            if (itemButton != null) itemButton.interactable = interactable;
        }

        private void ShowMessage(string message)
        {
            if (messageText != null) messageText.text = message;
            else if (!string.IsNullOrEmpty(message)) Debug.Log($"[TeamManage] {message}");

            MessageRaised?.Invoke(message);
        }
    }
}
