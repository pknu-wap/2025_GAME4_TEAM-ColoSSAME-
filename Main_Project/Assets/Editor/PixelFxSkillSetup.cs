using System.Collections.Generic;
using System.IO;
using System.Linq;
using BattleK.Scripts.AI.Skill.Base;
using UnityEditor;
using UnityEngine;

// 스킬 그림을 pixelfx 이펙트로 교체 (판정, 데미지, 쿨타임은 그대로)
public static class PixelFxSkillSetup
{
    private const string PrefabDir = "Assets/BattleK/Prefabs/Skills";
    private const string SkillDir = "Assets/BattleK/SO/Skills";
    private const string FxDir = "Assets/BattleK/Packages/pixelfx";
    private const int SortingOrder = 60;

    // pixelfx 기준점은 캐릭터 발밑, 유닛 루트 기준 발밑은 Y -0.25
    // 크기 0.42면 pixelfx 캐릭터(38px)가 우리 캐릭터 키(약 0.5)와 비슷
    private const float Feet = -0.25f;
    private const float Size = 0.42f;

    // 1_Skill_Normal 0.5초 중 0.17초에서 0.27초 사이에 휘두름
    private const float SwingWindup = 0.17f;

    // 맞는 반응: pixelfx push 1당 밀리는 거리 (평타와 같음)
    private const float PushPerUnit = 0.0125f;

    private class Fx
    {
        public string Name;
        public bool FromOwner = true;
        public float Scale = Size, X, Y = Feet, Delay = 0f, Lifetime, Fade, Speed = 1f, Skip;
        public float MaxTilt = 0f; // 스킬 모션은 옆으로 휘두르므로 기울이지 않음
    }

    private class SkillFx
    {
        public string Prefab, Skill; // Prefabs/Skills, SO/Skills 아래 경로 (확장자 없이)
        public float Windup = -1f;   // 0 이상이면 SO WindupTime을 모션에 맞춤
        public Fx[] Effects;
    }

    private static readonly SkillFx[] Skills =
    {
        // 강한 일격: 불꽃 내려찍기. 이펙트 자체 준비 동작 0.3초는 캐릭터 모션이 하므로 건너뜀
        new()
        {
            Prefab = "Gladiator/GLD_T2_2_P", Skill = "2_Gladiator/GLD_T2_2_S", Windup = SwingWindup,
            Effects = new[] { new Fx { Name = "heavy-cleave", X = 0.05f, Skip = 0.3f, Lifetime = 1.2f, Fade = 0.4f } },
        },
    };

    [MenuItem("Tools/pixelfx/스킬 이펙트 교체 (검투사)")]
    public static void Apply()
    {
        // 군단병 T2_2도 검투사 T2_2 프리팹을 같이 쓰고 있어서 군단병 쪽은 복사본으로 분리 (그림 그대로)
        Detach("3_Legionary/LEG_T2_2_S", "Gladiator/GLD_T2_2_P", "Legionary/LEG_T2_2_P");

        foreach (var s in Skills) Set(s);

        AssetDatabase.SaveAssets();
        Debug.Log("pixelfx: 검투사 스킬 이펙트 적용");
    }

    private static void Detach(string skillPath, string shared, string copy)
    {
        var skill = AssetDatabase.LoadAssetAtPath<SkillSO>($"{SkillDir}/{skillPath}.asset");
        var sharedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/{shared}.prefab");
        if (skill == null || sharedPrefab == null || skill.SkillPrefab != sharedPrefab) return;

        var copyPath = $"{PrefabDir}/{copy}.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(copyPath) == null)
            AssetDatabase.CopyAsset($"{PrefabDir}/{shared}.prefab", copyPath);
        skill.SkillPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(copyPath);
        EditorUtility.SetDirty(skill);
    }

    private static void Set(SkillFx s)
    {
        var prefabPath = $"{PrefabDir}/{s.Prefab}.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) == null)
        {
            Debug.LogWarning($"pixelfx: {prefabPath} 없음");
            return;
        }

        var root = PrefabUtility.LoadPrefabContents(prefabPath);
        try
        {
            // 기존 그림만 끔 (판정 컴포넌트는 그대로)
            foreach (var sr in root.GetComponentsInChildren<SpriteRenderer>(true)) sr.enabled = false;
            foreach (var anim in root.GetComponentsInChildren<Animator>(true)) anim.enabled = false;

            if (!root.TryGetComponent<PixelFxStrike>(out var strike)) strike = root.AddComponent<PixelFxStrike>();
            strike.effects = s.Effects.Select(ToEntry).ToArray();
            strike.hits = s.Effects.SelectMany(ReadHits).ToArray();
            strike.flashMaterial = AssetDatabase.LoadAssetAtPath<Material>($"{FxDir}/PixelFxFlash.mat");

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }

        if (s.Windup < 0f) return;
        var skill = AssetDatabase.LoadAssetAtPath<SkillSO>($"{SkillDir}/{s.Skill}.asset");
        if (skill == null) return;
        skill.WindupTime = s.Windup;
        EditorUtility.SetDirty(skill);
    }

    private static PixelFxStrike.Entry ToEntry(Fx e)
    {
        var fx = AssetDatabase.LoadAssetAtPath<GameObject>($"{FxDir}/Prefabs/{e.Name}.prefab");
        if (fx == null) Debug.LogWarning($"pixelfx: {e.Name} 프리팹 없음");
        return new PixelFxStrike.Entry
        {
            effect = fx, fromOwner = e.FromOwner, offset = new Vector2(e.X, e.Y), scale = e.Scale, delay = e.Delay,
            lifetime = e.Lifetime, fade = e.Fade, speed = e.Speed, sortingOrder = SortingOrder, maxTilt = e.MaxTilt, skip = e.Skip,
        };
    }

    // 시트 JSON meta.pixelfx.act.hits (건너뛴 만큼, 재생 속도만큼 당김)
#pragma warning disable CS0649 // JsonUtility가 채움
    [System.Serializable] private class SheetJson { public SheetMeta meta; }
    [System.Serializable] private class SheetMeta { public SheetFx pixelfx; }
    [System.Serializable] private class SheetFx { public SheetAct act; }
    [System.Serializable] private class SheetAct { public SheetHit[] hits; }
    [System.Serializable] private class SheetHit { public float t, freeze, push; }
#pragma warning restore CS0649

    private static IEnumerable<PixelFxStrike.Hit> ReadHits(Fx e)
    {
        var path = $"{FxDir}/Sheets/{e.Name}.json";
        if (!File.Exists(path)) yield break;
        var hits = JsonUtility.FromJson<SheetJson>(File.ReadAllText(path))?.meta?.pixelfx?.act?.hits;
        if (hits == null) yield break;
        foreach (var h in hits)
            yield return new PixelFxStrike.Hit
            {
                time = e.Delay + Mathf.Max(0f, h.t - e.Skip) / e.Speed, flash = 0.06f, freeze = h.freeze, push = Mathf.Abs(h.push) * PushPerUnit,
            };
    }
}
