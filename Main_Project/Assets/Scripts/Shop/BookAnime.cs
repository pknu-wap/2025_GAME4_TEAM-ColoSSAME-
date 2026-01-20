using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BookEnterExitController : MonoBehaviour
{
    [System.Serializable]
    public class EnterMapping
    {
        [Header("진입 버튼 (TrainButton 등)")]
        public Button enterButton;

        [Header("0.5초 후 SetActive(true) 할 기능 오브젝트들")]
        public GameObject[] targetFields;
    }

    [Header("책 Animator")]
    public Animator bookAnimator;

    [Header("Animator Trigger 이름")]
    public string forwardTrigger = "FlipForward";
    public string backwardTrigger = "FlipBackward";

    [Header("진입 버튼 ↔ 기능 오브젝트 매핑")]
    public EnterMapping[] enterMappings;

    [Header("뒤로가기 버튼")]
    public Button backButton;

    [Header("딜레이(초)")]
    public float enterDelay = 0.5f;

    [Header("연타 방지")]
    public bool blockWhileFlipping = true;

    private bool isFlipping = false;

    private void Awake()
    {
        // 진입 버튼 이벤트 자동 연결
        if (enterMappings != null)
        {
            for (int i = 0; i < enterMappings.Length; i++)
            {
                int idx = i; // 클로저 캡처 방지
                Button btn = enterMappings[idx].enterButton;
                if (btn == null) continue;

                btn.onClick.AddListener(() => OnClickEnter(idx));
            }
        }

        // back 버튼 연결
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnClickBack);
        }
    }

    /// <summary>
    /// 진입 버튼 클릭:
    /// 1) enterButton 전부 숨김
    /// 2) 책 정방향 애니메이션
    /// 3) 0.5초 후 targetFields 전부 SetActive(true)
    /// </summary>
    private void OnClickEnter(int index)
    {
        if (blockWhileFlipping && isFlipping) return;
        if (enterMappings == null || index < 0 || index >= enterMappings.Length) return;

        EnterMapping mapping = enterMappings[index];

        if (mapping.targetFields == null || mapping.targetFields.Length == 0)
        {
            Debug.LogWarning($"⚠️ EnterMapping[{index}]에 targetFields가 없습니다.");
            return;
        }

        if (bookAnimator == null)
        {
            Debug.LogError("❌ BookEnterExitController: bookAnimator가 연결되지 않았습니다.");
            return;
        }

        // ✅ 1) 모든 진입 버튼 숨김
        SetAllEnterButtonsVisible(false);

        // ✅ 2) 책 정방향 애니메이션
        isFlipping = true;
        bookAnimator.SetTrigger(forwardTrigger);

        // ✅ 3) 0.5초 후 대상 오브젝트들 활성화
        StartCoroutine(SetTargetsActiveAfterDelay(mapping.targetFields, enterDelay));
    }

    private IEnumerator SetTargetsActiveAfterDelay(GameObject[] targets, float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (GameObject obj in targets)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        isFlipping = false;
    }
    private void OnClickBack()
    {
        if (blockWhileFlipping && isFlipping) return;

        if (bookAnimator == null)
        {
            Debug.LogError("❌ BookEnterExitController: bookAnimator가 연결되지 않았습니다.");
            return;
        }

        isFlipping = true;

        // ✅ 1) 책 애니메이션 역방향 실행
        bookAnimator.SetTrigger(backwardTrigger);

        // ✅ 2) backButton은 즉시 숨김
        if (backButton != null)
            backButton.gameObject.SetActive(false);

        // ✅ 3) 0.5초 후 enterButton 6개 다시 표시
        StartCoroutine(ShowAllEnterButtonsAfterDelay(enterDelay));
    }


    private IEnumerator ShowAllEnterButtonsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        SetAllEnterButtonsVisible(true);
        isFlipping = false;
    }

    /// <summary>
    /// enterButton 6개 전부 보이기/숨기기
    /// </summary>
    private void SetAllEnterButtonsVisible(bool visible)
    {
        for (int i = 0; i < enterMappings.Length; i++)
        {
            if (enterMappings[i].enterButton == null) continue;
            enterMappings[i].enterButton.gameObject.SetActive(visible);
        }
    }

    /// <summary>
    /// (선택) 애니메이션 끝 프레임에 Animation Event로 호출
    /// </summary>
    public void OnFlipFinished()
    {
        isFlipping = false;
    }
}
