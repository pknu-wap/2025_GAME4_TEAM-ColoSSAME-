using UnityEngine;
using UnityEngine.UI;

public class NegotiationItem : MonoBehaviour
{
    public Image itemImage; // 이 Prefab 내의 아이템 이미지
    public GameObject resultParent; // 왼쪽 영역 부모 오브젝트 (GridLayoutGroup이 붙어 있음)

    public void OnClick()
    {
        // 새로운 GameObject 생성
        GameObject newImgObj = new GameObject("NegotiatedItem");
        newImgObj.transform.SetParent(resultParent.transform, false);

        // Image 컴포넌트 추가
        Image img = newImgObj.AddComponent<Image>();
        img.sprite = itemImage.sprite; // 현재 오브젝트의 이미지 복사
        img.preserveAspect = true;

        // 원하는 크기 설정 (optional)
        RectTransform rt = newImgObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(64, 64); // 원하는 크기로 조정
    }
}