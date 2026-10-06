using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TeamManage
{
    public class ItemEntryView : MonoBehaviour, ISlotView<ItemEntryData>
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text countText;
        [SerializeField] private GameObject equippedMark;
        [SerializeField] private GameObject lockedMark;
        [SerializeField] private Button button;

        public int ItemId { get; private set; } = EquipmentService.NoItem;
        public event Action<ItemEntryView> Clicked;

        private void Awake()
        {
            button.onClick.AddListener(HandleClicked);
        }

        private void OnDestroy()
        {
            if (button != null) button.onClick.RemoveListener(HandleClicked);
        }

        public void SetItem(ItemEntryData data)
        {
            ItemId = data.Item.id;
            gameObject.SetActive(true);

            iconImage.sprite = data.Item.icon;
            iconImage.enabled = data.Item.icon != null;

            nameText.text = data.Item.itemName;
            countText.text = $"x {data.AvailableCount}/{data.OwnedCount}";

            equippedMark.SetActive(data.EquippedByCurrentUnit);
            lockedMark.SetActive(!data.Selectable);
            button.interactable = data.Selectable;
        }

        public void Clear()
        {
            ItemId = EquipmentService.NoItem;
            gameObject.SetActive(false);
        }

        private void HandleClicked()
        {
            if (ItemId != EquipmentService.NoItem) Clicked?.Invoke(this);
        }
    }
}
