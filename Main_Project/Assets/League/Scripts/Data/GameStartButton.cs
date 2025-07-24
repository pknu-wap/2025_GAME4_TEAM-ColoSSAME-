using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStartButton : MonoBehaviour
{

    /// <summary>
    /// 새 게임 시작 버튼 클릭 시 호출
    /// </summary>
    public void OnClickNewGame()
    {
        // 1. LeagueManager Singleton 호출하여 새 리그 생성
        if (LeagueManager.Instance != null)
        {
            LeagueManager.Instance.NewLeague();
            Debug.Log("✅ New League 생성 완료");
        }
        else
        {
            Debug.LogWarning("❌ LeagueManager.Instance가 존재하지 않습니다.");
        }
        if (UserManager.Instance != null)
        {
            UserManager.Instance.NewUser("User");
            Debug.Log("✅ New User 생성 완료");
        }
        else
        {
            Debug.LogWarning("❌ UserManager.Instance가 존재하지 않습니다.");
        }
    }
}