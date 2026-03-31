using UnityEngine;

public class ShopController : MonoBehaviour
{
    public static ShopController Instance { get; private set; }

    [Header("DB(구매 판단용)")]
    public ItemDatabase itemDatabase;

    [Header("메시지 UI(선택)")]
    public ShopToastUI toastUI;

    [Header("시설 업그레이드 매니저(상점 할인 적용용)")]
    public BuildingUpgradeManager upgradeManager; // ✅ 인스펙터에서 연결 (선택)

    [Header("Gacha")]
    public GachaTableSO gachaTable;
    public int gachaCost = 200;

    // ✅ Page03 Right 패널 표시용: 마지막 가챠 결과 캐시
    public bool HasLastGachaResult { get; private set; }
    public GachaResult LastGachaResult { get; private set; }

    // (선택) UI가 이벤트로 받게 하고 싶다면 사용
    public System.Action<GachaResult> OnGachaRolled;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// 기존 호환용: 내부에서 할인 적용까지 처리
    /// (StorePageUI에서 finalPrice를 넘기면 이 함수는 사용 안 해도 됨)
    /// </summary>
    public bool TryBuy(int itemId)
    {
        if (!TryGetItemData(itemId, out ItemData data))
            return false;

        int finalPrice = data.price;

        // ✅ 일반 상점 아이템만 할인 적용(보험)
        if (upgradeManager != null)
            finalPrice = upgradeManager.GetDiscountedShopPrice(data.price);

        return TryBuy(itemId, finalPrice);
    }

    /// <summary>
    /// ✅ 권장: StorePageUI에서 할인 적용된 finalPrice를 계산해서 넘겨주는 구매 함수
    /// </summary>
    public bool TryBuy(int itemId, int finalPrice)
    {
        if (!TryGetItemData(itemId, out ItemData data))
            return false;

        // ✅ finalPrice 유효성 보정
        if (finalPrice < 1) finalPrice = 1;

        // 돈 차감 (핵심: data.price가 아니라 finalPrice)
        bool paid = UserManager.Instance.SpendGold(finalPrice);
        if (!paid)
        {
            toastUI?.Show("돈이 부족합니다", 1f);
            return false;
        }

        // 인벤 추가
        UserManager.Instance.AddItem(itemId.ToString(), 1);

        toastUI?.Show("구매 완료!", 1f);
        return true;
    }

    /// <summary>
    /// 가챠 1회 실행
    /// - ⚠️ 가챠 비용에는 할인 적용 금지
    /// </summary>
    public bool TryGachaRoll()
    {
        if (UserManager.Instance == null || UserManager.Instance.user == null)
        {
            Debug.LogError("❌ UserManager 또는 user가 준비되지 않았습니다.");
            return false;
        }

        if (gachaTable == null)
        {
            Debug.LogError("❌ ShopController: gachaTable이 연결되지 않았습니다.");
            return false;
        }

        if (itemDatabase == null)
        {
            Debug.LogError("❌ ShopController: itemDatabase가 연결되지 않았습니다.");
            return false;
        }

        // ✅ 1) 비용 차감(할인 적용 금지: gachaCost 그대로)
        if (!UserManager.Instance.SpendGold(gachaCost))
        {
            toastUI?.Show("돈이 부족합니다", 1f);
            return false;
        }

        // ✅ 2) 무조건 결과 반환(가챠 결과 실패 없음)
        GachaResult result = GachaRoller.Roll(gachaTable, itemDatabase);

        // ✅ 3) 결과 캐시 (Page03 Right 표시용)
        HasLastGachaResult = true;
        LastGachaResult = result;
        OnGachaRolled?.Invoke(result);

        // ✅ 4) 지급 + 토스트
        if (result.isItem)
        {
            UserManager.Instance.AddItem(result.itemId.ToString(), result.itemCount);

            ItemData data = itemDatabase.GetById(result.itemId);
            string itemName = (data != null) ? data.itemName : $"Item({result.itemId})";

            toastUI?.Show($"획득: {itemName} x{result.itemCount}", 1f);
            return true;
        }
        else
        {
            UserManager.Instance.AddGold(result.goldAmount);
            toastUI?.Show($"획득: 골드 +{result.goldAmount}", 1f);
            return true;
        }
    }

    /// <summary>
    /// 구매 공통 검증 로직
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