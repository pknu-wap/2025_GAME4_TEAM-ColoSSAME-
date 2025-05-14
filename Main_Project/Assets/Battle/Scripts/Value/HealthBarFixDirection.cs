using UnityEngine;

namespace Battle.Scripts.Value
{
    public class HealthBarFixDirection : MonoBehaviour
    {
        public void ForceFix()
        {
            // 부모 포함 실제 화면에 적용된 최종 스케일 계산
            float worldScaleX = transform.lossyScale.x;

            if (worldScaleX < 0)
            {
                // 부모가 뒤집혔으면, 자식 스케일을 반전해서 중화시킴
                Vector3 scale = transform.localScale;
                scale.x *= -1;
                transform.localScale = scale;
            }
        }
    }
}