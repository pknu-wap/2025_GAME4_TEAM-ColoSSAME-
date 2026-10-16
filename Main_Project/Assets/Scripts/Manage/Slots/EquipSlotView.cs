using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TeamManage
{
    public class EquipSlotView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text itemDesc;
        [SerializeField] private Button button;

        public string Key { get; private set; }
        public bool IsEmpty => string.IsNullOrEmpty(Key);
        public event Action<EquipSlotView> Clicked;

        private void Awake()
        {
            if (button == null) button = GetComponent<Button>();
            if (button != null) button.onClick.AddListener(HandleClicked);

            if (IsEmpty) Clear();
        }

        private void OnDestroy()
        {
            if (button != null) button.onClick.RemoveListener(HandleClicked);
        }


        public void SetContent(Sprite icon, string desc, string key)
        {
            Key = key;
            if (iconImage != null)
            {
                iconImage.sprite = icon;
                iconImage.preserveAspect = true;
                iconImage.enabled = icon != null;
            }
            if (itemDesc != null) itemDesc.text = desc;
        }

        public void SetContent(Sprite icon, string key) => SetContent(icon, null, key);

        public void Clear()
        {
            Key = null;
            if (iconImage != null)
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
            }
            if (itemDesc != null) itemDesc.text = Loc.Get("equip_empty");
        }

        private void HandleClicked()
        {
            if (!IsEmpty) Clicked?.Invoke(this);
        }
    }
}
