using System;
using BattleK.Scripts.Data.Stat;
using Colosseum.Character;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TeamManage
{
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

        private string SelectedUnitId { get; set; }

        public Unit SelectedUnit =>
            UserManager.Instance != null ? UserManager.Instance.GetMyUnitById(SelectedUnitId) : null;

        public event Action<Unit> EquipmentChanged;

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
            UserManager.Instance.SetSelectedUnit(unit.Id);

            RefreshMainDetail();
            SetDetailButtonsInteractable(true);
            ShowMessage(string.Empty);
        }

        public void ClearSelection()
        {
            SelectedUnitId = null;
            RefreshMainDetail();
            SetDetailButtonsInteractable(false);
        }

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
