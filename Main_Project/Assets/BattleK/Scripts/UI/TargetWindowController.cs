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
        }

        private void SelectSlot(int index)
        {
            _selectedSlotIndex = index;
            HighlightButton(_slot1Button, index == 0);
            HighlightButton(_slot2Button, index == 1);
        }
        
        private void HighlightButton(Button btn, bool isSelected) => 
            btn.image.color = isSelected ? new Color(1f, 1f, 0.7f) : Color.white;
    
        private void UpdateSlotText(TextMeshProUGUI txt, string content) => 
            txt.text = content;
    
        private string FormatName(string key) => 
            string.IsNullOrWhiteSpace(key) ? "" : Regex.Replace(key.Replace('_', ' '), @"\s+", " ").Trim();
    }
}
