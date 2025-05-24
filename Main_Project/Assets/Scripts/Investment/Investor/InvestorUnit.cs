using UnityEngine;
using UnityEngine.UI;

public class InvestorUnit : MonoBehaviour
{
    public Button investorButton;
    public GameObject[] panelsToHide;
    
    private GameObject activeNegotiationPanel;
    private GameObject negotiationPanelPrefab;
    private Transform storyUIAnchor;
    public void Init(PuzzleManager manager)
    {
        panelsToHide = manager.hidePanels;
        storyUIAnchor = manager.storyUIAnchor;
        negotiationPanelPrefab = manager.negotiationPanelPrefab;
        
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
                Destroy(this.gameObject);               // 나 자신 삭제
                Destroy(activeNegotiationPanel);        // UI 삭제
                manager.SpawnNewInvestor();             // 새 투자자 생성
            };

            // 외부 UI 및 investor 프리팹 숨기기
            manager.HideOtherInvestors(this.gameObject);
            foreach (var panel in panelsToHide)
                panel.SetActive(false);
        });
    }
}