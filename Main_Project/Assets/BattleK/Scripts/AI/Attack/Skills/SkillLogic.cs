using System.Collections;
using BattleK.Scripts.AI;
using UnityEngine;

public abstract class SkillLogic : MonoBehaviour
{
    // 이펙트 배치 옵션들
    public enum EffectAnchor      { Owner, Target, Custom }     // 기준 트랜스폼
    public enum EffectFollowMode  { Follow, WorldFixed }        // 따라다님/월드 고정
    public enum EffectSortingMode { None, SameAsAnchor, AboveAnchor } // 소팅 보정

    public abstract void Execute(AICore user, Transform target);

    // --------------------------------------------------------------------
    // 이펙트 생성/배치/소팅/수명 — 공통 스폰 헬퍼
    // --------------------------------------------------------------------
    protected GameObject SpawnEffect(
        GameObject prefab,
        AICore user,
        Transform target,
        EffectAnchor anchor                = EffectAnchor.Owner,
        Transform customAnchor             = null,
        EffectFollowMode followMode        = EffectFollowMode.Follow,
        Vector3 offset                     = default,
        float lifetime                     = 0.6f,
        EffectSortingMode sorting          = EffectSortingMode.AboveAnchor,
        bool forceLocalZZero               = true,
        bool autoParticleStopDestroy       = true
    )
    {
        if (prefab == null || user == null) return null;

        // 1) 앵커 선택
        Transform anchorTf = null;
        switch (anchor)
        {
            case EffectAnchor.Owner:  anchorTf = user.transform; break;
            case EffectAnchor.Target: anchorTf = target != null ? target : user.transform; break;
            case EffectAnchor.Custom: anchorTf = customAnchor != null ? customAnchor : user.transform; break;
        }
        if (anchorTf == null) anchorTf = user.transform;

        // 2) 인스턴스 생성
        var instance = Instantiate(prefab);

        // 3) 배치
        if (followMode == EffectFollowMode.Follow)
        {
            instance.transform.SetParent(anchorTf, false);          // 로컬 좌표계로 붙임
            instance.transform.localPosition = offset;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale    = Vector3.one;
            if (forceLocalZZero)
            {
                var lp = instance.transform.localPosition;
                instance.transform.localPosition = new Vector3(lp.x, lp.y, 0f);
            }
        }
        else
        {
            instance.transform.SetParent(null);
            instance.transform.position   = anchorTf.position + offset; // 월드 고정
            instance.transform.rotation   = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
        }

        // 4) 소팅 보정
        if (sorting != EffectSortingMode.None)
        {
            var anchorRenderer = anchorTf.GetComponentInChildren<SpriteRenderer>(true);
            if (anchorRenderer != null)
            {
                foreach (var r in instance.GetComponentsInChildren<SpriteRenderer>(true))
                {
                    if (!r) continue;
                    r.sortingLayerID = anchorRenderer.sortingLayerID;
                    r.sortingOrder   = (sorting == EffectSortingMode.AboveAnchor)
                        ? anchorRenderer.sortingOrder + 1
                        : anchorRenderer.sortingOrder;
                }
            }
        }

        // 5) 파티클 stopAction=Destroy (선택)
        if (autoParticleStopDestroy)
        {
            var pss = instance.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in pss)
            {
                var main = ps.main;
                main.stopAction = ParticleSystemStopAction.Destroy;
                ps.Play();
            }
        }

        // 6) 안전망: lifetime이 있으면 자동 파괴
        if (lifetime > 0f) StartCoroutine(DestroyAfter(instance, lifetime));

        return instance;
    }

    private IEnumerator DestroyAfter(GameObject go, float t)
    {
        yield return new WaitForSeconds(t);
        if (go != null) Destroy(go);
    }

    // --------------------------------------------------------------------
    // 연출 타이밍: 이펙트 먼저, 효과는 지연 후 적용할 때 사용
    // 호출측: yield return LeadDelay(seconds, condition);
    // --------------------------------------------------------------------
    protected IEnumerator LeadDelay(float seconds, bool enabled)
    {
        if (enabled && seconds > 0f)
            yield return new WaitForSeconds(seconds);
    }

    // --------------------------------------------------------------------
    // Renderer/Color 공통 안전 유틸 (AICore의 renderers/originalColors에 직접 반영)
    // --------------------------------------------------------------------
    protected void CacheRenderersAndColors(AICore ai)
    {
        if (!ai) return;

        var latest = ai.GetComponentsInChildren<SpriteRenderer>(true);
        ai.renderers = System.Array.FindAll(latest, r => r != null);

        ai.originalColors = new Color[ai.renderers.Length];
        for (int i = 0; i < ai.renderers.Length; i++)
        {
            var r = ai.renderers[i];
            ai.originalColors[i] = r ? r.color : Color.white;
        }
    }

    protected void RefreshRenderersIfChanged(AICore ai)
    {
        if (!ai) return;

        var latest = ai.GetComponentsInChildren<SpriteRenderer>(true);
        // 길이만 비교하면 부족할 수 있으나, 파괴/생성 변화의 빠른 감지를 위해 경량 체크
        if (latest.Length != (ai.renderers?.Length ?? 0))
        {
            ai.renderers = System.Array.FindAll(latest, r => r != null);
        }
    }

    protected float[] SnapshotAlphas(AICore ai)
    {
        if (!ai || ai.renderers == null) return System.Array.Empty<float>();
        var arr = new float[ai.renderers.Length];
        for (int i = 0; i < ai.renderers.Length; i++)
        {
            var r = ai.renderers[i];
            if (!r) { arr[i] = 1f; continue; }

            if (ai.originalColors != null && i < ai.originalColors.Length)
                arr[i] = ai.originalColors[i].a;
            else
                arr[i] = r.color.a;
        }
        return arr;
    }

    protected void SetAlphaForAll(AICore ai, float alpha)
    {
        if (!ai || ai.renderers == null) return;
        for (int i = 0; i < ai.renderers.Length; i++)
        {
            var r = ai.renderers[i];
            if (!r) continue;
            var c = r.color;
            c.a = alpha;
            r.color = c;
        }
    }
}