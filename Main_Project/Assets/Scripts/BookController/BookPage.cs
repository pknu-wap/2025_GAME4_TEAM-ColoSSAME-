using UnityEngine;

namespace BattleK.UI.Book
{
    public class BookPage : MonoBehaviour
    {
        [SerializeField] private BookPageController pageController;
        [SerializeField] private BookPageId pageId;
        [SerializeField] private GameObject content;

        private void Awake()
        {
            if (pageController == null)
                pageController = GetComponentInParent<BookPageController>();

            if (pageController == null)
                Debug.LogError("[BookPage] pageController가 연결되지 않았습니다.", this);
            if (content == null)
                Debug.LogError("[BookPage] content가 연결되지 않았습니다.", this);
        }

        private void OnEnable()
        {
            if (pageController == null) return;

            pageController.OnPageChanged += HandlePageChanged;
            pageController.OnPageClosing += HandlePageClosing;
            ApplyState(pageController.CurrentPage == pageId);
        }

        private void OnDisable()
        {
            if (pageController == null) return;
            pageController.OnPageChanged -= HandlePageChanged;
            pageController.OnPageClosing -= HandlePageClosing;
        }
        private void HandlePageClosing(BookPageId closingPageId)
        {
            if (closingPageId == pageId) ApplyState(false);
        }
        private void HandlePageChanged(BookPageId newPage, BookPageId previousPage)
        {
            bool isThisPage = newPage == pageId;
            ApplyState(isThisPage);

            if (isThisPage)
                OnPageOpened();
            else if (previousPage == pageId)
                OnPageClosed();
        }

        private void ApplyState(bool active)
        {
            if (content != null) content.SetActive(active);
        }

        protected virtual void OnPageOpened() { }
        protected virtual void OnPageClosed() { }
    }
}