using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BattleK.Scripts.Data;
using BattleK.Scripts.Data.ClassInfo;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace BattleK.Scripts.UI
{
    public class TargetWindowController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private Image _characterImg;
        
        [Header("Target Slots")]
        [SerializeField] private Button _slot1Button;
        [SerializeField] private TextMeshProUGUI _slot1Text;
        [SerializeField] private Button _slot2Button;
        [SerializeField] private TextMeshProUGUI _slot2Text;
        
        [Header("Job Buttons")]
        [SerializeField] private Button[] _jobButtons;

        private string _currentKey;
        private int _selectedSlotIndex = -1;
        private AsyncOperationHandle<Sprite> _currentHandle;

        private void Start()
        {
            _slot1Button.onClick.AddListener(() => SelectSlot(0));
            _slot2Button.onClick.AddListener(() => SelectSlot(1));

            foreach (var button in _jobButtons)
            {
                var label = button.GetComponentInChildren<TextMeshProUGUI>()?.text;
                if(!string.IsNullOrEmpty(label)) button.onClick.AddListener(() => OnJobButtonClicked(label));
            }
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            ReleaseHandle();
        }

        public void SetCharacter(string characterKey)
        {
            _currentKey = characterKey;
            if(_nameText) _nameText.text = FormatName(_currentKey);

            if (!string.IsNullOrEmpty(_currentKey))
            {
                StartCoroutine(LoadImage(_currentKey));
            }
            else
            {
                ClearImage();
            }

            LoadSavedTargets();
            SelectSlot(-1);
        }

        private IEnumerator LoadImage(string key)
        {
            ReleaseHandle();
            var handle = Addressables.LoadAssetAsync<Sprite>(key);
            _currentHandle = handle;
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                if (!_characterImg) yield break;
                _characterImg.sprite = handle.Result;
                _characterImg.color = Color.white;
            }
            else
            {
                Debug.LogWarning($"[TargetWindow] Image Load Failed: {key}");
                ClearImage();
            }
        }

        private void ClearImage()
        {
            if (_characterImg)
            {
                _characterImg.sprite = null;
                _characterImg.color = Color.clear;
            }
            ReleaseHandle();
        }
        
        private void ReleaseHandle()
        {
            if (_currentHandle.IsValid()) Addressables.Release(_currentHandle);
        }
        
        private void LoadSavedTargets()
        {
            var record = PlayerCharacterSaveManager.Instance.GetRecord(_currentKey);
            if (record == null) return;
            UpdateSlotText(_slot1Text, GetTargetName(record, 0));
            UpdateSlotText(_slot2Text, GetTargetName(record, 1));
        }

        private string GetTargetName(CharacterRecord record, int index)
        {
            if (record.targetClasses == null || record.targetClasses.Count <= index) return $"타겟{index + 1}";
            var enumVal = record.targetClasses[index];
            return TargetClassMap.EnumToKo.TryGetValue(enumVal, out var koName) ? koName : $"타겟{index + 1}";
        }

        private void SelectSlot(int index)
        {
            _selectedSlotIndex = index;
            HighlightButton(_slot1Button, index == 0);
            HighlightButton(_slot2Button, index == 1);
        }

        private void OnJobButtonClicked(string jobNameKOR)
        {
            if(_selectedSlotIndex == - 1 || string.IsNullOrEmpty(_currentKey)) return;
            UpdateSlotText(_selectedSlotIndex == 0 ? _slot1Text : _slot2Text, jobNameKOR);

            if (TargetClassMap.TryKoToEnum(jobNameKOR, out var jobEnum))
            {
                PlayerCharacterSaveManager.Instance.ModifyRecord(_currentKey, record =>
                {
                    record.targetClasses ??= new List<UnitClass>();
                    while (record.targetClasses.Count <= _selectedSlotIndex) record.targetClasses.Add(default);
                    record.targetClasses[_selectedSlotIndex] = jobEnum;
                });
            }
        }
        
        private void HighlightButton(Button btn, bool isSelected) => 
            btn.image.color = isSelected ? new Color(1f, 1f, 0.7f) : Color.white;
    
        private void UpdateSlotText(TextMeshProUGUI txt, string content) => 
            txt.text = content;
    
        private string FormatName(string key) => 
            string.IsNullOrWhiteSpace(key) ? "" : Regex.Replace(key.Replace('_', ' '), @"\s+", " ").Trim();
    }
}
