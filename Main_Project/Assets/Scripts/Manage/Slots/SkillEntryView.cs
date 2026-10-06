using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TeamManage
{
    public class SkillEntryView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text tierText;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text effectText;
        [Tooltip("이미 장착 중일 때 켜지는 표시")]
        [SerializeField] private GameObject equippedMark;
        [SerializeField] private Button button;

        public string SkillName { get; private set; }
        public event Action<SkillEntryView> Clicked;

        private void Awake()
        {
            button.onClick.AddListener(HandleClicked);
        }

        private void OnDestroy()
        {
            if (button != null) button.onClick.RemoveListener(HandleClicked);
        }

        public void Bind(SkillEntryData data)
        {
            SkillName = data.SkillName;
            gameObject.SetActive(true);

            Sprite icon = data.Skill != null ? data.Skill.Icon : null;
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;

            tierText.text = SkillCatalog.GetTierLabel(data.Tier);
            nameText.text = $"이름 : {data.DisplayName}";
            string description = data.Skill != null ? data.Skill.Description : string.Empty;
            effectText.text = $"효과 : {description}";

            equippedMark.SetActive(data.Equipped);
        }

        public void Clear()
        {
            SkillName = null;
            gameObject.SetActive(false);
        }

        private void HandleClicked()
        {
            if (!string.IsNullOrEmpty(SkillName)) Clicked?.Invoke(this);
        }
    }
}
