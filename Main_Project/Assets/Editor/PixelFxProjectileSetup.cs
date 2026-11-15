using BattleK.Scripts.AI.Attack;
using BattleK.Scripts.AI.Skill.Base;
using UnityEditor;
using UnityEngine;

// 마법사, 척후병 평타 투사체 그림을 pixelfx 이펙트로 교체
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

    // 값은 월드 기준 (유닛 루트 스케일 0.7, 키 약 0.5: 몸통 0.2~0.25, 머리 0.45)
    private class Projectile
    {
        public string Prefab, Skill, Fx;
        public float Scale, Speed;
        public float Windup;  // 공격 모션에서 실제로 쏘는 순간 (초)
        public float Height;  // 발밑에서 그림 높이
        public float Forward; // 몸 앞으로 당기는 거리
        public string Impact; // 타격 이펙트
        public float ImpactScale, ImpactLifetime, ImpactFade;
        public float ImpactDepth = 0.4f; // 닿은 지점에서 몸 안쪽으로
        public float ImpactHeight;       // 이펙트 기준점 높이 (그림이 기준점보다 위에 있으면 낮게)
    }

    private static readonly Projectile Fireball = new()
    {
        Prefab = "Magician_NormalAttack", Skill = "Magician_NormalAttack", Fx = "fireball",
        Scale = 0.6f, Speed = 6f,
        Windup = 0.22f, // 0_Attack_Magic 0.42초 중 손 내미는 순간
        Height = 0.45f, Forward = 0.3f,
        Impact = "small-explosion", ImpactScale = 0.25f, ImpactLifetime = 0.9f, ImpactFade = 0.3f, // 뒤쪽 연기는 잘라냄
        ImpactHeight = 0f, // 발밑에서 위로 퍼지며 몸을 덮음
    };

    private static readonly Projectile Arrow = new()
    {
        Prefab = "Skirmisher_NormalAttack", Skill = "Skirmisher_NormalAttack", Fx = "arrow",
        Scale = 0.7f, Speed = 7f,
        Windup = 0.35f, // 0_Attack_Bow 0.83초 중 시위 놓는 순간
        Height = 0.2f, Forward = 0.4f, // 몸통 높이
        Impact = "hit-spark", ImpactScale = 0.35f,
        ImpactHeight = 0f, // 불꽃이 기준점보다 약 0.24 위 = 몸통
    };

    [MenuItem("Tools/pixelfx/평타 투사체 교체 (마법사, 척후병)")]
    public static void Apply()
    {
        if (AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>($"{FxDir}/Animations/fireball.controller") == null)
            PixelFxImporter.ImportAll();

        // 마법사: 사제와 같이 쓰던 Bullet_NormalAttack을 복사해서 분리
        var magePath = $"{AttackDir}/{Fireball.Prefab}.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(magePath) == null)
            AssetDatabase.CopyAsset($"{AttackDir}/Bullet_NormalAttack.prefab", magePath);

        SetVisual(Fireball);
        SetSkill(Fireball, AssetDatabase.LoadAssetAtPath<GameObject>(magePath));

        // 척후병: 기존 프리팹에 바로 적용 (척후병만 사용)
        SetVisual(Arrow);
        SetSkill(Arrow, null);

        AssetDatabase.SaveAssets();
        Debug.Log("pixelfx: 마법사 fireball, 척후병 arrow 적용");
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
            if (root.TryGetComponent<BoxCollider2D>(out var box)) { box.size = new Vector2(hit, hit); box.offset = new Vector2(forward, 0f); }
            if (root.TryGetComponent<CircleCollider2D>(out var circle)) { circle.radius = hit * 0.5f; circle.offset = new Vector2(forward, 0f); }

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
