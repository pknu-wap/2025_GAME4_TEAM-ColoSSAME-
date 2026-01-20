using UnityEngine;
using UnityEngine.UI;

public class StorePageUI : MonoBehaviour
{
    [Header("현재 페이지에 표시할 아이템 ID")]
    public int itemId; // 인스펙터에서 설정

    [Header("DB")]
    public ItemDatabase itemDatabase;

    [Header("UI 연결")]
    public Image iconImage;       // left/Image
    public Text nameText;         // left/Name (Text or Text Legacy)
    public Text priceText;        // left/Price
    public Text descriptionText;  // left/Description
    public Button buyButton;      // left/Buy

    private ItemData currentData; // 현재 표시 중인 아이템 데이터 캐싱

    private void Start()
    {
        RefreshUI();

        // ✅ Buy 버튼에 클릭 이벤트 자동 연결 (인스펙터에서 안 해도 됨)
        if (buyButton != null)
        {
            buyButton.onClick.RemoveListener(OnClickBuy);
            buyButton.onClick.AddListener(OnClickBuy);
        }
    }

    /// <summary>
    /// itemId의 정보를 DB에서 읽어 UI에 반영
    /// </summary>
    public void RefreshUI()
    {
        if (itemDatabase == null)
        {
            Debug.LogError("❌ StorePageUI: itemDatabase가 연결되지 않았습니다.");
            return;
        }

        currentData = itemDatabase.GetById(itemId);
        if (currentData == null)
        {
            Debug.LogError($"❌ StorePageUI: 아이템 데이터를 찾을 수 없습니다. itemId={itemId}");
            return;
        }

        // 아이콘 표시
        if (iconImage != null)
        {
            iconImage.sprite = currentData.icon;
            iconImage.enabled = (currentData.icon != null);
        }

        // 텍스트 표시
        if (nameText != null) nameText.text = currentData.itemName;
        if (priceText != null) priceText.text = currentData.price.ToString();
        if (descriptionText != null) descriptionText.text = currentData.description;

        // (선택) 돈이 부족하면 버튼 비활성화하고 싶다면 여기서 처리 가능
        // 예: buyButton.interactable = (UserManager.Instance.user.money >= currentData.price);
    }

    /// <summary>
    /// Buy 버튼 클릭 시 구매 요청
    /// </summary>
    private void OnClickBuy()
    {
        if (currentData == null)
        {
            Debug.LogWarning("⚠️ StorePageUI: currentData가 없습니다. RefreshUI가 먼저 필요합니다.");
            return;
        }

        if (ShopController.Instance == null)
        {
            Debug.LogError("❌ ShopController.Instance가 없습니다.");
            return;
        }

        ShopController.Instance.TryBuy(currentData.id);
    }
}
