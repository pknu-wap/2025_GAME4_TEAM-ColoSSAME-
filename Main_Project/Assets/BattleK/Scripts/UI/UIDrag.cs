using UnityEngine;
using UnityEngine.EventSystems;

public class UIDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;

    // 드래그 중 복귀 지점(이번 드래그의 시작점: 취소/미스 드롭 시 돌아갈 곳)
    private Vector2 originalPosition;

    // ✅ 최초 목록(풀) 위치: 우클릭 제외 시 항상 여기로
    [Header("원래 목록 영역(풀)으로 돌려보낼 부모 (선택)")]
    public RectTransform homeParent;  // 비워두면 현재 parent 유지
    private Vector2 homePosition;     // 최초 Awake 시점 anchoredPosition
    private int homeSiblingIndex;     // 보기 순서 복원용

    private Slot currentSlot;

    [Header("스냅 판정 거리 (픽셀)")]
    public float slotSnapDistance = 80f;

    [Header("스냅 위치 보정 오프셋")]
    public Vector2 correctionOffset = Vector2.zero;

    [Header("더블클릭 시간 간격 (초)")]
    public float doubleClickThreshold = 0.25f;
    private float lastClickTime = -1f;

    [Header("타겟 선택창 오브젝트 (씬 상의 원본)")]
    public GameObject targetingWindow;

    // ✅ CharacterID 연동
    private CharacterID characterID;

    // 현재 열려있는 캐릭터 키(전역 공유)
    private static string currentOpenCharacterKey = null;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        // home 정보는 "최초 자리"로 고정 저장
        homeParent = homeParent ?? rectTransform.parent as RectTransform;
        homePosition = rectTransform.anchoredPosition;
        homeSiblingIndex = rectTransform.GetSiblingIndex();

        if (targetingWindow == null)
            targetingWindow = GameObject.Find("TargetingWindow");
        if (targetingWindow != null)
            targetingWindow.SetActive(false);

        // ✅ CharacterID 캐시
        characterID = GetComponent<CharacterID>();
        if (characterID == null)
        {
            Debug.LogWarning($"[UIDrag] {gameObject.name}에 CharacterID 컴포넌트가 없습니다. 타겟창 연동이 제한될 수 있습니다.");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 이번 드래그의 되돌릴 자리(슬롯→슬롯 실패 시 돌아갈 위치)
        originalPosition = rectTransform.anchoredPosition;

        // 현재 슬롯 점유 해제
        if (currentSlot != null)
        {
            currentSlot.IsOccupied = false;
            currentSlot.Occupant = null;
            currentSlot = null;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localMousePos))
        {
            rectTransform.anchoredPosition = localMousePos;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 가장 가까운 비어있는 슬롯 찾기
        Slot[] slots = GameObject.FindObjectsOfType<Slot>();
        Slot closestValidSlot = null;
        float closestDistance = float.MaxValue;

        foreach (var slot in slots)
        {
            if (slot.IsOccupied) continue;
            RectTransform slotRect = slot.GetComponent<RectTransform>();
            float distance = Vector2.Distance(rectTransform.anchoredPosition, slotRect.anchoredPosition);
            if (distance < slotSnapDistance && distance < closestDistance)
            {
                closestDistance = distance;
                closestValidSlot = slot;
            }
        }

        if (closestValidSlot != null)
        {
            // 슬롯에 성공적으로 앉음 → 창은 유지
            RectTransform slotRect = closestValidSlot.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = slotRect.anchoredPosition + correctionOffset;

            currentSlot = closestValidSlot;
            closestValidSlot.IsOccupied = true;
            closestValidSlot.Occupant = this;
        }
        else
        {
            // ❗슬롯 스냅 실패 → 직전 자리(슬롯/목록)로만 복귀, 창은 유지
            ReturnToOriginalPosition(closeTargetWindow: false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (Time.unscaledTime - lastClickTime < doubleClickThreshold)
            {
                if (currentSlot != null)
                {
                    HandleTargetWindowToggleOrUpdate();
                }
            }
            lastClickTime = Time.unscaledTime;
        }

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // ❗우클릭 = 배치에서 제외 → 항상 목록(풀)로 복귀 + 창 닫기
            if (currentSlot != null)
            {
                currentSlot.IsOccupied = false;
                currentSlot.Occupant = null;
                currentSlot = null;
            }
            ReturnToHome(closeTargetWindow: true);
        }
    }

    private void HandleTargetWindowToggleOrUpdate()
    {
        if (targetingWindow == null) return;
        var controller = targetingWindow.GetComponent<TargetWindowController>();
        if (controller == null) return;

        // ✅ CharacterID에서 키를 얻는다.
        string key = characterID != null ? characterID.characterKey : null;
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogWarning("[UIDrag] CharacterID.characterKey 가 비어있습니다. 타겟창에 올바른 키를 전달할 수 없습니다.");
            return;
        }

        if (!targetingWindow.activeSelf)
        {
            targetingWindow.SetActive(true);
            controller.SetCharacter(key);
            currentOpenCharacterKey = key;
            return;
        }

        if (currentOpenCharacterKey == key)
        {
            targetingWindow.SetActive(false);
            currentOpenCharacterKey = null;
        }
        else
        {
            controller.SetCharacter(key);
            currentOpenCharacterKey = key;
        }
    }

    public void ReturnToOriginalPosition(bool closeTargetWindow = false)
    {
        rectTransform.anchoredPosition = originalPosition;

        if (closeTargetWindow)
            CloseTargetWindowIfOpenForThis();
    }

    // ✅ 목록(풀)로 복귀: 우클릭 제외 시 항상 호출
    public void ReturnToHome(bool closeTargetWindow = false)
    {
        // 부모 변경이 필요하면(목록 컨테이너가 다를 때)
        if (homeParent != null && rectTransform.parent != homeParent)
        {
            rectTransform.SetParent(homeParent, worldPositionStays: false);
        }

        rectTransform.anchoredPosition = homePosition;
        rectTransform.SetSiblingIndex(homeSiblingIndex);

        if (closeTargetWindow)
            CloseTargetWindowIfOpenForThis();
    }

    private void CloseTargetWindowIfOpenForThis()
    {
        if (targetingWindow == null) return;

        string key = characterID != null ? characterID.characterKey : null;
        if (string.IsNullOrEmpty(key)) return;

        if (targetingWindow.activeSelf && currentOpenCharacterKey == key)
        {
            targetingWindow.SetActive(false);
            currentOpenCharacterKey = null;
        }
    }
}
