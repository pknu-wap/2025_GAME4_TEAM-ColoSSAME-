using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TeamManage
{
    /// <summary>
    /// 아이템 페이지 목록의 한 칸. PagedSlotFiller가 채운다.
    /// 장착된 아이템도 목록에서 지우지 않고 표시(mark)만 바꾼다.
    ///  - equippedMark : 현재 유닛이 장착 중 (다시 누르면 해제)
    ///  - lockedMark   : 남은 수량이 없어 장착 불가 (다른 유닛이 사용 중)
    /// </summary>
    public class ItemEntryView : MonoBehaviour, ISlotView<ItemEntryData>
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text countText;
        [SerializeField] private GameObject equippedMark;
        [SerializeField] private GameObject lockedMark;
        [Tooltip("비워두면 이 오브젝트(없으면 자식)의 Button 사용")]
        [SerializeField] private Button button;

        public int ItemId { get; private set; } = EquipmentService.NoItem;
        public event Action<ItemEntryView> Clicked;

        private void Awake()
        {
            if (button == null) button = GetComponent<Button>();
            if (button == null) button = GetComponentInChildren<Button>(true);

            if (button != null) button.onClick.AddListener(HandleClicked);
            else Debug.LogWarning($"[ItemEntryView] {name}에 Button이 없습니다.", this);
        }

        private void OnDestroy()
        {
            if (button != null) button.onClick.RemoveListener(HandleClicked);
        }

        public void SetItem(ItemEntryData data)
        {
            ItemId = data.Item.id;
            gameObject.SetActive(true);

            if (iconImage != null)
            {
                iconImage.sprite = data.Item.icon;
                iconImage.enabled = data.Item.icon != null;
            }

            if (nameText != null) nameText.text = data.Item.itemName;
            if (countText != null) countText.text = $"x {data.AvailableCount}/{data.OwnedCount}";

            if (equippedMark != null) equippedMark.SetActive(data.EquippedByCurrentUnit);
            if (lockedMark != null) lockedMark.SetActive(!data.Selectable);
            if (button != null) button.interactable = data.Selectable;
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
