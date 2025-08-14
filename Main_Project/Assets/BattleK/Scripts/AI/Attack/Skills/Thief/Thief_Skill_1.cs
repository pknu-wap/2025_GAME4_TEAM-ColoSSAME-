using System.Collections;
using UnityEngine;

public class Thief_Skill_1 : SkillLogic
{
    [Header("은신 지속 시간")]
    public float stealthDuration = 3f;

    [Header("스텔스 알파값")]
    [Range(0f, 1f)]
    public float stealthAlpha = 0.3f;

    [Header("스텔스 이펙트 프리팹(선택)")]
    public GameObject effectPrefab;

    [Header("이펙트 유지 시간(초) — 은신과 별개")]
    public float effectLifetime = 0.6f;

    [Header("이펙트 선행 지연(초) — 이펙트 먼저, 효과는 나중")]
    public float effectLeadDelay = 0.15f;

    [Tooltip("true면 첫 발동 때만 지연을 적용, false면 스택 추가 때도 매번 지연")]
    public bool delayOnlyOnFirstStack = true;

    private AICore owner;
    private float originEvasionRate;

    // 중첩 방어용 스택
    private int stealthStack = 0;

    // (선택) 이펙트 핸들
    private GameObject effectInstance;

    public override void Execute(AICore user, Transform target)
    {
        owner = user;
        originEvasionRate = owner.evasionRate;

        // 공통: 안전 캐싱
        CacheRenderersAndColors(owner);

        bool isFirstActivation = (stealthStack == 0);

        // 이펙트: 공통 스폰 (오너 기준, 따라다님, 위로 소팅, effectLifetime 뒤 제거)
        if (isFirstActivation && effectPrefab)
        {
            effectInstance = SpawnEffect(
                prefab: effectPrefab,
                user: owner,
                target: target,
                anchor: EffectAnchor.Owner,
                customAnchor: null,
                followMode: EffectFollowMode.Follow,
                offset: Vector3.zero,
                lifetime: effectLifetime,
                sorting: EffectSortingMode.AboveAnchor,
                forceLocalZZero: true,
                autoParticleStopDestroy: true
            );
        }

        // 스택 증가 & 실행
        stealthStack++;
        StartCoroutine(StealthRoutine(isFirstActivation));
    }

    private IEnumerator StealthRoutine(bool isFirstActivation)
    {
        owner.State = State.Skill;

        // 연출 타이밍: 이펙트가 먼저, 효과는 지연 후
        bool useDelay = effectLeadDelay > 0f && (isFirstActivation || !delayOnlyOnFirstStack);
        yield return LeadDelay(effectLeadDelay, useDelay);

        // 현재 알파 스냅샷(필요 시 참고)
        var beforeAlphas = SnapshotAlphas(owner);

        // 효과 적용
        SetAlphaForAll(owner, stealthAlpha);
        owner.evasionRate = 100f;

        yield return new WaitForSeconds(stealthDuration);

        // 스택 감소
        stealthStack = Mathf.Max(0, stealthStack - 1);

        // 아직 다른 은신이 남아있으면 복구하지 않음
        if (stealthStack > 0) yield break;

        // ----- 최종 복구 -----
        RefreshRenderersIfChanged(owner);

        if (owner.originalColors != null && owner.renderers != null &&
            owner.originalColors.Length == owner.renderers.Length)
        {
            for (int i = 0; i < owner.renderers.Length; i++)
            {
                var r = owner.renderers[i];
                if (!r) continue;
                r.color = owner.originalColors[i];
            }
        }
        else
        {
            SetAlphaForAll(owner, 1f);
        }

        // 회피율 복귀
        owner.evasionRate = originEvasionRate;

        // (안전망) 이펙트 정리
        if (effectInstance) { Destroy(effectInstance); effectInstance = null; }

        // 🔹 SkillLogic 컨테이너 정리
        if (gameObject.scene.IsValid())
        {
            if (gameObject != owner.gameObject) Destroy(gameObject);
            else Destroy(this);
        }
    }
}