using UnityEngine;

namespace Battle.Scripts.Value.HpBar
{
    public class HealthBarFixDirection : MonoBehaviour
    {
        private Vector3 originalScale;

        private void Awake()
        {
            originalScale = transform.localScale;
        }

        public void ForceFix()
        {
            float parentScaleX = transform.parent.lossyScale.x;
            float fixDirection = parentScaleX >= 0 ? -1f : 1f;

            // x축만 방향 보정, 나머지는 그대로
            transform.localScale = new Vector3(
                Mathf.Abs(originalScale.x) * fixDirection,
                originalScale.y,
                originalScale.z
            );
        }
    }
}