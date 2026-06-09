using UnityEngine;
using UnityEngine.UI;

public class StoreItemUI : MonoBehaviour
{
    [Header("DB")]
    public ItemDatabase itemDatabase;

    [Header("시설 업그레이드")]
    public BuildingUpgradeManager upgradeManager;

    [Header("UI 연결")]
    public Image iconImage;
    public Text nameText;
    public Text priceText;
    public Text descriptionText;
    public Button buyButton;

    private ItemData currentData;
    private int cachedFinalPrice;

    private void OnEnable()
    {
        if (buyButton != null)
        {
            buyButton.onClick.RemoveListener(OnClickBuy);
            buyButton.onClick.AddListener(OnClickBuy);
        }

        if (upgradeManager != null)
        {
            upgradeManager.OnBuildingUpgraded -= HandleBuildingUpgraded;
            upgradeManager.OnBuildingUpgraded += HandleBuildingUpgraded;
        }

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
        if (type == BuildingType.Shop)
            RefreshUI();
    }

    private void HandleMoneyChanged(int money)
    {
        UpdateBuyButtonInteractable();
    }

    /// 슬롯에 아이템 설정
    public void SetItem(ItemData data)
    {
        currentData = data;

        if (currentData == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        RefreshUI();
    }

    /// UI 갱신
    public void RefreshUI()
    {
        if (currentData == null)
            return;

        // 아이콘
        if (iconImage != null)
        {
            iconImage.sprite = currentData.icon;
            iconImage.enabled = (currentData.icon != null);
        }

        // 이름
        if (nameText != null)
            nameText.text = currentData.itemName;

        // 설명
        if (descriptionText != null)
            descriptionText.text = currentData.description;

        // 할인 가격 계산
        cachedFinalPrice = currentData.price;

        if (upgradeManager != null)
        {
            cachedFinalPrice =
                upgradeManager.GetDiscountedShopPrice(currentData.price);
        }

        // 가격 표시
        if (priceText != null)
            priceText.text = cachedFinalPrice.ToString();

        UpdateBuyButtonInteractable();
    }

    private void UpdateBuyButtonInteractable()
    {
        if (buyButton == null)
            return;

        if (UserManager.Instance == null ||
            UserManager.Instance.user == null)
        {
            buyButton.interactable = false;
            return;
        }

        buyButton.interactable =
            UserManager.Instance.user.money >= cachedFinalPrice;
    }
    /// 구매 버튼 클릭
    private void OnClickBuy()
    {
        if (currentData == null)
            return;

        if (ShopController.Instance == null)
        {
            Debug.LogError("ShopController.Instance 없음");
            return;
        }

        ShopController.Instance.TryBuy(
            currentData.id,
            cachedFinalPrice
        );

        UpdateBuyButtonInteractable();
    }
}