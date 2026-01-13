using UnityEngine;
using UnityEngine.UI;

public class ItemDetailPanel : MonoBehaviour
{
    [Header("DB 에셋")]
    public ItemDatabase itemDatabase;

    [Header("상세 UI")]
    public Image iconImage;
    public Text nameText;
    public Text descText;
    public Text priceText;

    [Header("구매 확인 팝업(씬 오브젝트)")]
    public PurchaseConfirmPopup confirmPopup;

    // 현재 선택된 아이템 ID 저장
    private int currentItemId = -1;

    public void Show(int itemId)
    {
        if (itemDatabase == null)
        {
            Debug.LogError("❌ ItemDetailPanel: itemDatabase가 연결되지 않았습니다.");
            return;
        }

        ItemData data = itemDatabase.GetById(itemId);
        if (data == null)
        {
            Debug.LogWarning($"⚠️ ItemDetailPanel: 아이템을 찾지 못함 id={itemId}");
            return;
        }

        currentItemId = data.id;

        if (iconImage != null)
        {
            iconImage.sprite = data.icon;
            iconImage.enabled = (data.icon != null);
        }

        if (nameText != null) nameText.text = data.itemName;
        if (descText != null) descText.text = data.description;
        if (priceText != null) priceText.text = data.price.ToString();
    }

    /// <summary>
    /// ItemDetailPanel 아래 BuyButton이 누르면 호출됨
    /// → 팝업을 띄우기만 한다
    /// </summary>
    public void OnClickOpenConfirmPopup()
    {
        Debug.Log("✅ BuyButton 클릭됨: OnClickOpenConfirmPopup 실행");
        Debug.Log($"currentItemId={currentItemId}");
        Debug.Log($"confirmPopup={(confirmPopup == null ? "NULL" : "OK")}");
        Debug.Log($"itemDatabase={(itemDatabase == null ? "NULL" : "OK")}");
        if (currentItemId < 0)
        {
            Debug.LogWarning("⚠️ 선택된 아이템이 없습니다. (아이템을 먼저 클릭하세요)");
            return;
        }

        if (itemDatabase == null || confirmPopup == null)
        {
            Debug.LogError("❌ itemDatabase 또는 confirmPopup 연결이 필요합니다.");
            return;
        }

        ItemData data = itemDatabase.GetById(currentItemId);
        if (data == null) return;

        // ✅ 팝업에 현재 선택 아이템 정보를 넘기고 팝업 표시
        confirmPopup.Open(currentItemId, data.itemName, data.price, data.icon);
    }
}