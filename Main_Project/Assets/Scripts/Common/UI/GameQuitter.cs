using UnityEngine;

/// <summary>끝내기 버튼용. 에디터에서는 종료되지 않으므로 로그로 확인한다.</summary>
public class GameQuitter : MonoBehaviour
{
    public void Quit()
    {
        Debug.Log("[GameQuitter] 게임 종료 요청");
        Application.Quit();
    }
}
