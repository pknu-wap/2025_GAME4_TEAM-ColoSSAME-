using System.Collections.Generic;
using BattleK.Scripts.AI.Skill.Base;
using BattleK.Scripts.Data.Stat;
using BattleK.UI.Book;
using Colosseum.Character;
using TMPro;
using UnityEngine;

namespace TeamManage
{
    public class SkillPage : BookPage
    {
        [Header("왼쪽 페이지")]
        [Tooltip("CharacterDetail")]
        [SerializeField] private CharacterDetail unitPanel;
        [Tooltip("EquipSlotView 3개를 자식으로 가진 부모")]
        [SerializeField] private Transform equippedSlotsParent;

        [Header("오른쪽 페이지")]
        [Tooltip("SkillEntryView들을 자식으로 가진 부모 (최대 5개: 티어3×2, 티어2×2, 궁극기×1)")]
        [SerializeField] private Transform skillEntriesParent;

        [Header("스킬 데이터")]
        [SerializeField] private RandomSkillGrantA skillGrant;

        [Header("안내 메시지 (선택 사항)")]
        [SerializeField] private TMP_Text messageText;

        private readonly List<EquipSlotView> equippedSlots = new List<EquipSlotView>();
        private readonly List<SkillEntryView> entryViews = new List<SkillEntryView>();
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

        protected override void OnPageOpened()
        {
            isOpen = true;
            Subscribe(false);
            Subscribe(true);
            SetMessage(string.Empty);
            Refresh();
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
            if (isOpen) Refresh();
        }

        private void SetMessage(string message)
        {
            if (messageText != null) messageText.text = message;
        }

        private void Refresh()
        {
            EnsureCollected();

            Unit unit = Controller != null ? Controller.SelectedUnit : null;
            if (unit == null)
            {
                if (unitPanel != null) unitPanel.Clear();
                foreach (EquipSlotView slot in equippedSlots) slot.Clear();
                foreach (SkillEntryView entry in entryViews) entry.Clear();
                return;
            }

            if (unitPanel != null)
            {
                unitPanel.CoroutineHost = this;
                unitPanel.ShowCharacter(unit);
            }

            List<string> equippedNames = EquipmentService.GetEquippedSkillNames(unit);
            for (int i = 0; i < equippedSlots.Count; i++)
            {
                if (i < equippedNames.Count)
                {
                    SkillSO skill = SkillCatalog.FindSkill(skillGrant, unit, equippedNames[i], out _);
                    equippedSlots[i].SetContent(skill != null ? skill.Icon : null, equippedNames[i]);
                }
                else
                {
                    equippedSlots[i].Clear();
                }
            }

            List<SkillEntryData> entries = SkillCatalog.BuildOwnedEntries(skillGrant, unit);
            if (entries.Count > entryViews.Count)
                Debug.LogWarning($"[SkillPage] 표시 칸({entryViews.Count})보다 보유 스킬({entries.Count})이 많습니다.", this);

            for (int i = 0; i < entryViews.Count; i++)
            {
                if (i < entries.Count) entryViews[i].Bind(entries[i]);
                else entryViews[i].Clear();
            }
        }

        private void HandleEntryClicked(SkillEntryView entry)
        {
            Unit unit = Controller != null ? Controller.SelectedUnit : null;
            if (unit == null) return;

            Controller.CommitEquipment(unit, EquipmentService.ToggleSkill(unit, entry.SkillName));
        }

        private void HandleEquippedSlotClicked(EquipSlotView slot)
        {
            Unit unit = Controller != null ? Controller.SelectedUnit : null;
            if (unit == null) return;

            Controller.CommitEquipment(unit, EquipmentService.UnequipSkill(unit, slot.Key));
        }

        private void EnsureCollected()
        {
            if (collected) return;

            if (equippedSlotsParent == null || skillEntriesParent == null)
            {
                Debug.LogError("[SkillPage] equippedSlotsParent / skillEntriesParent가 연결되지 않았습니다.", this);
                return;
            }

            foreach (Transform child in equippedSlotsParent)
            {
                EquipSlotView slot = child.GetComponent<EquipSlotView>();
                if (slot == null) continue;

                slot.Clicked += HandleEquippedSlotClicked;
                equippedSlots.Add(slot);
            }

            foreach (Transform child in skillEntriesParent)
            {
                SkillEntryView entry = child.GetComponent<SkillEntryView>();
                if (entry == null) continue;

                entry.Clicked += HandleEntryClicked;
                entryViews.Add(entry);
            }

            if (equippedSlots.Count != EquipmentService.MaxSkillSlots)
                Debug.LogWarning($"[SkillPage] 장착 칸은 {EquipmentService.MaxSkillSlots}개여야 합니다 (현재 {equippedSlots.Count}).", this);

            collected = true;
        }
    }
}
