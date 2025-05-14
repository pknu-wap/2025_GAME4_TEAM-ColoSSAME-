using System.Collections;
using Battle.Scripts.Ai.State;
using Unity.VisualScripting;
using UnityEngine;

namespace Battle.Scripts.Ai.Weapon
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class ArrowWeapon : MonoBehaviour
    {
        private BattleAI ownerAI;
        public GameObject arrowPrefab;

        public void Initialize(BattleAI ai, GameObject arrow)
        {
            ownerAI = ai;
            Debug.Log("ai대입 성공");
            this.arrowPrefab = arrow;
        }
        
        public void FireArrow()
        {
            GameObject arrow = Instantiate(arrowPrefab, ownerAI.transform.position, ownerAI.transform.rotation);
            arrow.layer = LayerMask.NameToLayer("Default");
    
            ArrowWeapon arrowWeapon = arrow.GetComponent<ArrowWeapon>();
            arrowWeapon.Initialize(ownerAI,arrowPrefab);
            arrowWeapon.StartCoroutine(arrowWeapon.MoveToTarget(ownerAI.CurrentTarget.position, 0.1f));
        }

        private IEnumerator MoveToTarget(Vector3 targetPos, float duration) //duration : 투사체 날아가는 시간
        {
            float elapsed = 0f;
            Vector3 start = transform.position;
            float distance = Vector3.Distance(start, targetPos);
            float speed = distance / duration;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
                yield return null; // 💥 프레임 분할
            }

            transform.position = targetPos;
            Destroy(gameObject);
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            BattleAI targetAI = other.GetComponent<BattleAI>();
            if (targetAI == null || targetAI.team == ownerAI.team) return;
            Destroy(gameObject);

            // 피격 처리: 데미지 전달
            targetAI.StateMachine.ChangeState(new DamageState(targetAI, ownerAI.damage, ownerAI.stunTime));
        }
    }
}
