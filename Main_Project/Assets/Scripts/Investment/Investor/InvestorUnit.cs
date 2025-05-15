using UnityEngine;
using UnityEngine.UI;

public class InvestorUnit : MonoBehaviour
{
    public Button investorButton;        // investor0 버튼
    public GameObject negotiationPanel;  // nego0 패널

    public void Init(PuzzleManager manager)
    {
        // 클릭 시 nego 패널 활성화
        investorButton.onClick.AddListener(() =>
        {
            negotiationPanel.SetActive(true);
        });

        // 퍼즐 완료 시 manager에 알림
        negotiationPanel.GetComponent<NegoController>().OnPuzzleComplete += () =>
        {
            Destroy(this.gameObject); // 자신 삭제
            manager.SpawnNewInvestor(); // 새로 생성
        };
    }
}