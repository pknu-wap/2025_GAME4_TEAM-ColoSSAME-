using UnityEngine;

namespace BattleK.Scripts.UI
{
    public class Slot : MonoBehaviour
    {
        public bool IsOccupied = false;
        public UIDrag Occupant;
    
        public void Clear()
        {
            if (!IsOccupied || !Occupant) return;
            Occupant.ReturnToHome(closeWindow: true);

            IsOccupied = false;
            Occupant = null;
        }
    }
}