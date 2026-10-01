using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TeamManage
{
    /// <summary>스킬 페이지 오른쪽 목록의 한 줄 (아이콘 / 티어 / 이름 / 효과 + 장착 표시).</summary>
    public class SkillEntryView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text tierText;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text effectText;
        [Tooltip("이미 장착 중일 때 켜지는 표시 (선택 사항)")]
        [SerializeField] private GameObject equippedMark;
        [Tooltip("비워두면 이 오브젝트(없으면 자식)의 Button 사용")]
        [SerializeField] private Button button;

        public string SkillName { get; private set; }
        public event Action<SkillEntryView> Clicked;

        private void Awake()
        {
            if (button == null) button = GetComponent<Button>();
            if (button == null) button = GetComponentInChildren<Button>(true);

            if (button != null) button.onClick.AddListener(HandleClicked);
            else Debug.LogWarning($"[SkillEntryView] {name}에 Button이 없습니다.", this);
        }

        private void OnDestroy()
        {
            if (button != null) button.onClick.RemoveListener(HandleClicked);
        }

        public void Bind(SkillEntryData data)
        {
            SkillName = data.SkillName;
            gameObject.SetActive(true);

            if (iconImage != null)
            {
                Sprite icon = data.Skill != null ? data.Skill.Icon : null;
                iconImage.sprite = icon;
                iconImage.enabled = icon != null;
            }

            if (tierText != null) tierText.text = SkillCatalog.GetTierLabel(data.Tier);
            if (nameText != null) nameText.text = $"이름 : {data.DisplayName}";
            if (effectText != null)
            {
                string description = data.Skill != null ? data.Skill.Description : string.Empty;
                effectText.text = $"효과 : {description}";
            }

            if (equippedMark != null) equippedMark.SetActive(data.Equipped);
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
