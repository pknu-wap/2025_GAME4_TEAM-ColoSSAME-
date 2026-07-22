using System;
using System.Collections;
using UnityEngine;

namespace BattleK.Scripts.Manager
{
    public class UnitMover
    {
        public const float DefaultDuration = 0.5f;

        public IEnumerator MoveTo(Transform target, Vector3 from, Vector3 to, float duration, Action<float> onProgress = null)
        {
            if (duration <= 0f)
            {
                target.localPosition = to;
                onProgress?.Invoke(1f);
                yield break;
            }

            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                target.localPosition = Vector3.Lerp(from, to, t);
                onProgress?.Invoke(t);
                yield return null;
            }

            target.localPosition = to;
        }
    }
}