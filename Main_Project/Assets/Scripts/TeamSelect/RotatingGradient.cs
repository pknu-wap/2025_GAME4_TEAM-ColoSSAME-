using UnityEngine;

public class RotatingGradient : MonoBehaviour
{
    public Material material1;
    public Material material2;
    public float rotationSpeed = 100f; // 초당 회전 속도 (도/초)
    private float currentAngle = 0f;

    void Start()
    {
    }

    void Update()
    {
        
            // 앵글을 시간에 따라 회전 속도만큼 증가시킨다.
            currentAngle += rotationSpeed * Time.deltaTime;
            
            // 360도를 넘어가면 다시 0부터 시작하도록
            if (currentAngle >= 360f)
            {
                currentAngle -= 360f;
            }

            // 셰이더의 _Angle 속성에 계산된 앵글 값을 적용한다.
            material1.SetFloat("_Angle", currentAngle);
            material2.SetFloat("_Angle", currentAngle);
        }
}