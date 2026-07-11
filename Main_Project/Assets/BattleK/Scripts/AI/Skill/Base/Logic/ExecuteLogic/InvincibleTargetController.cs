using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Logic.ExecuteLogic
{
    public sealed class InvincibleTargetController : MonoBehaviour
    {
        private float _releaseTime;

        public bool IsInvincible => Time.time < _releaseTime;

        public void Apply(float duration, bool refreshDuration)
        {
            if (IsInvincible && !refreshDuration)
            {
                return;
            }

            _releaseTime = Time.time + Mathf.Max(0f, duration);
        }

        private void Update()
        {
            if (!IsInvincible)
            {
                Destroy(this);
            }
        }
    }
}
