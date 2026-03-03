using UnityEngine;

public class ShopController : MonoBehaviour
{
    public static ShopController Instance { get; private set; }

    [Header("DB(구매 판단용)")]
    public ItemDatabase itemDatabase;

    [Header("메시지 UI(선택)")]
    public ShopToastUI toastUI;

    [Header("시설 업그레이드 매니저(상점 할인 적용용)")]
    public BuildingUpgradeManager upgradeManager; // ✅ 인스펙터에서 연결 (선택이지만 권장)

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 기존 호환용: (할인 적용을 ShopController에서 계산하려면 upgradeManager가 필요)
    /// StorePageUI에서 finalPrice를 넘기는 구조라면 이 함수는 안 써도 됨.
    /// </summary>
    public bool TryBuy(int itemId)
    {
        if (!TryGetItemData(itemId, out ItemData data))
            return false;

        int finalPrice = data.price;

        // ✅ upgradeManager가 연결되어 있으면 여기서도 할인 적용 가능(보험)
        if (upgradeManager != null)
            finalPrice = upgradeManager.GetDiscountedShopPrice(data.price);

        return TryBuy(itemId, finalPrice);
    }

    /// <summary>
    /// ✅ 권장: StorePageUI가 할인 적용된 finalPrice를 계산해서 넘겨주는 구매 함수
    /// </summary>
    public bool TryBuy(int itemId, int finalPrice)
    {
        if (!TryGetItemData(itemId, out ItemData data))
            return false;

        // ✅ finalPrice 유효성 보정(실수로 0/음수 넘어오는 상황 방지)
        if (finalPrice < 1) finalPrice = 1;

        // 돈 차감 (핵심: data.price가 아니라 finalPrice)
        bool paid = UserManager.Instance.SpendGold(finalPrice);
        if (!paid)
        {
            toastUI?.Show("돈이 부족합니다", 1f);
            return false;
        }

        // 인벤 추가
        // (참고) 지금 구조는 itemId.ToString()을 key로 쓰고 있으니 그대로 유지
        UserManager.Instance.AddItem(itemId.ToString(), 1);

        toastUI?.Show("구매 완료!", 1f);
        return true;
    }

    /// <summary>
    /// 구매 공통 검증 로직 분리
    /// </summary>
    private bool TryGetItemData(int itemId, out ItemData data)
    {
        data = null;

        if (itemDatabase == null)
        {
            Debug.LogError("❌ ShopController: itemDatabase가 연결되지 않았습니다.");
            return false;
        }

        if (UserManager.Instance == null || UserManager.Instance.user == null)
        {
            Debug.LogError("❌ UserManager 또는 user가 준비되지 않았습니다.");
            return false;
        }

        data = itemDatabase.GetById(itemId);
        if (data == null)
        {
            Debug.LogError($"❌ ShopController: 아이템 데이터 없음 itemId={itemId}");
            return false;
        }

        return true;
    }
}