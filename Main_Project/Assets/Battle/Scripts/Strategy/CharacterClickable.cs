using Battle.Scripts.Strategy;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableCharacter : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 originalPosition;
    private SpriteRenderer spriteRenderer;
    private int originalLayer;
    private PlayerSlot currentSlot;

    private PlayerSlot[] allSlots;

    public StrategyManager strategyManager;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = transform.position;
        originalLayer = gameObject.layer;
        spriteRenderer.color = new Color(1, 1, 1, 0.5f);

        if (currentSlot != null)
        {
            currentSlot.ClearSlot();
            currentSlot = null;
        }

        // 모든 슬롯 가져와서 하이라이트 켜기
        allSlots = FindObjectsOfType<PlayerSlot>();
        foreach (var slot in allSlots)
        {
            slot.EnableHighlight(true);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        mouseWorldPos.z = 0;
        transform.position = mouseWorldPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        mouseWorldPos.z = 0;

        bool snapped = false;

        foreach (var slot in allSlots)
        {
            if (Vector2.Distance(mouseWorldPos, slot.transform.position) < slot.snapRange)
            {
                slot.SnapCharacter(transform);
                gameObject.layer = 9;
                currentSlot = slot;
                snapped = true;
                break;
            }
        }

        if (!snapped)
        {
            ReturnToOriginalPosition();
        }

        spriteRenderer.color = Color.white;
        strategyManager.IsDeployed();

        // 드래그 종료 후 슬롯 하이라이트 끄기
        foreach (var slot in allSlots)
        {
            slot.EnableHighlight(false);
        }
    }

    public void ReturnToOriginalPosition()
    {
        transform.position = originalPosition;
        gameObject.layer = originalLayer;
    }
}
