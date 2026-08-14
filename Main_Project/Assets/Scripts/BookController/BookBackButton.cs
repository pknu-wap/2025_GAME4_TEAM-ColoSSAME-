using UnityEngine;
using UnityEngine.UI;

namespace BattleK.UI.Book
{
    public class BookBackButton : MonoBehaviour
    {
        [SerializeField] private BookPageController pageController;
        [SerializeField] private Button button;

        private void Awake()
        {
            if (pageController == null)
                pageController = GetComponentInParent<BookPageController>();

            if (button != null)
                button.onClick.AddListener(HandleClick);
            else
                Debug.LogError("[BookBackButton] button이 연결되지 않았습니다.", this);
        }

        private void OnEnable()
        {
            if (pageController == null)
            {
                Debug.LogError("[BookBackButton] pageController가 연결되지 않았습니다.", this);
                return;
            }

            pageController.OnPageChanged += HandlePageChanged;
            ApplyVisibility(pageController.IsChildPage(pageController.CurrentPage));
        }

        private void OnDisable()
        {
            if (pageController == null) return;
            pageController.OnPageChanged -= HandlePageChanged;
        }

        private void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(HandleClick);
        }

        private void HandleClick()
        {
            pageController?.GoBack();
        }

        private void HandlePageChanged(BookPageId newPage, BookPageId previousPage)
        {
            ApplyVisibility(pageController.IsChildPage(newPage));
        }

        private void ApplyVisibility(bool visible)
        {
            if (button != null) button.gameObject.SetActive(visible);
        }
    }
}