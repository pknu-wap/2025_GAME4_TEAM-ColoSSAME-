using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleK.UI.Book
{
    public class BookPageController : MonoBehaviour
    {
        [Serializable]
        public class PageEntry
        {
            public BookPageId id;

            [Tooltip("같은 부모를 공유하는 형제(예: 대표 카테고리 5개)끼리 방향을 비교할 때만 사용. " +
                     "책갈피 위치 순서와 맞추면 된다.")]
            public int order;

            [Tooltip("Back을 눌렀을 때 돌아갈 대상. 대표 카테고리처럼 최상위 페이지면 None으로 둔다.")]
            public BookPageId parent = BookPageId.None;
        }

        [SerializeField] private BookAnimationController animationController;

        [Header("페이지 계층 정의 (형제 order / 부모-자식 parent)")]
        [SerializeField] private PageEntry[] pageEntries;

        [Header("씬 시작 시 애니메이션 없이 곧바로 표시할 기본 페이지")]
        [SerializeField] private BookPageId initialPage = BookPageId.None;

        private static readonly PageEntry NoneEntry = new PageEntry { id = BookPageId.None, order = -1, parent = BookPageId.None };

        private Dictionary<BookPageId, PageEntry> entryLookup;
        private BookPageId currentPage = BookPageId.None;
        private BookPageId pendingPage = BookPageId.None;

        public BookPageId CurrentPage => currentPage;

        public event Action<BookPageId, BookPageId> OnPageChanged;
        public event Action<BookPageId> OnPageClosing;

        private void Start()
        {
            if (currentPage == BookPageId.None && initialPage != BookPageId.None)
                SetInitialPage(initialPage);
        }

        private void Awake()
        {
            EnsureLookupBuilt();
        }

        private void EnsureLookupBuilt()
        {
            if (entryLookup != null) return;

            entryLookup = new Dictionary<BookPageId, PageEntry>();
            if (pageEntries == null) return;

            foreach (PageEntry entry in pageEntries)
            {
                if (entry == null) continue;
                if (entryLookup.ContainsKey(entry.id))
                {
                    Debug.LogWarning($"[BookPageController] {entry.id}가 pageEntries에 중복 등록되어 있습니다.");
                    continue;
                }
                entryLookup.Add(entry.id, entry);
            }
        }

        private void OnEnable()
        {
            if (animationController == null)
            {
                Debug.LogError("[BookPageController] animationController가 연결되지 않았습니다.");
                return;
            }

            animationController.OnForwardCompleted += HandleForwardCompleted;
            animationController.OnBackwardCompleted += HandleBackwardCompleted;
        }

        private void OnDisable()
        {
            if (animationController == null) return;

            animationController.OnForwardCompleted -= HandleForwardCompleted;
            animationController.OnBackwardCompleted -= HandleBackwardCompleted;
        }
        public void OpenPage(BookPageId pageId)
        {
            if (pageId == currentPage) return;
            if (animationController == null || animationController.IsAnimating) return;

            if (!TryGetEntry(pageId, out _))
            {
                Debug.LogError($"[BookPageController] {pageId}가 pageEntries에 등록되어 있지 않습니다.");
                return;
            }

            pendingPage = pageId;

            OnPageClosing?.Invoke(currentPage);

            if (IsForwardTransition(currentPage, pageId))
                animationController.PlayForward();
            else
                animationController.PlayBackward();
        }
        public void GoBack()
        {
            if (!TryGetEntry(currentPage, out PageEntry entry)) return;
            if (entry.parent == currentPage) return; // 더 이상 올라갈 곳 없음

            OpenPage(entry.parent);
        }
        public void SetInitialPage(BookPageId pageId)
        {
            if (currentPage != BookPageId.None) return;

            if (!TryGetEntry(pageId, out _))
            {
                Debug.LogError($"[BookPageController] 초기 페이지 {pageId}가 pageEntries에 등록되어 있지 않습니다.");
                return;
            }

            currentPage = pageId;
            OnPageChanged?.Invoke(currentPage, BookPageId.None);
        }
        public bool IsChildPage(BookPageId pageId)
        {
            return TryGetEntry(pageId, out PageEntry entry) && entry.parent != BookPageId.None;
        }

        private bool IsForwardTransition(BookPageId from, BookPageId to)
        {
            TryGetEntry(from, out PageEntry fromEntry);
            TryGetEntry(to, out PageEntry toEntry);

            if (toEntry.parent == from) return true;
            if (fromEntry.parent == to) return false;

            return toEntry.order > fromEntry.order;
        }

        private bool TryGetEntry(BookPageId id, out PageEntry entry)
        {
            EnsureLookupBuilt();

            if (id == BookPageId.None)
            {
                entry = NoneEntry;
                return true;
            }

            return entryLookup.TryGetValue(id, out entry);
        }

        private void HandleForwardCompleted() => RaisePageChanged(pendingPage);

        private void HandleBackwardCompleted() => RaisePageChanged(pendingPage);

        private void RaisePageChanged(BookPageId newPage)
        {
            BookPageId previousPage = currentPage;
            currentPage = newPage;
            OnPageChanged?.Invoke(currentPage, previousPage);
        }
    }
}