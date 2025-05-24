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

    public void Init(PuzzleManager manager, GameObject investor, GameObject[] panelsToRestore)
    {
        this.manager = manager;
        this.ownerInvestor = investor;
        this.panelsToShow = panelsToRestore;

        showPanelsButton.onClick.AddListener(() =>
        {
            foreach (var panel in panelsToShow)
                panel.SetActive(true);
            manager.ShowOtherInvestors(ownerInvestor);
        });
    }

    public void Complete()
    {
        OnPuzzleComplete?.Invoke();
    }
}