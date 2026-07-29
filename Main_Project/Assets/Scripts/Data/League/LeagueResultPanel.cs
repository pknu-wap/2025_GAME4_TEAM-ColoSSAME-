using UnityEngine;
using UnityEngine.SceneManagement;

public class LeagueResultPanel : MonoBehaviour
{
    [SerializeField] private GameObject root;         // leagueResultBackground
    [SerializeField] private GameObject victoryPanel; // leagueVictory
    [SerializeField] private GameObject defeatPanel;  // leagueDefeat


    public void ShowVictory()
    {
        root.SetActive(true);
        victoryPanel.SetActive(true);
        defeatPanel.SetActive(false);
    }

    public void ShowDefeat()
    {
        root.SetActive(true);
        victoryPanel.SetActive(false);
        defeatPanel.SetActive(true);
    }

    public void OnClickNextLeague()
    {
        LeagueManager.Instance.StartNextLeague();
        ReloadScene();
    }

    public void OnClickRetry()
    {
        LeagueManager.Instance.RollbackToLeagueStart();
        SceneTransition.Instance.Load(SceneManager.GetActiveScene().name);
        ReloadScene();
    }


    private void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}