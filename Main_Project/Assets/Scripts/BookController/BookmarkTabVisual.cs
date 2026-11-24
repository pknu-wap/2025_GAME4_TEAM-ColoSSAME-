using UnityEngine;
using UnityEngine.UI;

namespace BattleK.UI.Book
{
    // 책갈피 선택 표시: 선택된 탭(하위 페이지 포함)은 hover 그림을 유지하고 바깥으로 조금 튀어나온다.
    // BookPageController의 페이지 변경 이벤트만 듣고, 페이지 전환 자체는 BookPageButton이 맡는다.
    public class BookmarkTabVisual : MonoBehaviour
    {
        [SerializeField] private BookPageController pageController;
        [SerializeField] private BookPageId pageId;
        [SerializeField] private Image background;
        [SerializeField] private Sprite normalSprite;
        [SerializeField] private Sprite selectedSprite;
        [SerializeField] private float selectedOffsetX = 8.4f; // 책 픽셀 4 × 책 배율 2.1

        private RectTransform rect;
        private Vector2 basePosition;

        private void Awake()
        {
            rect = (RectTransform)transform;
            basePosition = rect.anchoredPosition;
            if (pageController == null) pageController = FindObjectOfType<BookPageController>();
        }

        private void OnEnable()
        {
            if (pageController == null) return;
            pageController.OnPageChanged += HandlePageChanged;
            Apply(pageController.CurrentPage);
        }

        private void OnDisable()
        {
            if (pageController != null) pageController.OnPageChanged -= HandlePageChanged;
        }

        // OnPageChanged는 (새 페이지, 이전 페이지) 순서로 보낸다.
        private void HandlePageChanged(BookPageId current, BookPageId previous) => Apply(current);

        private void Apply(BookPageId current)
        {
            bool selected = current == pageId || pageController.IsInside(current, pageId);
            if (background != null) background.sprite = selected ? selectedSprite : normalSprite;
            rect.anchoredPosition = basePosition + (selected ? new Vector2(selectedOffsetX, 0f) : Vector2.zero);
        }
    }
}
