using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Projectile
{
    public class ProjectileMovement : MonoBehaviour
    {
        [SerializeField] private float _speed = 10f;
        [SerializeField] private float _lifeTime = 3f;

        private Vector2 _direction;
        private float _timer;

        public void Init(Vector2 direction)
        {
            this._direction = direction.normalized;

            if (this._direction != Vector2.zero)
            {
                float angle = Mathf.Atan2(this._direction.y, this._direction.x) * Mathf.Rad2Deg;
                this.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }

        private void Update()
        {
            this.Move();
            this.CheckLifeTime();
        }

        private void Move()
        {
            transform.position += (Vector3)(this._direction * this._speed * Time.deltaTime);
        }

        private void CheckLifeTime()
        {
            this._timer += Time.deltaTime;

            if (this._timer >= this._lifeTime)
            {
                Destroy(this.gameObject);
            }
        }
    }
}