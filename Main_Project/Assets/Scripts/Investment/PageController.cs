using System.Collections;
using UnityEngine;

public class BookPageController : MonoBehaviour
{
    public Animator bookAnimator;
    public GameObject[] contentPanels;
    
    private int currentIndex = 0;
    private bool isFlipping = false;

    void Update()
    {
        AnimatorStateInfo stateInfo = bookAnimator.GetCurrentAnimatorStateInfo(0);
    }

    
    public void OnButtonClicked(int targetIndex)
    {
        if (targetIndex == currentIndex || isFlipping) return;

        isFlipping = true;

        // 모든 내용 비활성화
        foreach (GameObject panel in contentPanels)
            panel.SetActive(false);

        // 애니메이션 방향 결정
        if (targetIndex > currentIndex)
            bookAnimator.SetTrigger("FlipForward");
        else
            bookAnimator.SetTrigger("FlipBackward");

        // 현재 인덱스 업데이트 및 0.5초 후 내용 표시
        StartCoroutine(ShowPanelAfterDelay(targetIndex));
        currentIndex = targetIndex;
    }

    IEnumerator ShowPanelAfterDelay(int index)
    {
        yield return new WaitForSeconds(0.5f);
        contentPanels[index].SetActive(true);
        isFlipping = false;
    }
}