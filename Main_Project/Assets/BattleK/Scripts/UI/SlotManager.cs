using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BattleK.Scripts.UI
{
    public class SlotManager : MonoBehaviour
    {
        public static SlotManager Instance { get; private set; }
        [SerializeField] private List<Slot> _allSlots = new();
        public IReadOnlyList<Slot> AllSlots => _allSlots;

        private void Awake()
        {
            if (Instance && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        public void ClearAllSlots()
        {
            var count = 0;
            foreach (var slot in _allSlots.Where(slot => slot.IsOccupied && slot.Occupant))
            {
                slot.Clear();
                count++;
            }
        }
    }
}