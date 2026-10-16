using UnityEngine;
using UnityEngine.UI;

namespace BattleK.UI.Book
{
   [RequireComponent(typeof(Button))]
    public class BookPageButton : MonoBehaviour
    {
        [SerializeField] private BookPageController pageController;
        [SerializeField] private BookPageId pageId;

        [Header("책갈피 탭이면 선택 안 됐을 때 어둡게")]
        [SerializeField] private bool dimWhenUnselected;
        [SerializeField] private Color dimColor = new Color(0.6f, 0.6f, 0.6f, 1f);

        private Button button;
        private Graphic[] graphics;
        private Color[] baseColors;

        private void Awake()
        {
            if (pageController == null)
                pageController = GetComponentInParent<BookPageController>();

            button = GetComponent<Button>();
            button.onClick.AddListener(HandleClick);

            if (dimWhenUnselected)
            {
                graphics = GetComponentsInChildren<Graphic>(true);
                baseColors = new Color[graphics.Length];
                for (int i = 0; i < graphics.Length; i++) baseColors[i] = graphics[i].color;
            }
        }

        private void OnEnable()
        {
            if (!dimWhenUnselected || pageController == null) return;
            pageController.OnPageChanged += HandlePageChanged;
            ApplyHighlight(pageController.CurrentPage);
        }

        private void OnDisable()
        {
            if (!dimWhenUnselected || pageController == null) return;
            pageController.OnPageChanged -= HandlePageChanged;
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(HandleClick);
        }

        private void HandleClick()
        {
            if (pageController == null)
            {
                Debug.LogError("[BookPageButton] pageController가 연결되지 않았습니다.", this);
                return;
            }

            pageController.OpenPage(pageId);
        }

        private void HandlePageChanged(BookPageId newPage, BookPageId previousPage) => ApplyHighlight(newPage);

        private void ApplyHighlight(BookPageId currentPage)
        {
            bool selected = pageController.IsInside(currentPage, pageId);
            for (int i = 0; i < graphics.Length; i++)
                graphics[i].color = selected ? baseColors[i] : baseColors[i] * dimColor;
        }
    }
}
