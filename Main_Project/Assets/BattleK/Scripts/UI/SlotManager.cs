using UnityEngine;

public class SlotManager : MonoBehaviour
{
    public Slot[] allSlots;

    public void ClearAllSlots()
    {
        foreach (var slot in allSlots)
        {
            if (slot.IsOccupied && slot.Occupant != null)
            {
                // 캐릭터 원래 자리로 이동
                slot.Occupant.ReturnToOriginalPosition();

                // 슬롯 초기화
                slot.IsOccupied = false;
                slot.Occupant = null;
            }
        }

        Debug.Log("[ClearAllSlots] 모든 슬롯이 초기화되었습니다.");
    }
}