using UnityEngine;
using UnityEngine.UI;

namespace BattleK.UI.Book
{
   [RequireComponent(typeof(Button))]
    public class BookPageButton : MonoBehaviour
    {
        [SerializeField] private BookPageController pageController;
        [SerializeField] private BookPageId pageId;

        private Button button;

        private void Awake()
        {
            if (pageController == null)
                pageController = GetComponentInParent<BookPageController>();

            button = GetComponent<Button>();
            button.onClick.AddListener(HandleClick);
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
    }
}