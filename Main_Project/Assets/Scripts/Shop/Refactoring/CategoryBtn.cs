using UnityEngine;
using UnityEngine.UI;

public class CategoryBtn : MonoBehaviour
{
    [Header("이 버튼이 담당하는 카테고리")]
    public ItemCategory category;

    [Header("Store UI")]
    public StoreUIManager storeUIManager;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.RemoveListener(OnClickButton);
            button.onClick.AddListener(OnClickButton);
        }
    }

    private void OnClickButton()
    {
        if (storeUIManager == null)
        {
            Debug.LogWarning("[CategoryBtn] StoreUIManager가 연결되어 있지 않습니다.");
            return;
        }

        storeUIManager.SelectCategory(category);
    }
}