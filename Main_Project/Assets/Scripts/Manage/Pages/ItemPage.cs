using System.Collections.Generic;
using BattleK.Scripts.Data.Stat;
using BattleK.UI.Book;
using Colosseum.Character;
using TMPro;
using UnityEngine;

namespace TeamManage
{
    public class ItemPage : BookPage
    {
        [Header("왼쪽 페이지")]
        [Tooltip("치료소와 공용인 CharacterDetail")]
        [SerializeField] private CharacterDetail unitPanel;
        [SerializeField] private EquipSlotView equippedItemSlot;

        [Header("오른쪽 페이지")]
        [Tooltip("ItemEntryView들을 자식으로 가진 부모. 여러 개면 순서대로 이어서 채운다.")]
        [SerializeField] private List<Transform> itemEntryParents = new List<Transform>();

        [Header("아이템 데이터")]
        [SerializeField] private ItemDatabase itemDatabase;
        [Tooltip("표시할 카테고리. 비우면 전체")]
        [SerializeField] private List<ItemCategory> displayCategories = new List<ItemCategory>();

        [Header("안내 메시지 (선택 사항)")]
        [SerializeField] private TMP_Text messageText;

        private readonly List<ItemEntryView> entryViews = new List<ItemEntryView>();
        private PagedSlotFiller<ItemEntryData> pager;
        private TeamManageController controller;
        private bool collected;
        private bool isOpen;

        private TeamManageController Controller
        {
            get
            {
                if (controller == null) controller = GetComponentInParent<TeamManageController>(true);
                return controller;
            }
        }

        // Inspector: 이전/다음 페이지 버튼 OnClick
        public void NextPage()
        {
            EnsureCollected();
            pager?.NextPage();
        }

        public void PrevPage()
        {
            EnsureCollected();
            pager?.PrevPage();
        }

        protected override void OnPageOpened()
        {
            isOpen = true;
            Subscribe(false);
            Subscribe(true);
            SetMessage(string.Empty);
            Refresh(false);
        }

        protected override void OnPageClosed()
        {
            isOpen = false;
            Subscribe(false);
        }

        private void OnDestroy()
        {
            Subscribe(false);
        }

        private void Subscribe(bool on)
        {
            if (Controller == null) return;

            Controller.EquipmentChanged -= HandleEquipmentChanged;
            Controller.MessageRaised -= SetMessage;
            if (!on) return;

            Controller.EquipmentChanged += HandleEquipmentChanged;
            Controller.MessageRaised += SetMessage;
        }

        private void HandleEquipmentChanged(Unit unit)
        {
            if (isOpen) Refresh(true);   // 장착 후에도 보던 페이지 유지
        }

        private void SetMessage(string message)
        {
            if (messageText != null) messageText.text = message;
        }

        private void Refresh(bool keepPage)
        {
            EnsureCollected();

            Unit unit = Controller != null ? Controller.SelectedUnit : null;
            if (unit == null || UserManager.Instance == null)
            {
                if (unitPanel != null) unitPanel.Clear();
                if (equippedItemSlot != null) equippedItemSlot.Clear();
                pager?.SetItems(new List<ItemEntryData>(), false);
                return;
            }

            if (unitPanel != null)
            {
                unitPanel.CoroutineHost = this;
                unitPanel.ShowCharacter(unit);
            }

            RefreshEquippedSlot(unit);

            List<ItemEntryData> entries = EquipmentService.BuildItemEntries(
                UserManager.Instance.user, unit, itemDatabase, displayCategories);
            pager?.SetItems(entries, keepPage);
        }

        private void RefreshEquippedSlot(Unit unit)
        {
            if (equippedItemSlot == null) return;

            ItemData equipped = unit.equippedItemId != EquipmentService.NoItem && itemDatabase != null
                ? itemDatabase.GetById(unit.equippedItemId)
                : null;

            if (equipped != null) equippedItemSlot.SetContent(equipped.icon, equipped.description,equipped.id.ToString());
            else equippedItemSlot.Clear();
        }

        private void HandleEntryClicked(ItemEntryView entry)
        {
            Unit unit = Controller != null ? Controller.SelectedUnit : null;
            if (unit == null || UserManager.Instance == null) return;

            Controller.CommitEquipment(unit, EquipmentService.ToggleItem(UserManager.Instance.user, unit, entry.ItemId));
        }

        private void HandleEquippedSlotClicked(EquipSlotView slot)
        {
            Unit unit = Controller != null ? Controller.SelectedUnit : null;
            if (unit == null) return;

            Controller.CommitEquipment(unit, EquipmentService.UnequipItem(unit));
        }

        private void EnsureCollected()
        {
            if (collected) return;

            foreach (Transform parent in itemEntryParents)
            {
                if (parent == null) continue;

                foreach (Transform child in parent)
                {
                    ItemEntryView entry = child.GetComponent<ItemEntryView>();
                    if (entry == null) continue;

                    entry.Clicked += HandleEntryClicked;
                    entryViews.Add(entry);
                }
            }

            if (entryViews.Count == 0)
                Debug.LogError("[ItemPage] ItemEntryView를 찾지 못했습니다. itemEntryParents를 확인하세요.", this);

            pager = new PagedSlotFiller<ItemEntryData>(entryViews);

            if (equippedItemSlot != null) equippedItemSlot.Clicked += HandleEquippedSlotClicked;
            if (itemDatabase == null) Debug.LogError("[ItemPage] itemDatabase가 연결되지 않았습니다.", this);

            collected = true;
        }
    }
}
