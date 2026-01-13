using UnityEngine;
using UnityEngine.UI;

public class PurchaseConfirmPopup : MonoBehaviour
{
    [Header("팝업 루트(켜고 끌 대상)")]
    public GameObject popupRoot;   // 예: PurchasePopup(Panel) 또는 이 오브젝트 자신

    [Header("팝업 메시지 Text(구매하시겠습니까?)")]
    public Text messageText;       // 팝업의 Text

    [Header("팝업 아이콘(선택)")]
    public Image previewIconImage; // 있으면 연결

    [Header("안내 문구(돈 부족/구매 완료)")]
    public Text infoText;          // store/InfoText (선택)

    // 현재 구매 대상 아이템 정보 저장
    private int pendingItemId = -1;
    private int pendingPrice = 0;
    private string pendingName = "";

    private void Start()
    {
        // 시작 시 팝업은 꺼두는 게 일반적
        Close();
    }

    /// <summary>
    /// 팝업 열기(상세 패널에서 호출)
    /// </summary>
    public void Open(int itemId, string itemName, int price, Sprite icon)
    {
        pendingItemId = itemId;
        pendingName = itemName;
        pendingPrice = price;

        if (messageText != null)
            messageText.text = $"{itemName}\n{price} 골드로 구매하시겠습니까?";

        if (previewIconImage != null)
        {
            previewIconImage.sprite = icon;
            previewIconImage.enabled = (icon != null);
        }

        if (popupRoot != null) popupRoot.SetActive(true);
        else gameObject.SetActive(true);
    }

    public void Close()
    {
        pendingItemId = -1;
        pendingName = "";
        pendingPrice = 0;

        if (popupRoot != null) popupRoot.SetActive(false);
        else gameObject.SetActive(false);
    }

    /// <summary>
    /// 팝업 안 Buy 버튼(확인) 클릭 시 실제 구매
    /// </summary>
    public void OnClickBuy()
    {
        if (pendingItemId < 0)
        {
            Debug.LogWarning("⚠️ 구매 대상 아이템이 없습니다.");
            return;
        }

        if (UserManager.Instance == null || UserManager.Instance.user == null)
        {
            Debug.LogError("❌ UserManager 또는 user가 준비되지 않았습니다.");
            return;
        }

        // 돈 체크
        int money = UserManager.Instance.user.money; // 네 User 구조에 맞게 변수명 확인
        if (money < pendingPrice)
        {
            ShowInfo("돈이 부족합니다");
            Close();
            return;
        }

        // 돈 차감 (네가 이미 SpendGold를 쓰고 있으면 그걸 쓰는 게 더 깔끔)
        bool success = UserManager.Instance.SpendGold(pendingPrice);
        if (!success)
        {
            ShowInfo("돈이 부족합니다");
            Close();
            return;
        }

        // 인벤토리에 추가 (Dictionary<string,int> 기준)
        // key를 int -> string으로 통일: id를 문자열로 저장
        string key = pendingItemId.ToString();
        UserManager.Instance.AddItem(key, 1);

        ShowInfo("구매 완료!");
        Close();
    }

    /// <summary>
    /// 팝업 Cancel 버튼 클릭
    /// </summary>
    public void OnClickCancel()
    {
        Close();
    }

    private void ShowInfo(string msg)
    {
        if (infoText == null) return;

        infoText.text = msg;
        infoText.gameObject.SetActive(true);

        // 필요하면 일정 시간 후 숨기는 코루틴 추가 가능
    }
}
