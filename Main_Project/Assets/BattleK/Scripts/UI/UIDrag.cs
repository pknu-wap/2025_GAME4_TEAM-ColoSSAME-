using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BattleK.Scripts.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(RectTransform))]
    public class UIDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        private RectTransform _rectTransform;
        private Canvas _canvas;
        private CanvasGroup _canvasGroup;
        private CharacterID _characterID;
        private Vector2 _dragOffset;

        [Header("설정")] [SerializeField] private float _slotSnapDistance = 80f;
        [SerializeField] private Vector2 _correctionOffset = Vector2.zero;

        [Header("더블클릭 시간 간격 (초)")] [SerializeField]
        private float _doubleClickThreshold = 0.25f;

        [Header("타겟 선택창 오브젝트 (씬 상의 원본)")]
        [SerializeField] private  TargetWindowController _targetWindowController;

        private Transform _homeParent;
        private Vector2 _homePosition;
        private int _homeSiblingIndex;

        private Vector2 _dragStartPosition;
        private Slot _currentSlot;
        private float _lastClickTime = -1f;
        
        private static string _currentOpenCharacterKey;

        private void Awake()
        {
            InitializeComponents();
            InitializeHomeState();
        }

    public void OnBeginDrag(PointerEventData eventData)
        {
            _dragStartPosition = _rectTransform.anchoredPosition;
            _canvasGroup.blocksRaycasts = false;
            DetachFromSlot();
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _rectTransform.parent as RectTransform,
                    eventData.position,
                    eventData.pressEventCamera,
                    out var localMousePos))
            {
                _dragOffset = _rectTransform.anchoredPosition - localMousePos;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _rectTransform.parent as RectTransform,
                    eventData.position,
                    eventData.pressEventCamera,
                    out var localMousePos))
            {
                _rectTransform.anchoredPosition = localMousePos + _dragOffset;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _canvasGroup.blocksRaycasts = true;
            if (TrySnapToNearestSlot(out var targetSlot))
            {
                AttachToSlot(targetSlot);
            }
            else
            {
                _rectTransform.anchoredPosition = _dragStartPosition;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    HandleLeftClick();
                    break;
                case PointerEventData.InputButton.Right:
                    HandleRightClick();
                    break;
                case PointerEventData.InputButton.Middle:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandleLeftClick()
        {
            if (Time.unscaledTime - _lastClickTime < _doubleClickThreshold)
            {
                if (_currentSlot) ToggleTargetWindow();
            }
            _lastClickTime = Time.unscaledTime;
        }

        private void HandleRightClick()
        {
            if (!_currentSlot) return;
            DetachFromSlot();
            ReturnToHome(closeWindow: true);
        }

        private bool TrySnapToNearestSlot(out Slot targetSlot)
        {
            targetSlot = null;
            if (!SlotManager.Instance) return false;
            var slots = SlotManager.Instance.AllSlots;
            var closestDistance = float.MaxValue;
            foreach (var slot in slots)
            {
                if (slot.IsOccupied) continue;
                var dist = Vector2.Distance(transform.position, slot.transform.position);
                if (!(dist < _slotSnapDistance) || !(dist < closestDistance)) continue;
                closestDistance = dist;
                targetSlot = slot;
            }
            return targetSlot;
        }

        private void AttachToSlot(Slot slot)
        {
            _currentSlot = slot;
            _currentSlot.IsOccupied = true;
            _currentSlot.Occupant = this;
            _rectTransform.position = slot.transform.position; 
            _rectTransform.anchoredPosition += _correctionOffset;
        }
        
        private void DetachFromSlot()
        {
            if (!_currentSlot) return;
            _currentSlot.IsOccupied = false;
            _currentSlot.Occupant = null;
            _currentSlot = null;
        }

        public void ReturnToHome(bool closeWindow)
        {
            if (_homeParent && _rectTransform.parent != _homeParent)
            {
                _rectTransform.SetParent(_homeParent, false);
            }

            _rectTransform.anchoredPosition = _homePosition;
            _rectTransform.SetSiblingIndex(_homeSiblingIndex);

            if (closeWindow)
            {
                CloseWindowIfMine();
            }
        }

        private void ToggleTargetWindow()
        {
            if(!_targetWindowController) return;
            var key = GetCharacterKey();
            if(string.IsNullOrEmpty(key)) return;
            var windowObj = _targetWindowController.gameObject;

            if (!windowObj.activeSelf)
            {
                OpenWindow(key);
                return;
            }
            
            if (_currentOpenCharacterKey == key)
            {
                windowObj.SetActive(false);
                _currentOpenCharacterKey = null;
            }
            else
            {
                OpenWindow(key);
            }
        }

        private void OpenWindow(string key)
        {
            _targetWindowController.gameObject.SetActive(true);
            _targetWindowController.SetCharacter(key);
            _currentOpenCharacterKey = key;
        }

        private void CloseWindowIfMine()
        {
            if(!_targetWindowController || !_targetWindowController.gameObject.activeSelf) return;
            var key = GetCharacterKey();
            if (string.IsNullOrEmpty(key) || _currentOpenCharacterKey != key) return;
            _targetWindowController.gameObject.SetActive(false);
            _currentOpenCharacterKey = null;
        }

        private void InitializeComponents()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _characterID = GetComponent<CharacterID>();
            _canvas = GetComponentInParent<Canvas>();
            
            if (_canvas && _canvas.rootCanvas) _canvas = _canvas.rootCanvas;
            if(!_characterID) Debug.LogWarning("Character ID is missing");
        }

        private void InitializeHomeState()
        {
            _homeParent = _rectTransform.parent;
            _homePosition = _rectTransform.anchoredPosition;
            _homeSiblingIndex = _rectTransform.GetSiblingIndex();
        }
        
        private string GetCharacterKey()
        {
            return _characterID ? _characterID.characterKey : null;
        }
    }
}
