using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 공통 UI 버튼. 상태별 그림은 Button의 Sprite Swap으로 바꾸고,
/// 여기서는 그림만으로 표현되지 않는 글자 쪽 규칙을 맞춘다.
/// - 눌림: 글자를 스프라이트 1px만큼 아래로
/// - 비활성: 글자를 회갈색으로
/// </summary>
[AddComponentMenu("UI/Pixel Button")]
public class PixelButton : Button
{
    [SerializeField] private RectTransform label;
    [SerializeField] private Graphic labelGraphic;
    [Tooltip("눌렸을 때 글자가 움직이는 거리. 스프라이트 1px = 화면 배율(책 기준 2.1)")]
    [SerializeField] private Vector2 pressedOffset = new Vector2(0f, -2.1f);
    [SerializeField] private Color labelColor = Color.white;
    [SerializeField] private Color labelDisabledColor = new Color(0.44f, 0.40f, 0.35f, 1f);

    private Vector2 labelBasePosition;
    private bool hasBasePosition;

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);

        if (labelGraphic != null)
        {
            labelGraphic.color = state == SelectionState.Disabled ? labelDisabledColor : labelColor;
        }

        // 에디터 편집 중에는 배치한 위치를 건드리지 않는다.
        if (label == null || !Application.isPlaying) return;

        if (!hasBasePosition)
        {
            labelBasePosition = label.anchoredPosition;
            hasBasePosition = true;
        }

        label.anchoredPosition = state == SelectionState.Pressed
            ? labelBasePosition + pressedOffset
            : labelBasePosition;
    }
}
