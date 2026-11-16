using System.Collections.Generic;
using System.IO;
using System.Linq;
using BattleK.Scripts.AI.Attack;
using BattleK.Scripts.AI.Skill.Base;
using UnityEditor;
using UnityEngine;

// 직업별 평타 그림을 pixelfx 이펙트로 교체 (원거리: 투사체, 근접: 상대 위치 베기)
public static class PixelFxProjectileSetup
{
    private const string AttackDir = "Assets/BattleK/Prefabs/StaticAttack/NormalAttack";
    private const string FxDir = "Assets/BattleK/Packages/pixelfx";
    private const string VisualName = "FxVisual";

    // 유닛(정렬 5) 위에 그림
    private const int SortingOrder = 50;

    private const float ActiveTime = 1.2f; // 기존 0.65, 사거리 끝까지 닿게

    // 판정 크기 (월드). 기존 판정이 그림보다 커서 닿기 전에 맞고 사라졌음
    private const float HitSize = 0.3f;


    // 내려가는 투사체가 맞기 이만큼 전에 도착 높이에 닿음 (덜 내려온 채 사라지면 조금 키움)
    private const float DescendMargin = 0f;

    // 값은 월드 기준 (유닛 루트 스케일 0.7, 키 약 0.5: 몸통 0.2~0.25, 머리 0.45)
    private class Projectile
    {
        public string Prefab, Skill, Fx;
        public float Scale, Speed;
        public float Windup;  // 공격 모션에서 실제로 쏘는 순간 (초)
        public float Height;  // 발밑에서 그림 높이
        public float Forward; // 몸 앞으로 당기는 거리
        // 판정을 그림보다 이만큼 뒤에 둠. 클수록 몸 안쪽까지 들어간 뒤 사라짐
        public float Penetration = 0.4f;
        public float GrowTime = 0.2f; // 쏜 직후 작게 시작해서 원래 크기가 되는 시간 (초)
        public float EndHeight = -1f; // 0 이상이면 날아가며 이 높이로 감 (쏜 직후 커지는 효과 포함)
        public string Cast; // 시전 이펙트 (출발 자리)
        public float CastScale, CastLifetime, CastFade;
        public string Impact; // 타격 이펙트
        public float ImpactScale, ImpactLifetime, ImpactFade;
        public float ImpactDepth = 0.1f; // 사라지는 순간 머리 위치에서 몸 안쪽으로
        public float ImpactHeight;       // 이펙트 기준점 높이 (그림이 기준점보다 위에 있으면 낮게)
        public float HitPush = 0.025f;   // 맞은 상대가 밀리는 거리 (근접 찌르기, 베기는 0.035)
    }

    // 원거리 맞는 반응: 근접보다 짧게 멈춤
    private const float RangedFreeze = 0.08f;

    private static readonly Projectile Fireball = new()
    {
        Prefab = "Magician_NormalAttack", Skill = "Magician_NormalAttack", Fx = "fireball",
        Scale = 0.6f, Speed = 6f,
        Windup = 0.22f, // 0_Attack_Magic 0.42초 중 손 내미는 순간
        Height = 0.2f, Forward = 0.3f, EndHeight = 0.05f, // 배 높이에서 상대 발밑으로
        Cast = "small-explosion", CastScale = 0.1f, CastLifetime = 0.25f, CastFade = 0.1f, // 손앞에서 작게 퍽
        Impact = "small-explosion", ImpactScale = 0.25f, ImpactLifetime = 0.9f, ImpactFade = 0.3f, // 뒤쪽 연기는 잘라냄
        ImpactHeight = -0.2f, // 불덩이가 기준점보다 약 0.35 위에 그려짐 -> 중심이 발밑 근처
        HitPush = 0.035f, // 폭발이라 근접만큼
    };

    private static readonly Projectile Arrow = new()
    {
        Prefab = "Skirmisher_NormalAttack", Skill = "Skirmisher_NormalAttack", Fx = "arrow",
        Scale = 0.35f, Speed = 7f, // 그림 길이 약 0.7 (꼬리 포함)
        Windup = 0.35f, // 0_Attack_Bow 0.83초 중 시위 놓는 순간
        Height = 0.1f, Forward = 0.4f, EndHeight = 0.1f, // 몸통 높이로 수평 (쏜 직후 커지는 효과만 씀)
        Penetration = 0.6f, // 상대 중심을 조금 지나서 사라짐
        GrowTime = 0.1f,
        Cast = "feather-burst", CastScale = 0.15f, CastLifetime = 0.25f, CastFade = 0.12f, // 번쩍 + 깃털 조금
        Impact = "hit-spark", ImpactScale = 0.35f,
        ImpactHeight = 0f, // 불꽃이 기준점보다 약 0.24 위 = 몸통
    };

    // 사제: plasma-shot을 금색 팔레트로 뽑은 holy-shot (머리가 기준점, 위아래 가운데)
    private static readonly Projectile Holy = new()
    {
        Prefab = "Priest_NormalAttack", Skill = "Priest_NormalAttack", Fx = "holy-shot",
        Scale = 0.38f, Speed = 6f, // 길이 약 0.85 (꼬리 포함)
        Windup = 0.22f, // 마법사와 같은 0_Attack_Magic
        Height = 0.2f, Forward = 0.3f, EndHeight = 0.12f, // 배 높이에서 몸통 아래쪽으로
        GrowTime = 0.15f,
        Impact = "holy-hit", ImpactScale = 0.27f, ImpactLifetime = 0.6f, ImpactFade = 0.25f, // zap-hit 금색, 번개 매듭이 기준점
        ImpactHeight = 0.2f,
    };

    // 근접: 상대 위치에 나오는 순간 베기 (값은 월드, X는 상대 쪽 방향)
    // 베기는 시전자 기준으로 그림 중심이 몸 앞 약 0.45(무기 끝)
    // 맞는 반응(번쩍, 밀림, 멈춤)은 시트 JSON의 act.hits 값을 그대로 씀 (pixelfx 근접 공통 방식)
    private class StrikeFx
    {
        public string Fx;
        public bool FromOwner; // 시전자 기준 (베기는 무기 쪽에)
        public float Scale, X, Y, Lifetime, Fade, Speed = 1f;
        public float Delay = 0f; // 늦게 띄울 때
        public float MaxTilt = -1f; // 0 이상이면 이 각도까지만 기울임
    }

    private class Strike
    {
        public string Prefab, Skill;
        public float Windup; // 공격 모션에서 무기가 내려오는 순간
        public int AnimationIndex = -1; // 0 이상이면 평타 모션도 바꿈 (유닛 프리팹 AttackIndex와 맞출 것)
        public StrikeFx[] Effects;
    }

    // 맞는 반응: pixelfx push(허수아비 흔들림 세기) 1당 밀리는 거리 (월드), 번쩍이는 시간
    private const float PushPerUnit = 0.0125f;
    private const float FlashTime = 0.06f;

    // 0_Attack_Normal, ShotSwordAttack_1 둘 다 0.17초에서 0.25초 사이에 휘두름
    private const float MeleeWindup = 0.17f;

    private static readonly Strike[] Strikes =
    {
        new()
        {
            Prefab = "Gladiator_NormalAttack", Skill = "Gladiator_NormalAttack", Windup = MeleeWindup,
            Effects = new[]
            {
                new StrikeFx { Fx = "sword-slash", FromOwner = true, Scale = 0.35f, X = 0.3f, Y = -0.26f, Lifetime = 0.45f, Fade = 0.2f },
            },
        },
        new()
        {
            Prefab = "Legionary_NormalAttack", Skill = "Legionary_NormalAttack", Windup = MeleeWindup,
            AnimationIndex = 7, // ShotSwordAttack_1: 짧게 앞으로 찌르는 모션
            Effects = new[]
            {
                // 창 찌르기: 모션대로 바라보는 쪽 수평으로 (상대가 위아래로 어긋나도 기울이지 않음)
                new StrikeFx { Fx = "thrust", FromOwner = true, Scale = 0.28f, X = 0f, Y = -0.225f, Lifetime = 0.4f, Fade = 0.15f, Speed = 1.2f, MaxTilt = 0f },
            },
        },
        new()
        {
            Prefab = "Assasin_NormalAttack", Skill = "Assasin_NormalAttack", Windup = MeleeWindup,
            Effects = new[]
            {
                // 두 번 베어 X자 (단검 두 자루), 1.6배속. X 중심이 몸 앞 약 0.85
                new StrikeFx { Fx = "double-slash", FromOwner = true, Scale = 0.45f, X = 0.02f, Y = -0.36f, Lifetime = 0.5f, Fade = 0.15f, Speed = 1.6f },
            },
        },
    };

    [MenuItem("Tools/pixelfx/평타 이펙트 교체 (전 직업)")]
    public static void Apply()
    {
        if (AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>($"{FxDir}/Animations/fireball.controller") == null)
            PixelFxImporter.ImportAll();

        // 마법사, 사제: 같이 쓰던 Bullet_NormalAttack을 복사해서 분리
        foreach (var p in new[] { Fireball, Holy })
        {
            var path = $"{AttackDir}/{p.Prefab}.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
                AssetDatabase.CopyAsset($"{AttackDir}/Bullet_NormalAttack.prefab", path);
        }

        SetVisual(Fireball);
        SetSkill(Fireball, AssetDatabase.LoadAssetAtPath<GameObject>($"{AttackDir}/{Fireball.Prefab}.prefab"));

        // 척후병: 기존 프리팹에 바로 적용 (척후병만 사용)
        SetVisual(Arrow);
        SetSkill(Arrow, null);

        SetVisual(Holy);
        SetSkill(Holy, AssetDatabase.LoadAssetAtPath<GameObject>($"{AttackDir}/{Holy.Prefab}.prefab"));

        foreach (var s in Strikes) SetStrike(s);

        AssetDatabase.SaveAssets();
        Debug.Log("pixelfx: 마법사, 척후병, 사제 투사체와 검투사, 군단병, 암살자 근접 이펙트 적용");
    }

    private static void SetStrike(Strike s)
    {
        var prefabPath = $"{AttackDir}/{s.Prefab}.prefab";
        var root = PrefabUtility.LoadPrefabContents(prefabPath);
        try
        {
            // 기존 PixelAttackFx 그림은 끔 (판정은 InstantTargetProcessor 그대로)
            // 베이스 Flash 프리팹 일부가 없는 스프라이트를 가리켜서 Missing으로 보이므로 비움
            foreach (var sr in root.GetComponentsInChildren<SpriteRenderer>(true))
            {
                sr.enabled = false;
                sr.sprite = null;
            }
            foreach (var anim in root.GetComponentsInChildren<Animator>(true)) anim.enabled = false;

            if (!root.TryGetComponent<PixelFxStrike>(out var strike)) strike = root.AddComponent<PixelFxStrike>();
            strike.effects = s.Effects.Select(e =>
            {
                var fx = AssetDatabase.LoadAssetAtPath<GameObject>($"{FxDir}/Prefabs/{e.Fx}.prefab");
                if (fx == null) Debug.LogWarning($"pixelfx: {e.Fx} 프리팹 없음");
                return new PixelFxStrike.Entry
                {
                    effect = fx, fromOwner = e.FromOwner, offset = new Vector2(e.X, e.Y), scale = e.Scale, delay = e.Delay,
                    lifetime = e.Lifetime, fade = e.Fade, speed = e.Speed, sortingOrder = SortingOrder + 10, maxTilt = e.MaxTilt,
                };
            }).ToArray();
            strike.hits = s.Effects.SelectMany(ReadHits).ToArray();
            strike.flashMaterial = FlashMaterial();

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }

        var skill = AssetDatabase.LoadAssetAtPath<SkillSO>($"Assets/BattleK/SO/Skills/00_NormalAttack/{s.Skill}.asset");
        if (skill == null) return;
        skill.WindupTime = s.Windup;
        if (s.AnimationIndex >= 0) skill.AnimationIndex = s.AnimationIndex;
        EditorUtility.SetDirty(skill);
    }

    // 시트 JSON meta.pixelfx.act.hits (t는 이펙트 시작부터, 재생 속도만큼 당김)
#pragma warning disable CS0649 // JsonUtility가 채움
    [System.Serializable] private class SheetJson { public SheetMeta meta; }
    [System.Serializable] private class SheetMeta { public SheetFx pixelfx; }
    [System.Serializable] private class SheetFx { public SheetAct act; }
    [System.Serializable] private class SheetAct { public SheetHit[] hits; }
    [System.Serializable] private class SheetHit { public float t, freeze, push; }
#pragma warning restore CS0649

    private static IEnumerable<PixelFxStrike.Hit> ReadHits(StrikeFx e)
    {
        var path = $"{FxDir}/Sheets/{e.Fx}.json";
        if (!File.Exists(path)) yield break;
        var hits = JsonUtility.FromJson<SheetJson>(File.ReadAllText(path))?.meta?.pixelfx?.act?.hits;
        if (hits == null) yield break;
        foreach (var h in hits)
            yield return new PixelFxStrike.Hit
            {
                time = e.Delay + h.t / e.Speed, flash = FlashTime, freeze = h.freeze, push = Mathf.Abs(h.push) * PushPerUnit,
            };
    }

    private static Material FlashMaterial()
    {
        var path = $"{FxDir}/PixelFxFlash.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat != null) return mat;
        var shader = Shader.Find("PixelFx/Flash");
        if (shader == null)
        {
            Debug.LogWarning("pixelfx: PixelFx/Flash 셰이더 없음");
            return null;
        }
        mat = new Material(shader);
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    private static void SetSkill(Projectile p, GameObject prefab)
    {
        var skill = AssetDatabase.LoadAssetAtPath<SkillSO>($"Assets/BattleK/SO/Skills/00_NormalAttack/{p.Skill}.asset");
        if (skill == null) return;
        if (prefab != null) skill.SkillPrefab = prefab;
        skill.ActiveTime = ActiveTime;
        skill.WindupTime = p.Windup;
        EditorUtility.SetDirty(skill);
    }

    private static void SetVisual(Projectile p)
    {
        var prefabPath = $"{AttackDir}/{p.Prefab}.prefab";
        var ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>($"{FxDir}/Animations/{p.Fx}.controller");
        if (ctrl == null) { Debug.LogWarning($"pixelfx: {p.Fx} 컨트롤러 없음, 시트 가져오기 먼저 실행"); return; }
        Sprite firstSprite = null;
        foreach (var o in AssetDatabase.LoadAllAssetsAtPath($"{FxDir}/Sheets/{p.Fx}.png"))
            if (o is Sprite s && s.name == $"{p.Fx}_000") firstSprite = s;

        var root = PrefabUtility.LoadPrefabContents(prefabPath);
        try
        {
            // 루트 스케일을 나눠서 월드 기준 값으로 맞춤
            var sx = Mathf.Max(0.0001f, Mathf.Abs(root.transform.localScale.x));
            var sy = Mathf.Max(0.0001f, Mathf.Abs(root.transform.localScale.y));
            var forward = p.Forward / sx; // 루트가 날아가는 방향으로 돌아가므로 +x가 앞

            // 기존 그림은 끄고 자식으로 이펙트 추가
            var baseSr = root.GetComponent<SpriteRenderer>();
            if (baseSr) baseSr.enabled = false;

            var old = root.transform.Find(VisualName);
            if (old) Object.DestroyImmediate(old.gameObject);

            var vis = new GameObject(VisualName);
            vis.transform.SetParent(root.transform, false);
            vis.transform.localScale = new Vector3(p.Scale / sx, p.Scale / sx, 1f);
            vis.transform.localPosition = new Vector3(forward, p.Height / sy, 0f);

            var sr = vis.AddComponent<SpriteRenderer>();
            sr.sprite = firstSprite;
            if (baseSr) sr.sortingLayerID = baseSr.sortingLayerID;
            sr.sortingOrder = SortingOrder;
            vis.AddComponent<Animator>().runtimeAnimatorController = ctrl;

            // 판정을 이펙트 머리 위치에 작게
            var hit = HitSize / sx;
            var hitX = (p.Forward - p.Penetration) / sx;
            if (root.TryGetComponent<BoxCollider2D>(out var box)) { box.size = new Vector2(hit, hit); box.offset = new Vector2(hitX, 0f); }
            if (root.TryGetComponent<CircleCollider2D>(out var circle)) { circle.radius = hit * 0.5f; circle.offset = new Vector2(hitX, 0f); }

            // 날아가며 상대 몸통 높이로 내려감
            root.TryGetComponent<PixelFxDescend>(out var descend);
            if (p.EndHeight >= 0f)
            {
                if (!descend) descend = root.AddComponent<PixelFxDescend>();
                descend.visual = vis.transform;
                descend.startHeight = p.Height;
                descend.endHeight = p.EndHeight;
                descend.arriveEarly = DescendMargin; // 맞는 거리는 실행 중 실제 판정 크기로 계산
                descend.growTime = p.GrowTime;
                descend.castEffect = string.IsNullOrEmpty(p.Cast) ? null
                    : AssetDatabase.LoadAssetAtPath<GameObject>($"{FxDir}/Prefabs/{p.Cast}.prefab");
                descend.castScale = p.CastScale;
                descend.castLifetime = p.CastLifetime;
                descend.castFade = p.CastFade;
                descend.castSortingOrder = SortingOrder + 5;
                if (!string.IsNullOrEmpty(p.Cast) && !descend.castEffect) Debug.LogWarning($"pixelfx: {p.Cast} 프리팹 없음");
            }
            else if (descend) Object.DestroyImmediate(descend, true);

            // 맞을 때 타격 이펙트
            if (!root.TryGetComponent<PixelFxImpact>(out var impact)) impact = root.AddComponent<PixelFxImpact>();
            impact.effect = AssetDatabase.LoadAssetAtPath<GameObject>($"{FxDir}/Prefabs/{p.Impact}.prefab");
            impact.point = vis.transform;
            impact.scale = p.ImpactScale;
            impact.lifetime = p.ImpactLifetime;
            impact.fade = p.ImpactFade;
            impact.depth = p.ImpactDepth;
            impact.height = p.ImpactHeight;
            impact.sortingOrder = SortingOrder + 10;
            impact.flashMaterial = FlashMaterial();
            impact.hitFlash = FlashTime;
            impact.hitFreeze = RangedFreeze;
            impact.hitPush = p.HitPush;
            if (impact.effect == null) Debug.LogWarning($"pixelfx: {p.Impact} 프리팹 없음");

            var move = root.GetComponent<ProjectileBase>();
            if (move)
            {
                var so = new SerializedObject(move);
                so.FindProperty("_speed").floatValue = p.Speed;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
}
