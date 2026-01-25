using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class LoadManager : MonoBehaviour
{
    private string playerSavePath;
    private string leagueSavePath;
    private string enemySavePath;
    private string userSavePath;

    void Awake()
    {
        playerSavePath = Path.Combine(Application.persistentDataPath, "PlayerSave.json");
        leagueSavePath = Path.Combine(Application.persistentDataPath, "LeagueSave.json");
        enemySavePath = Path.Combine(Application.persistentDataPath, "EnemySave.json");
        userSavePath = Path.Combine(Application.persistentDataPath, "UserSave.json");
    }

    public void OnClickLoad()
    {
        bool playerLoaded = File.Exists(playerSavePath);
        bool leagueLoaded = File.Exists(leagueSavePath);
        bool enemyLoaded = File.Exists(enemySavePath);
        bool userLoaded = File.Exists(userSavePath);

        if (playerLoaded && leagueLoaded && enemyLoaded)
        {
            // 실제로 Load 함수를 호출한다면 여기서 호출
            Debug.Log("✅ 모든 세이브 파일 불러오기 성공");
            
            // 다음 씬으로 이동
            SceneManager.LoadScene("Book5");
        }
        else
        {
            Debug.LogWarning("❌ 세이브 파일 중 하나라도 없습니다. 불러오기 취소");
            // 필요하면 UI에 경고 메시지 표시
        }
    }
}