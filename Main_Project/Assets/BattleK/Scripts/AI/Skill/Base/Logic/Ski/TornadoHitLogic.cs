using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Projectile
{
    public class TornadoHitLogic : MonoBehaviour
    {
        [Header("Damage")]
        [SerializeField] private int _damage = 20;

        [Header("Knockback")]
        [SerializeField] private float _knockbackPower = 3f;

        private StaticAICore _owner;

        public void Init(StaticAICore owner)
        {
            this._owner = owner;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // owner 없으면 종료
            if (this._owner == null)
                return;

            // StaticAICore 검사
            if (!collision.TryGetComponent(out StaticAICore target))
                return;

            // 자기 자신 무시
            if (target == this._owner)
                return;

            // 적 레이어만 허용
            if (((1 << target.gameObject.layer) & this._owner.TargetLayer) == 0)
            {
                return;
            }
            // 데미지
            target.OnTakeDamage(this._damage, false);

            // 넉백
            if (target.TryGetComponent(out Rigidbody2D rb))
            {
                Vector2 dir =
                    (target.transform.position - transform.position).normalized;

                rb.AddForce(
                    dir * this._knockbackPower,
                    ForceMode2D.Impulse);
            }
        }
    }
}