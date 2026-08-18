using System;
using System.Collections;
using UnityEngine;

namespace BattleK.UI.Book
{
    public class BookAnimationController : MonoBehaviour
    {
        [Header("책 Animator")]
        [SerializeField] private Animator bookAnimator;

        [Header("Animator Trigger 이름")]
        [SerializeField] private string forwardTrigger = "FlipForward";
        [SerializeField] private string backwardTrigger = "FlipBackward";

        [Header("Animation Event 유실 대비 타임아웃")]
        [SerializeField] private float forwardTimeoutSeconds = 0.5f;
        [SerializeField] private float backwardTimeoutSeconds = 0.5f;

        public event Action OnForwardStarted;
        public event Action OnForwardCompleted;
        public event Action OnBackwardStarted;
        public event Action OnBackwardCompleted;

        public bool IsAnimating { get; private set; }

        private int animationToken;

        public bool PlayForward()
        {
            if (IsAnimating) return false;

            if (bookAnimator == null)
            {
                Debug.LogError("[BookAnimationController] bookAnimator가 연결되지 않았습니다.");
                return false;
            }

            IsAnimating = true;
            int token = ++animationToken;
            bookAnimator.SetTrigger(forwardTrigger);
            OnForwardStarted?.Invoke();
            StartCoroutine(TimeoutFallback(token, forwardTimeoutSeconds, NotifyForwardAnimationComplete));
            return true;
        }

        public bool PlayBackward()
        {
            if (IsAnimating) return false;

            if (bookAnimator == null)
            {
                Debug.LogError("[BookAnimationController] bookAnimator가 연결되지 않았습니다.");
                return false;
            }

            IsAnimating = true;
            int token = ++animationToken;
            bookAnimator.SetTrigger(backwardTrigger);
            OnBackwardStarted?.Invoke();
            StartCoroutine(TimeoutFallback(token, backwardTimeoutSeconds, NotifyBackwardAnimationComplete));
            return true;
        }
        public void NotifyForwardAnimationComplete()
        {
            if (!IsAnimating) return;
            IsAnimating = false;
            OnForwardCompleted?.Invoke();
        }
        public void NotifyBackwardAnimationComplete()
        {
            if (!IsAnimating) return;
            IsAnimating = false;
            OnBackwardCompleted?.Invoke();
        }
        private IEnumerator TimeoutFallback(int token, float timeoutSeconds, Action notify)
        {
            yield return new WaitForSeconds(timeoutSeconds);
            
            if (!IsAnimating || token != animationToken) yield break;

            Debug.LogWarning("[BookAnimationController] Animation Event를 받지 못해 타임아웃으로 강제 완료 처리합니다. Animator 클립의 이벤트 타이밍을 확인하세요.");
            notify();
        }
    }
}
