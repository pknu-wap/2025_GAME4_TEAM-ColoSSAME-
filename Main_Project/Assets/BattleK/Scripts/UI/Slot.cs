using UnityEngine;

namespace BattleK.Scripts.UI
{
    public class Slot : MonoBehaviour
    {
        private UIDrag _occupant;

        public UIDrag Occupant => _occupant;
        public bool IsOccupied => _occupant != null;
        internal bool TryOccupy(UIDrag unit)
        {
            if (!unit) return false;
            if (IsOccupied && _occupant != unit) return false;
            _occupant = unit;
            return true;
        }
        
        internal void Release(UIDrag unit)
        {
            if (_occupant != unit) return;
            _occupant = null;
        }

        public void Clear()
        {
            if (!_occupant)
            {
                _occupant = null;
                return;
            }
            _occupant.ReturnToHome(closeWindow: true);
        }
    }
}