using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement; // 또는 패널 전환 방식

public class InvestorButtonHandler : MonoBehaviour
{
    public GameObject loadingPanel; // 로딩 UI
    public GameObject negotiationPanel; // 협상 화면 (예: 두 번째 이미지)
    
    // 투자자 선택 버튼에서 이 함수 호출
    public void OnInvestorSelected()
    {
        StartCoroutine(LoadNegotiationScene());
    }

    private IEnumerator LoadNegotiationScene()
    {
        loadingPanel.SetActive(true); // 로딩 화면 보이기

        yield return new WaitForSeconds(2f); // 2초 대기

        loadingPanel.SetActive(false); // 로딩 화면 끄기
        negotiationPanel.SetActive(true); // 협상 화면 활성화
        // 또는 SceneManager.LoadScene("NegotiationScene");
    }
}