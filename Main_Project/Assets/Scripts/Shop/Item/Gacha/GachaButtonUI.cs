using UnityEngine;
using UnityEngine.UI;

public class GachaButtonUI : MonoBehaviour
{
    public Button gachaButton;

    private void OnEnable()
    {
        if (gachaButton != null)
        {
            gachaButton.onClick.RemoveListener(OnClick);
            gachaButton.onClick.AddListener(OnClick);
        }
    }

    private void OnClick()
    {
        if (ShopController.Instance == null)
        {
            Debug.LogError("❌ ShopController.Instance가 없습니다.");
            return;
        }

        ShopController.Instance.TryGachaRoll();
    }
}