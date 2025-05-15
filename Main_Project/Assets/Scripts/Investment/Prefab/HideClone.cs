using UnityEngine;
using UnityEngine.UI;

public class HideMyClone : MonoBehaviour
{
    void Start()
    {
        // 버튼 컴포넌트 가져오기
        Button btn = GetComponent<Button>();

        if (btn != null)
        {
            // 클릭 이벤트에 메서드 등록
            btn.onClick.AddListener(HideMyParent);
        }
    }

    void HideMyParent()
    {
        // root 하위에서 Tag가 "investor"인 오브젝트를 찾아서 비활성화
        GameObject parentClone = null;

        foreach (Transform child in transform.root.GetComponentsInChildren<Transform>(true))
        {
            if (child.CompareTag("investor"))
            {
                parentClone = child.gameObject;
                break;
            }
        }

        if (parentClone != null)
        {
            parentClone.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Tag 'investor'를 가진 오브젝트를 찾을 수 없습니다.");
        }
    }

}