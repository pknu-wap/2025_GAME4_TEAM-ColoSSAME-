using System.Collections;
using UnityEngine;

public static class EffectUtil
{
    /// <summary>
    /// Animator가 붙어있는 프리팹을 재생
    /// </summary>
    public static GameObject PlayAnimatorPrefab(GameObject prefab, Transform parent, string stateName, float fade = 0.05f, float lifetime = 0f)
    {
        if (prefab == null) return null;

        // 프리팹 인스턴스 생성
        GameObject inst = Object.Instantiate(prefab, parent.position, parent.rotation);
        inst.name = prefab.name; // (Clone) 제거

        if (parent != null)
            inst.transform.SetParent(parent, false);

        // Animator 상태 재생
        Animator animator = inst.GetComponentInChildren<Animator>();
        if (animator != null && !string.IsNullOrEmpty(stateName))
            animator.CrossFadeInFixedTime(stateName, fade);

        // lifetime 지정 시 자동 파괴
        if (lifetime > 0f)
            Object.Destroy(inst, lifetime);

        return inst;
    }

    /// <summary>
    /// 이미 존재하는 Animator에서 상태 재생
    /// </summary>
    public static Coroutine PlayAnimatorState(MonoBehaviour host, Animator animator, string stateName, float fade = 0.05f)
    {
        if (host == null || animator == null || string.IsNullOrEmpty(stateName))
            return null;

        animator.CrossFadeInFixedTime(stateName, fade);
        return host.StartCoroutine(WaitForState(animator, stateName));
    }

    private static IEnumerator WaitForState(Animator animator, string stateName)
    {
        // 한 프레임 대기 후 상태 확인
        yield return null;

        var st = animator.GetCurrentAnimatorStateInfo(0);
        if (!st.IsName(stateName)) yield break;

        // 루프가 아닌 경우 상태 길이만큼 대기
        if (!st.loop)
            yield return new WaitForSeconds(st.length);
    }
}