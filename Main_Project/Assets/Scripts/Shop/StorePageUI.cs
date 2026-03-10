using UnityEngine;
using UnityEngine.UI;

public class StorePageUI : MonoBehaviour
{
    [Header("현재 페이지에 표시할 아이템 ID")]
    public int itemId; // 인스펙터에서 설정

    [Header("DB")]
    public ItemDatabase itemDatabase;

    [Header("시설 업그레이드(상점 할인 적용용)")]
    public BuildingUpgradeManager upgradeManager; // ✅ 인스펙터에서 연결

    [Header("UI 연결")]
    public Image iconImage;       
    public Text nameText;         
    public Text priceText;        
    public Text descriptionText;  
    public Button buyButton;      

    private ItemData currentData; 
    private int cachedFinalPrice; // ✅ 현재 UI에 표시된(=결제에 써야 하는) 최종 가격 캐시

    private void OnEnable()
    {
        RefreshUI();

        // ✅ Buy 버튼 이벤트 연결
        if (buyButton != null)
        {
            buyButton.onClick.RemoveListener(OnClickBuy);
            buyButton.onClick.AddListener(OnClickBuy);
        }

        // ✅ 상점 업그레이드 후 가격 자동 갱신
        if (upgradeManager != null)
        {
            upgradeManager.OnBuildingUpgraded -= HandleBuildingUpgraded;
            upgradeManager.OnBuildingUpgraded += HandleBuildingUpgraded;
        }

        // ✅ 돈 변화 시 버튼 interactable 갱신하고 싶다면(선택)
        if (UserManager.Instance != null)
        {
            UserManager.Instance.OnMoneyChanged -= HandleMoneyChanged;
            UserManager.Instance.OnMoneyChanged += HandleMoneyChanged;
        }
    }

    private void OnDisable()
    {
        if (upgradeManager != null)
            upgradeManager.OnBuildingUpgraded -= HandleBuildingUpgraded;

        if (UserManager.Instance != null)
            UserManager.Instance.OnMoneyChanged -= HandleMoneyChanged;
    }

    private void HandleBuildingUpgraded(BuildingType type, int level)
    {
        // 상점 업그레이드일 때만 가격 갱신
        if (type == BuildingType.Shop)
            RefreshUI();
    }

    private void HandleMoneyChanged(int money)
    {
        UpdateBuyButtonInteractable();
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

        // 이름/설명
        if (nameText != null) nameText.text = currentData.itemName;
        if (descriptionText != null) descriptionText.text = currentData.description;

        // ✅ 할인 적용된 최종 가격 계산 + 표시
        cachedFinalPrice = currentData.price;
        if (upgradeManager != null)
            cachedFinalPrice = upgradeManager.GetDiscountedShopPrice(currentData.price);

        if (priceText != null) priceText.text = cachedFinalPrice.ToString();

        UpdateBuyButtonInteractable();

        // ✅ 디버그(원가/할인율/최종가 확인)
        // if (upgradeManager != null)
        //     Debug.Log($"[StorePageUI] base={currentData.price}, discount={upgradeManager.GetCurrentDiscountRate(BuildingType.Shop)}, final={cachedFinalPrice}");
    }

    private void UpdateBuyButtonInteractable()
    {
        if (buyButton == null) return;

        if (UserManager.Instance == null || UserManager.Instance.user == null)
        {
            buyButton.interactable = false;
            return;
        }

        buyButton.interactable = (UserManager.Instance.user.money >= cachedFinalPrice);
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

        // ✅ 핵심: 할인 적용된 최종 가격으로 구매 요청
        ShopController.Instance.TryBuy(currentData.id, cachedFinalPrice);

        // 구매 후 UI 갱신(돈/버튼 상태)
        UpdateBuyButtonInteractable();
    }
}