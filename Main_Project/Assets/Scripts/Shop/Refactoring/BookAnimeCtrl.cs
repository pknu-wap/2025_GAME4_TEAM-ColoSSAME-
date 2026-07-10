using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BookAnimeCtrl : MonoBehaviour
{
    [Header("애니메이터")]
    public Animator bookAnimator;

    [Header("페이지 버튼")]
    public Button nextButton;
    public Button prevButton;

    [Header("상점 UI")]
    public StoreUIManager storeUIManager;

    [Header("페이지 넘김 시간")]
    public float flipDuration = 0.5f;

    private bool isFlipping = false;
    
    public void NextPage()
    {
        if (isFlipping)
            return;

        StartCoroutine(FlipCoroutine(true));
    }
    
    public void PrevPage()
    {
        if (isFlipping)
            return;

        StartCoroutine(FlipCoroutine(false));
    }

    private IEnumerator FlipCoroutine(bool forward)
    {
        isFlipping = true;

        SetButtonsInteractable(false);

        // 애니메이션 실행
        if (forward)
            bookAnimator.SetTrigger("FlipForward");
        else
            bookAnimator.SetTrigger("FlipBackward");

        // 애니메이션 대기
        yield return new WaitForSeconds(flipDuration);

        // 실제 데이터 페이지 변경
        if (forward)
            storeUIManager.NextPage();
        else
            storeUIManager.PrevPage();

        SetButtonsInteractable(true);

        isFlipping = false;
    }

    private void SetButtonsInteractable(bool value)
    {
        if (nextButton != null)
            nextButton.interactable = value;

        if (prevButton != null)
            prevButton.interactable = value;
    }
}