using UnityEngine;
using UnityEngine.UI;

public class GachaPageUI : MonoBehaviour
{
    [Header("Left (가챠 상품)")]
    [SerializeField] private Image leftImage;
    [SerializeField] private Text leftNameText;
    [SerializeField] private Text leftPriceText;
    [SerializeField] private Text leftDescText;
    [SerializeField] private Button leftBuyButton;

    [Header("Right (가챠 결과 표시 전용)")]
    [SerializeField] private Image rightImage;
    [SerializeField] private Text rightNameText;
    [SerializeField] private Text rightPriceText;
    [SerializeField] private Text rightDescText;

    [Header("Static Icon (선택)")]
    [SerializeField] private Sprite goldSprite;

    private void OnEnable()
    {
        BindButton();
        RefreshLeftUI();
        ClearRightUI();
    }

    private void OnDisable()
    {
        if (leftBuyButton != null)
            leftBuyButton.onClick.RemoveListener(OnClickRoll);
    }

    private void BindButton()
    {
        if (leftBuyButton != null)
        {
            leftBuyButton.onClick.RemoveListener(OnClickRoll);
            leftBuyButton.onClick.AddListener(OnClickRoll);
        }
    }

    /// <summary>
    /// Left UI 갱신 (할인 적용 금지)
    /// </summary>
    private void RefreshLeftUI()
    {
        if (leftNameText != null)
            leftNameText.text = "랜덤뽑기";

        if (leftPriceText != null)
            leftPriceText.text = "100G";
        
        if (leftDescText != null)
            leftDescText.text = "무작위 아이템 또는 골드를 획득합니다.";

        if (leftPriceText != null && ShopController.Instance != null)
            leftPriceText.text = ShopController.Instance.gachaCost.ToString();

        UpdateBuyInteractable();
    }

    private void UpdateBuyInteractable()
    {
        if (leftBuyButton == null) return;

        if (ShopController.Instance == null ||
            UserManager.Instance == null ||
            UserManager.Instance.user == null)
        {
            leftBuyButton.interactable = false;
            return;
        }

        leftBuyButton.interactable =
            UserManager.Instance.user.money >= ShopController.Instance.gachaCost;
    }

    private void ClearRightUI()
    {
        if (rightImage != null) rightImage.enabled = false;
        if (rightNameText != null) rightNameText.text = "";
        if (rightPriceText != null) rightPriceText.text = "";
        if (rightDescText != null) rightDescText.text = "";
    }

    private void OnClickRoll()
    {
        if (ShopController.Instance == null)
        {
            Debug.LogError("ShopController.Instance 없음");
            return;
        }

        bool success = ShopController.Instance.TryGachaRoll();
        if (!success)
        {
            UpdateBuyInteractable();
            return;
        }

        if (ShopController.Instance.HasLastGachaResult)
        {
            ShowResult(ShopController.Instance.LastGachaResult);
        }

        UpdateBuyInteractable();
    }

    private void ShowResult(GachaResult result)
    {
        if (result.isItem)
        {
            ItemData data =
                ShopController.Instance.itemDatabase.GetById(result.itemId);

            if (rightImage != null)
            {
                if (data != null && data.icon != null)
                {
                    rightImage.sprite = data.icon;
                    rightImage.enabled = true;
                }
                else
                {
                    rightImage.enabled = false;
                }
            }

            if (rightNameText != null)
                rightNameText.text = data != null
                    ? data.itemName
                    : $"Item({result.itemId})";

            if (rightPriceText != null)
                rightPriceText.text = $"x{result.itemCount}";

            if (rightDescText != null)
                rightDescText.text = data != null
                    ? data.description
                    : "획득한 아이템입니다.";
        }
        else
        {
            if (rightImage != null)
            {
                if (goldSprite != null)
                {
                    rightImage.sprite = goldSprite;
                    rightImage.enabled = true;
                }
                else
                {
                    rightImage.enabled = false;
                }
            }

            if (rightNameText != null)
                rightNameText.text = "골드";

            if (rightPriceText != null)
                rightPriceText.text = $"+{result.goldAmount}";

            if (rightDescText != null)
                rightDescText.text = "획득한 골드입니다.";
        }
    }
}