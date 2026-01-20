using UnityEngine;

public class ShopController : MonoBehaviour
{
    public static ShopController Instance { get; private set; }

    [Header("DB(구매 판단용)")]
    public ItemDatabase itemDatabase;

    [Header("메시지 UI(선택)")]
    public ShopToastUI toastUI;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public bool TryBuy(int itemId)
    {
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

        ItemData data = itemDatabase.GetById(itemId);
        if (data == null)
        {
            Debug.LogError($"❌ ShopController: 아이템 데이터 없음 itemId={itemId}");
            return false;
        }

        // 돈 차감
        bool paid = UserManager.Instance.SpendGold(data.price);
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
}