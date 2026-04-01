using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Projectile
{
    public class ProjectileMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private bool _useMovement = true;
        [SerializeField] private float _speed = 10f;
        [SerializeField] private float _lifeTime = 3f;

        [Header("Direction Settings")]
        [SerializeField] private bool _useFlip = true;
        [SerializeField] private bool _useRotation = true;

        [Header("Art Offset")]
        [SerializeField] private float _baseAngleOffset = 0f;

        private Vector2 _direction;
        private float _timer;

        public void Init(Vector2 direction)
        {
            this._direction = direction.normalized;

            if (this._direction == Vector2.zero) return;

            bool isLeft = this._direction.x < 0f;

            if (this._useFlip)
            {
                this.ApplyFlip(isLeft);
            }

            if (this._useRotation)
            {
                this.ApplyRotation(this._direction, isLeft);
            }
        }

        private void Update()
        {
            this.Move();
            this.CheckLifeTime();
        }

        private void Move()
        {
            if (!this._useMovement) return;

            this.transform.position += (Vector3)(this._direction * this._speed * Time.deltaTime);
        }

        private void CheckLifeTime()
        {
            this._timer += Time.deltaTime;

            if (this._timer >= this._lifeTime)
            {
                Destroy(this.gameObject);
            }
        }

        private void ApplyFlip(bool isLeft)
        {
            Vector3 scale = this.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (isLeft ? -1f : 1f);
            this.transform.localScale = scale;
        }

        private void ApplyRotation(Vector2 direction, bool isLeft)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            if (isLeft)
            {
                angle += 180f;
            }

            this.transform.rotation = Quaternion.Euler(0f, 0f, angle + this._baseAngleOffset);
        }
    }
}