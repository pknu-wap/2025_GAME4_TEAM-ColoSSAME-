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
    private const float PixelArt = 0.7f; // 캐릭터 픽셀 크기에 맞춰 뽑은 시트

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
        public Fx[] Effects = { };
        public bool ReactAll; // 범위 스킬: 실제로 맞은 상대 전원에게 맞는 반응
        public float Hold; // 0보다 크면 이만큼 기를 모으는 동안 판정을 묶어 둠 (PixelFxHold)
        public Vector3[] Shakes = { }; // 화면 흔들림 (시점, 세기, 길이) (PixelFxShake)

        // 지속 피해: 맞은 상대 몸에 붙어 도는 이펙트 (PixelFxStatus), 체력바 색 (StatusVisualType)
        public Fx Status;
        public float StatusTime;
        public int DotVisual = -1;

        // 선딜 이펙트: 이 시트로 시전 프리팹을 새로 만들어 SO WindupPrefab에 연결
        public Fx WindupFx;
        public string WindupPrefab; // Prefabs/Skills 아래 경로
        public int AnimationIndex = -1; // 0 이상이면 SO 모션도 바꿈 (유닛 ATTACK 목록 순서)
        public float ActiveTime = -1f, Speed = -1f; // 0 이상이면 SO ActiveTime, 프리팹 ProjectileBase 속도도 바꿈

        // 돌진 (PixelFxDash): 달리는 동안 잔상, 실제로 맞힌 순간 OnHit
        public bool Dash;
        public Fx[] Start = { }, OnHit = { };
        public float HitTime, Freeze, Push;
    }

    private static readonly SkillFx[] Skills =
    {
        // 돌진: pixelfx dash-strike 구성 (잔상, 베기, 치명타 섬광). 시트는 거리가 고정이라 나눠서 붙임
        new()
        {
            Prefab = "Gladiator/GLD_T2_1_P", Skill = "2_Gladiator/GLD_T2_1_S", Dash = true,
            OnHit = new[]
            {
                new Fx { Name = "sword-slash", X = 0.3f, Y = -0.26f, Lifetime = 0.45f, Fade = 0.2f },
                new Fx { Name = "crit-hit", FromOwner = false, Scale = 0.3f, Delay = 0.06f, Lifetime = 0.6f, Fade = 0.25f },
            },
            HitTime = 0.06f, Freeze = 0.17f, Push = 3.4f * PushPerUnit, // dash-strike 시트 값
        },
        // 전방 베기: 판정(2x2)과 같이 날아가는 검기 (pixelfx 도구로 새로 만든 sword-wave)
        // 원래 0.2초에 2(속도 10)는 너무 빨라 안 보여서 0.33초에 2(속도 6)로. 시트도 152px(x 0.42 = 2)를 0.33초에 날아감
        // 휘두르는 순간에 날리도록 WindupTime 0.17
        new()
        {
            Prefab = "Gladiator/GLD_T3_1_P", Skill = "2_Gladiator/GLD_T3_1_S", Windup = SwingWindup, ReactAll = true,
            ActiveTime = 0.33f, Speed = 6f,
            Effects = new[] { new Fx { Name = "sword-wave", Lifetime = 0.72f, Fade = 0.15f, MaxTilt = 90f } }, // 판정이 가는 방향 그대로
            OnHit = new[] { new Fx { Name = "hit-spark", FromOwner = false, Scale = 0.32f, Fade = 0.15f } }, // 검기에 맞은 상대마다
        },
        // 회전 베기: 주변 2x2를 한 바퀴 (pixelfx spin-slash 원본 파란색). 고리 반지름 약 0.8
        // 데미지는 시전 즉시, 고리가 닫히는 0.17초에 맞는 반응
        // 모션은 찌르는 것처럼 보이는 1_Skill_Normal 대신 옆으로 휘두르는 0_Attack_Normal (0.17초부터 휘두름)
        new()
        {
            Prefab = "Gladiator/GLD_T3_2_P", Skill = "2_Gladiator/GLD_T3_2_S", ReactAll = true, AnimationIndex = 0,
            Effects = new[] { new Fx { Name = "spin-slash", Scale = 0.55f, Lifetime = 0.75f, Fade = 0.25f } },
        },
        // 궁극기: 기를 모으면 등 뒤에 날이 위로 선 거대한 환영 검이 생기고, 앞으로 휘둘러 땅에 내려찍으면 땅이 갈라져 나감
        // (pixelfx 도구로 새로 만든 phantom-sword, ground-split). 0.7초 모으는 동안 판정은 PixelFxHold가 묶어 둠
        // 갈라짐은 판정(2x3.8)과 같이 0.8초에 7칸 (속도 8.75, 시트 319px x 0.7 = 7)
        new()
        {
            Prefab = "Gladiator/GLD_U_P", Skill = "2_Gladiator/GLD_U_S", Windup = 0f, ReactAll = true, AnimationIndex = 0,
            Hold = 0.7f, ActiveTime = 0.7f + 0.8f, Speed = 7f / 0.8f,
            Effects = new[]
            {
                // 캐릭터와 같은 픽셀 크기로 그린 시트라 0.7 (SPUM 유닛 스케일)
                new Fx { Name = "phantom-sword", Scale = PixelArt, Lifetime = 1.45f, Fade = 0.15f },
                new Fx { Name = "ground-split", Scale = PixelArt, Delay = 0.7f, Lifetime = 1.35f, Fade = 0.2f, MaxTilt = 90f },
            },
            OnHit = new[] { new Fx { Name = "crit-hit", FromOwner = false, Scale = 0.35f, Lifetime = 0.6f, Fade = 0.25f } }, // 맞은 상대마다
            // 내려찍는 순간 크게, 불꽃이 번지는 동안 잔잔하게, 끝 불기둥에서 한 번 더
            Shakes = new[] { new Vector3(0.7f, 0.18f, 0.4f), new Vector3(0.75f, 0.04f, 0.8f), new Vector3(1.5f, 0.08f, 0.25f) },
        },
        // 강한 일격: 불꽃 내려찍기. 이펙트 자체 준비 동작 0.3초는 캐릭터 모션이 하므로 건너뜀
        new()
        {
            Prefab = "Gladiator/GLD_T2_2_P", Skill = "2_Gladiator/GLD_T2_2_S", Windup = SwingWindup,
            Effects = new[] { new Fx { Name = "heavy-cleave", X = 0.05f, Skip = 0.3f, Lifetime = 1.2f, Fade = 0.4f } },
        },
    };

    // 마법사: 불 속성. 시트는 캐릭터와 같은 픽셀 크기로 뽑아 0.7
    private static readonly SkillFx[] MageSkills =
    {
        // 화염 낙인: 1.6초 동안 발밑에 불 마법진이 그려지고 불기둥이 솟으면, 상대가 불꽃에 휩싸여 20초 동안 탐
        new()
        {
            Prefab = "Magician/Magicain_Tier2_Boom", Skill = "1_Magician/MAG_T2_1_S",
            WindupFx = new Fx { Name = "mage-fire-sigil", Scale = PixelArt }, WindupPrefab = "Magician/MAG_T2_1_W",
            Effects = new[] { new Fx { Name = "mage-ignite", FromOwner = false, Scale = PixelArt, Lifetime = 0.75f, Fade = 0.25f } },
            Status = new Fx { Name = "mage-burn", Scale = PixelArt }, StatusTime = 20f, DotVisual = 2, // Burn
        },
    };

    [MenuItem("Tools/pixelfx/스킬 이펙트 교체 (검투사)")]
    public static void Apply()
    {
        Import(Skills);

        // 군단병 T2_2도 검투사 T2_2 프리팹을 같이 쓰고 있어서 군단병 쪽은 복사본으로 분리 (그림 그대로)
        Detach("3_Legionary/LEG_T2_2_S", "Gladiator/GLD_T2_2_P", "Legionary/LEG_T2_2_P");

        foreach (var s in Skills) Set(s);

        AssetDatabase.SaveAssets();
        Debug.Log("pixelfx: 검투사 스킬 이펙트 적용");
    }

    [MenuItem("Tools/pixelfx/스킬 이펙트 교체 (마법사)")]
    public static void ApplyMage()
    {
        Import(MageSkills);
        foreach (var s in MageSkills) Set(s);
        AssetDatabase.SaveAssets();
        Debug.Log("pixelfx: 마법사 스킬 이펙트 적용");
    }

    // 쓰는 시트는 매번 다시 가져옴 (시트를 바꿔도 자르기와 프리팹이 따라오게)
    private static void Import(IEnumerable<SkillFx> list)
    {
        foreach (var n in list.SelectMany(s => s.Effects.Concat(s.Start).Concat(s.OnHit).Append(s.Status).Append(s.WindupFx))
                     .Where(e => e != null).Select(e => e.Name).Distinct())
            PixelFxImporter.ImportSheet(n);
        AssetDatabase.SaveAssets();
    }

    // 시트 하나로 선딜 프리팹 (풀에서 다시 쓰므로 스스로 삭제하지 않게 그림만 복사)
    private static GameObject MakeWindup(Fx e, string prefab)
    {
        var src = AssetDatabase.LoadAssetAtPath<GameObject>($"{FxDir}/Prefabs/{e.Name}.prefab");
        if (src == null) { Debug.LogWarning($"pixelfx: {e.Name} 프리팹 없음"); return null; }
        var path = $"{PrefabDir}/{prefab}.prefab";
        var root = new GameObject(Path.GetFileNameWithoutExtension(path));
        var art = new GameObject(e.Name);
        art.transform.SetParent(root.transform, false);
        art.transform.localPosition = new Vector3(e.X, e.Y, 0f);
        art.transform.localScale = new Vector3(e.Scale, e.Scale, 1f);
        var sr = art.AddComponent<SpriteRenderer>();
        sr.sprite = src.GetComponent<SpriteRenderer>().sprite;
        sr.sortingOrder = SortingOrder;
        art.AddComponent<Animator>().runtimeAnimatorController = src.GetComponent<Animator>().runtimeAnimatorController;
        var saved = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return saved;
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

            var flash = AssetDatabase.LoadAssetAtPath<Material>($"{FxDir}/PixelFxFlash.mat");
            if (s.Dash)
            {
                if (!root.TryGetComponent<PixelFxDash>(out var dash)) dash = root.AddComponent<PixelFxDash>();
                dash.start = s.Start.Select(ToEntry).ToArray();
                dash.onHit = s.OnHit.Select(ToEntry).ToArray();
                dash.hit = new PixelFxStrike.Hit { time = s.HitTime, flash = 0.06f, freeze = s.Freeze, push = s.Push };
                dash.flashMaterial = flash;
            }
            else
            {
                if (!root.TryGetComponent<PixelFxStrike>(out var strike)) strike = root.AddComponent<PixelFxStrike>();
                strike.effects = s.Effects.Select(ToEntry).ToArray();
                strike.hits = s.Effects.SelectMany(ReadHits).ToArray();
                strike.flashMaterial = flash;
                strike.reactAll = s.ReactAll;
                strike.onHit = s.OnHit.Select(ToEntry).ToArray();
            }

            if (s.Hold > 0f)
            {
                if (!root.TryGetComponent<PixelFxHold>(out var hold)) hold = root.AddComponent<PixelFxHold>();
                hold.hold = s.Hold;
                hold.resumeBefore = 0.1f; // 환영 검이 천천히 떨어지니 모션도 조금 일찍 휘두름
            }

            if (s.Status != null)
            {
                if (!root.TryGetComponent<PixelFxStatus>(out var status)) status = root.AddComponent<PixelFxStatus>();
                var e = ToEntry(s.Status);
                status.effect = e.effect;
                status.duration = s.StatusTime;
                status.scale = s.Status.Scale;
                status.offset = new Vector2(s.Status.X, s.Status.Y);
            }

            if (s.Shakes.Length > 0)
            {
                if (!root.TryGetComponent<PixelFxShake>(out var shake)) shake = root.AddComponent<PixelFxShake>();
                shake.shakes = s.Shakes.Select(v => new PixelFxShake.Shake { time = v.x, strength = v.y, duration = v.z }).ToArray();
            }

            if (s.Speed >= 0f && root.TryGetComponent<BattleK.Scripts.AI.Attack.ProjectileBase>(out var move))
            {
                var so = new SerializedObject(move);
                so.FindProperty("_speed").floatValue = s.Speed;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }

        if (s.Windup < 0f && s.AnimationIndex < 0 && s.ActiveTime < 0f && s.WindupFx == null && s.DotVisual < 0) return;
        var skill = AssetDatabase.LoadAssetAtPath<SkillSO>($"{SkillDir}/{s.Skill}.asset");
        if (skill == null) return;
        if (s.Windup >= 0f) skill.WindupTime = s.Windup;
        if (s.WindupFx != null)
        {
            var w = MakeWindup(s.WindupFx, s.WindupPrefab);
            if (w)
            {
                skill.WindupPrefab = w;
                skill.WindupPrefabScale = 1f;
                skill.FadeWindupPrefab = false; // 시트가 알아서 나타나고 사라짐
            }
        }
        if (s.DotVisual >= 0) SetDotVisual(skill, s.DotVisual);
        if (s.AnimationIndex >= 0) skill.AnimationIndex = s.AnimationIndex;
        if (s.ActiveTime >= 0f) skill.ActiveTime = s.ActiveTime;
        EditorUtility.SetDirty(skill);
    }

    // DealDotDamageLogic의 체력바 색 (SerializeReference 안이라 SerializedObject로)
    private static void SetDotVisual(SkillSO skill, int visual)
    {
        var so = new SerializedObject(skill);
        var it = so.GetIterator();
        while (it.Next(true))
            if (it.name == "_visualType" && it.propertyType == SerializedPropertyType.Enum) it.enumValueIndex = visual;
        so.ApplyModifiedPropertiesWithoutUndo();
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
