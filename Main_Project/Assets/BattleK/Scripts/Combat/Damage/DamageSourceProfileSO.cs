using UnityEngine;

namespace BattleK.Scripts.Combat.Damage
{
    [CreateAssetMenu(fileName = "NewDamageSource", menuName = "BattleK/Damage/Source Profile")]
    public class DamageSourceProfileSO : ScriptableObject, IDamageSourceProfile
    {
        [SerializeField] private string _id;
        [SerializeField] private string _group;
        [SerializeField] private DamageDelivery _delivery = DamageDelivery.Direct;
        [SerializeField] private DamageFlags _defaultFlags = DamageFlags.None;

        public string Id => string.IsNullOrEmpty(_id) ? name : _id;
        public string Group => _group;
        public DamageDelivery Delivery => _delivery;
        public DamageFlags DefaultFlags => _defaultFlags;
    }
}