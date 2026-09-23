using UnityEngine;

namespace BattleK.Scripts.AI.Attack.Modules
{
    public class AccelerationModule : MonoBehaviour, IProjectileVelocityModifier
    {
        [Header("Acceleration")]
        [SerializeField] private int _priority = 10;
        [SerializeField] private float _acceleration = 5f;
        [SerializeField] private float _maxSpeed = 25f;

        public int Priority => _priority;
        public void ModifyVelocity(ref Vector2 direction, ref float speed, float deltaTime, Transform self)
        {
            speed = Mathf.Min(speed + _acceleration * deltaTime, _maxSpeed);
        }
    }
}
