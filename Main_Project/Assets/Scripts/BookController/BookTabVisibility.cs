using System;
using UnityEngine;

namespace BattleK.UI.Book
{
    // 특정 페이지가 열려 있는 동안 책갈피 탭을 숨긴다. (예: 상점 카테고리 버튼이 탭 자리를 대신할 때)
    public class BookTabVisibility : MonoBehaviour
    {
        [SerializeField] private BookPageController pageController;

        [Tooltip("이 페이지들이 열려 있는 동안 tabs를 숨긴다.")]
        [SerializeField] private BookPageId[] hideOnPages;

        [Tooltip("숨길 탭 오브젝트. BackButtonAnchor처럼 계속 보여야 하는 오브젝트는 넣지 않는다.")]
        [SerializeField] private GameObject[] tabs;

        private void Awake()
        {
            if (pageController == null)
                pageController = GetComponentInParent<BookPageController>();
        }

        private void OnEnable()
        {
            if (pageController == null)
            {
                Debug.LogError("[BookTabVisibility] pageController가 연결되지 않았습니다.", this);
                return;
            }

            pageController.OnPageChanged += HandlePageChanged;
            pageController.OnPageClosing += HandlePageClosing;
            ApplyVisibility(!ShouldHide(pageController.CurrentPage));
        }

        private void OnDisable()
        {
            if (pageController == null) return;
            pageController.OnPageChanged -= HandlePageChanged;
            pageController.OnPageClosing -= HandlePageClosing;
        }

        private void HandlePageChanged(BookPageId newPage, BookPageId previousPage)
        {
            ApplyVisibility(!ShouldHide(newPage));
        }

        // 숨김 페이지를 떠나기 시작하면 페이지 넘김 동안에도 탭이 보이도록 바로 복구한다.
        private void HandlePageClosing(BookPageId closingPageId)
        {
            if (ShouldHide(closingPageId)) ApplyVisibility(true);
        }

        private bool ShouldHide(BookPageId pageId)
        {
            return hideOnPages != null && Array.IndexOf(hideOnPages, pageId) >= 0;
        }

        private void ApplyVisibility(bool visible)
        {
            if (tabs == null) return;

            foreach (GameObject tab in tabs)
            {
                if (tab != null) tab.SetActive(visible);
            }
        }
    }
}
