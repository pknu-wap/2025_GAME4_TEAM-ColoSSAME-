using UnityEngine;

namespace Battle.Scripts.Value.HpBar
{
    public class HealthBar : MonoBehaviour
    {
        public Transform fill;          // Fill 스프라이트
        public float fullWidth = 1.0f;  // 체력바 전체 너비 (Sprite 기준)

        private Vector3 originalScale = new Vector3(1f, 0.1f, 1f);
        private Vector3 originalPos = new Vector3(0f, -0.15f, 0f);

        void Start()
        {
            if (fill== null)
            {
                fill = transform.Find("Fill");
                if (fill == null)
                {
                    Debug.LogError("FillTransform이 연결되지 않았습니다!", gameObject);
                }
            }
            originalScale = fill.localScale;
            originalPos = fill.localPosition;
        }

        public void SetHealth(float current, float max)
        {
            float ratio = Mathf.Clamp01(current / max);

            // 크기 조절
            fill.localScale = new Vector3(ratio, originalScale.y, originalScale.z);

            // 위치 보정: 줄어든 만큼 왼쪽으로 이동
            float offset = (1f - ratio) * fullWidth * 0.5f;
            fill.localPosition = originalPos - new Vector3(offset, 0f, 0f);
        }
    
    }
}