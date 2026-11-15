using UnityEngine;
using UnityEngine.EventSystems;

namespace BattleK.Scripts.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(RectTransform))]
    public class UIDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [Header("설정")]
        [SerializeField] private float _slotSnapDistance = 80f;
        [SerializeField] private Vector2 _correctionOffset = Vector2.zero;
        [Header("더블클릭 시간 간격 (초)")]
        [SerializeField] private float _doubleClickThreshold = 0.25f;
        [Header("타겟 선택창 오브젝트 (씬 상의 원본)")]
        [SerializeField] private TargetWindowController _targetWindowController;

        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private CharacterID _characterID;
        private Transform _homeParent;
        private Vector2 _homePosition;
        private int _homeSiblingIndex;
        private Slot _currentSlot;
        private Vector2 _dragOffset;
        private Vector2 _dragStartPosition;
        private bool _isDragging;
        private bool _blocksRaycastsBeforeDrag;
        private float _lastClickTime = float.NegativeInfinity;
        private static string _currentOpenCharacterKey;
        public Slot CurrentSlot => _currentSlot;
        public string UnitKey => _characterID ? _characterID.characterKey : null;

        private void Awake()
        {
            InitializeComponents();
            InitializeHomeState();
        }

        private void OnDestroy()
        {
            DetachFromSlot();
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            _isDragging = true;
            _dragStartPosition = _rectTransform.anchoredPosition;
            _blocksRaycastsBeforeDrag = _canvasGroup.blocksRaycasts;
            _canvasGroup.blocksRaycasts = false;
            _lastClickTime = float.NegativeInfinity;
            _dragOffset = Vector2.zero;
            if (TryGetLocalPointerPosition(eventData, out var localMousePos))
            {
                _dragOffset = _rectTransform.anchoredPosition - localMousePos;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            if (TryGetLocalPointerPosition(eventData, out var localMousePos))
            {
                _rectTransform.anchoredPosition = localMousePos + _dragOffset;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            EndDragState();
            if (TrySnapToNearestSlot(out var targetSlot) && TryPlaceInSlot(targetSlot)) return;
            RestorePositionAfterFailedDrag();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isDragging || eventData.dragging) return;
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    HandleLeftClick();
                    break;
                case PointerEventData.InputButton.Right:
                    HandleRightClick();
                    break;
            }
        }
        
        public bool TryPlaceInSlot(Slot targetSlot)
        {
            if (!targetSlot) return false;
            if (!targetSlot.TryOccupy(this)) return false;
            if (_currentSlot != targetSlot)
            {
                var previousSlot = _currentSlot;
                _currentSlot = targetSlot;
                if (previousSlot) previousSlot.Release(this);
            }
            EndDragState();
            RefreshSlotPosition();
            return true;
        }
        
        public void RefreshSlotPosition()
        {
            if (!_currentSlot || _isDragging) return;
            _rectTransform.position = _currentSlot.transform.position;
            _rectTransform.anchoredPosition += _correctionOffset;
        }
        
        public void ReturnToHome(bool closeWindow)
        {
            EndDragState();
            DetachFromSlot();
            if (_homeParent && _rectTransform.parent != _homeParent)
            {
                _rectTransform.SetParent(_homeParent, false);
            }
            _rectTransform.anchoredPosition = _homePosition;
            _rectTransform.SetSiblingIndex(_homeSiblingIndex);
            if (closeWindow) CloseWindowIfMine();
        }

        private bool TryGetLocalPointerPosition(PointerEventData eventData, out Vector2 localPosition)
        {
            localPosition = Vector2.zero;
            return _rectTransform.parent is RectTransform parentRect && RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventData.pressEventCamera, out localPosition);
        }

        private bool TrySnapToNearestSlot(out Slot targetSlot)
        {
            targetSlot = null;
            if (!SlotManager.Instance) return false;
            var closestDistance = float.MaxValue;
            foreach (var slot in SlotManager.Instance.AllSlots)
            {
                if (!slot) continue;
                if (slot.IsOccupied && slot.Occupant != this) continue;
                var distance = Vector2.Distance(_rectTransform.position, slot.transform.position);
                if (distance >= _slotSnapDistance || distance >= closestDistance) continue;
                closestDistance = distance;
                targetSlot = slot;
            }
            return targetSlot;
        }

        private void RestorePositionAfterFailedDrag()
        {
            if (_currentSlot)
            {
                RefreshSlotPosition();
                return;
            }
            _rectTransform.anchoredPosition = _dragStartPosition;
        }

        private void EndDragState()
        {
            if (!_isDragging) return;

            _isDragging = false;
            _canvasGroup.blocksRaycasts = _blocksRaycastsBeforeDrag;
        }

        private void DetachFromSlot()
        {
            var previousSlot = _currentSlot;
            _currentSlot = null;
            if (previousSlot) previousSlot.Release(this);
        }

        private void HandleLeftClick()
        {
            if (Time.unscaledTime - _lastClickTime < _doubleClickThreshold)
            {
                ToggleTargetWindow();
                _lastClickTime = float.NegativeInfinity;
                return;
            }
            _lastClickTime = Time.unscaledTime;
        }

        private void HandleRightClick()
        {
            if (!_currentSlot) return;
            ReturnToHome(closeWindow: true);
        }

        private void ToggleTargetWindow()
        {
            if (!_targetWindowController) return;
            var key = UnitKey;
            if (string.IsNullOrEmpty(key)) return;
            var windowObject = _targetWindowController.gameObject;
            if (!windowObject.activeSelf)
            {
                OpenWindow(key);
                return;
            }
            if (_currentOpenCharacterKey == key)
            {
                windowObject.SetActive(false);
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
            if (!_targetWindowController || !_targetWindowController.gameObject.activeSelf) return;
            var key = UnitKey;
            if (string.IsNullOrEmpty(key) || _currentOpenCharacterKey != key) return;
            _targetWindowController.gameObject.SetActive(false);
            _currentOpenCharacterKey = null;
        }

        private void InitializeComponents()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _characterID = GetComponent<CharacterID>();
        }

        private void InitializeHomeState()
        {
            _homeParent = _rectTransform.parent;
            _homePosition = _rectTransform.anchoredPosition;
            _homeSiblingIndex = _rectTransform.GetSiblingIndex();
        }
    }
}