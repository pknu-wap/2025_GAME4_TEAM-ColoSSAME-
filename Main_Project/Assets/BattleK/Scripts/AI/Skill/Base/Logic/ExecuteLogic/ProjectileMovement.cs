using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Projectile
{
    public class ProjectileMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private bool _useMovement = true;
        [SerializeField] private float _speed = 10f;

        [Header("Direction Settings")]
        [SerializeField] private bool _useFlip = false;
        [SerializeField] private bool _useRotation = true;

        [Header("Visual Reference")]
        [SerializeField] private Transform _visualRoot;

        [Header("Art Offset")]
        [SerializeField] private float _baseAngleOffset = 0f;

        private Vector2 _direction;
        private float _timer;

        public void Init(Vector2 direction)
        {
            this._direction = direction.normalized;

            if (this._direction == Vector2.zero) return;

            bool isLeft = this._direction.x < 0f;

            if (this._useRotation)
            {
                this.ApplyRotation(this._direction);
            }

            if (this._useFlip)
            {
                this.ApplyFlip(isLeft);
            }
        }

        private void Update()
        {
            this.Move();
        }

        private void Move()
        {
            if (!this._useMovement) return;

            this.transform.position += (Vector3)(this._direction * this._speed * Time.deltaTime);
        }

        private void ApplyFlip(bool isLeft)
        {
            if (this._visualRoot == null) return;

            Vector3 scale = this._visualRoot.localScale;
            
            scale.x = Mathf.Abs(scale.x);

            // 왼쪽일 때만 위아래 반전
            scale.y = Mathf.Abs(scale.y) * (isLeft ? -1f : 1f);

            this._visualRoot.localScale = scale;
        }

        private void ApplyRotation(Vector2 direction)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            this.transform.rotation = Quaternion.Euler(0f, 0f, angle + this._baseAngleOffset);
        }
    }
}