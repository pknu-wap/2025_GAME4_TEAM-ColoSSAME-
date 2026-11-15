using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BattleK.Scripts.UI
{
    public class SlotManager : MonoBehaviour
    {
        public static SlotManager Instance { get; private set; }

        [Header("배치 슬롯")]
        [SerializeField] private List<Slot> _allSlots = new();

        public IReadOnlyList<Slot> AllSlots => _allSlots;
        public int SlotCount => _allSlots.Count;

        private void Awake()
        {
            if (Instance && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
        
        public bool TryGetSlot(int index, out Slot slot)
        {
            slot = null;
            if (index < 0 || index >= _allSlots.Count) return false;
            slot = _allSlots[index];
            return slot;
        }
        
        public void ClearAllSlots()
        {
            foreach (var slot in _allSlots.Where(slot => slot))
            {
                slot.Clear();
            }
        }
    }
}