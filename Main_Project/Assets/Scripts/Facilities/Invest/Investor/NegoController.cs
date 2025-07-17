using UnityEngine;
using UnityEngine.UI;

public class NegoController : MonoBehaviour
{
    public delegate void PuzzleCompleteAction();
    public event PuzzleCompleteAction OnPuzzleComplete;

    public Button showPanelsButton; // negotiationPanel 안에 있는 버튼
    private PuzzleManager manager;
    private GameObject ownerInvestor;
    private GameObject[] panelsToShow;
    public Button completeButton;
    public void Init(PuzzleManager manager, GameObject investor, GameObject[] panelsToRestore, int nameIndex, MoneyManager moneyManager)
    {
        this.manager = manager;
        this.ownerInvestor = investor;
        this.panelsToShow = panelsToRestore;
        
        CalculatorManager calc = GetComponentInChildren<CalculatorManager>();
        if (calc != null)
            calc.SetMoneyManager(moneyManager);
        
        RandomText randomText = GetComponentInChildren<RandomText>();
        if(randomText != null) randomText.SetIndex(nameIndex);
        if (showPanelsButton != null)
        {
            showPanelsButton.onClick.AddListener(() =>
            {
                foreach (var panel in panelsToShow)
                    panel.SetActive(true);
                manager.ShowOtherInvestors(ownerInvestor);
            });
        }
        if (completeButton != null)
        {
            completeButton.onClick.AddListener(() =>
            {
                Debug.Log("[NegoController] 협상 완료 버튼 클릭 → Complete 호출");
                Complete();
            });
        }
    }

    public void Complete()
    {
        Debug.Log("NegoController 퍼즐 완료됨 → 이벤트 호출");
        OnPuzzleComplete?.Invoke();
    }
}