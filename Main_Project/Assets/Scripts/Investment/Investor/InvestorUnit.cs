using UnityEngine;
using UnityEngine.UI;

public class InvestorUnit : MonoBehaviour
{
    public Button investorButton;
    public GameObject[] panelsToHide;
    
    private GameObject activeNegotiationPanel;
    private GameObject negotiationPanelPrefab;
    private Transform storyUIAnchor;

    private Image myImage;
    private PuzzleManager puzzleManager;

    public void Init(PuzzleManager manager)
    {
        puzzleManager = manager;
        panelsToHide = manager.hidePanels;
        storyUIAnchor = manager.storyUIAnchor;
        negotiationPanelPrefab = manager.negotiationPanelPrefab;

        myImage = GetComponentInChildren<Image>();    
        
        // 클릭 시 nego 패널 set true, 자신을 제외한 prefab들 set false
        investorButton.onClick.AddListener(() =>
        {
            //  negotiationPanel 생성 및 위치 고정
            activeNegotiationPanel = GameObject.Instantiate(negotiationPanelPrefab, storyUIAnchor);
            activeNegotiationPanel.transform.localPosition = Vector3.zero;    // 위치 초기화
            
            // 퍼즐 완료 이벤트 연결 (나중에 랜덤 추가할 예정)
            var nego = activeNegotiationPanel.GetComponent<NegoController>();
            
            nego.Init(manager, this.gameObject, panelsToHide);
            
            nego.OnPuzzleComplete += () =>
            {   
                Debug.Log("onPuzzleComplete");
                puzzleManager.MoveInvestorImageToCompletedSlot(this.gameObject);
                Destroy(this.gameObject);               // 나 자신 삭제
                Destroy(activeNegotiationPanel);        // UI 삭제
            };

            // 외부 UI 및 investor 프리팹 숨기기
            manager.HideOtherInvestors();
            foreach (var panel in panelsToHide)
                panel.SetActive(false);
        });
    }
}