using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InvestorUnit : MonoBehaviour
{
    public Button investorButton;
    public GameObject[] panelsToHide;

    private GameObject activeNegotiationPanel;
    private GameObject negotiationPanelPrefab;
    private Transform storyUIAnchor;

    private Image myImage;
    private PuzzleManager puzzleManager;
    public MoneyManager moneyManager;
    public TextMeshProUGUI names;

    private GameObject loadingPanel;
    private int nameIndex;

    public BGMController  bgmController;
    public void Init(PuzzleManager manager, string nameText, int nameIndex, BGMController bgm)
    {
        puzzleManager = manager;
        panelsToHide = manager.hidePanels;
        storyUIAnchor = manager.storyUIAnchor;
        negotiationPanelPrefab = manager.negotiationPanelPrefab;
        loadingPanel = manager.loadingPanel;
        this.nameIndex = nameIndex;
        bgmController = bgm;
        Debug.Log("bgmController 연결됨" + (bgmController != null));
        myImage = GetComponentInChildren<Image>();
        names.text = nameText;

        investorButton.onClick.RemoveAllListeners();
        investorButton.onClick.AddListener(OnInvestorButtonClicked);
    }

    private void OnInvestorButtonClicked()
    {
        StartCoroutine(ShowNegotiationWithDelay());
    }

    private IEnumerator ShowNegotiationWithDelay()
    {
        Debug.Log("✅ ShowNegotiationWithDelay 시작됨");

        // 👉 투자자 시각 요소 비활성화
        if (myImage != null)
        {
            myImage.enabled = false;
            Debug.Log("🟢 myImage 숨김 처리됨");
        }

        if (names != null)
        {
            names.enabled = false;
            Debug.Log("🟢 이름 텍스트 숨김 처리됨");
        }

        if (investorButton != null)
        {
            investorButton.interactable = false;
            Debug.Log("🟢 버튼 비활성화됨");
        }

        puzzleManager.HideOtherInvestors(this.gameObject);

        foreach (var panel in panelsToHide)
        {
            if (panel != null)
                panel.SetActive(false);
            else
                Debug.LogWarning("⚠ panelsToHide 안에 null이 있음");
        }

        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
            bgmController.PlayLoadingBGM();
            Debug.Log("🔵 loadingPanel 활성화됨");
        }

        yield return new WaitForSeconds(2f);

        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
            Debug.Log("🟢 loadingPanel 비활성화됨");
        }

        if (negotiationPanelPrefab == null)
        {
            Debug.LogError("❌ negotiationPanelPrefab이 null입니다!");
            yield break;
        }

        if (storyUIAnchor == null)
        {
            Debug.LogError("❌ storyUIAnchor가 null입니다!");
            yield break;
        }

        activeNegotiationPanel = Instantiate(negotiationPanelPrefab, storyUIAnchor);
        activeNegotiationPanel.transform.localPosition = Vector3.zero;
        Debug.Log("✅ 협상 패널 생성 완료");

        var nego = activeNegotiationPanel.GetComponent<NegoController>();
        if (nego == null)
        {
            Debug.LogError("❌ NegoController가 협상 패널에 없음!");
            yield break;
        }
        nego.OnPuzzleComplete += () =>
        {
            Debug.Log("🎯 협상 완료 콜백 실행");
            bgmController.PlayDefaultBGM();
            puzzleManager.MoveInvestorImageToCompletedSlot(this.gameObject);
            Destroy(this.gameObject);
            Destroy(activeNegotiationPanel);
        };
        nego.Init(puzzleManager, this.gameObject, panelsToHide, nameIndex, moneyManager);
        Debug.Log("✅ Nego Init 완료");
    }
}
