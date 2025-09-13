// ============================================
// File: Assets/BattleK/Scripts/EditorHelpers/UnitFactory.cs
//  - 런타임/에디터 공용 유닛 생성 로직
// ============================================
using UnityEngine;
using System.Reflection;
using Pathfinding;           // A* Pathfinding Project
using Pathfinding.RVO;      // RVOController

public static class UnitFactory
{
    /// <summary>
    /// 에디터에서 Undo 지원과 PrefabUtility 연결을 고려하여 생성.
    /// </summary>
#if UNITY_EDITOR
    public static GameObject CreateUnitInEditor(
        string unitFullName,
        bool isRanged,
        UnitClass unitClassName,
        GameObject spumPrefab,
        GameObject rangedAttackPrefab,
        GameObject meleeAttackPrefab
    )
    {
        // 1) 부모 빈 오브젝트 생성 (Undo 지원)
        var parent = new GameObject(unitFullName);
        parent.transform.localScale = new Vector3(0.7f, 0.7f, 1);
        UnityEditor.Undo.RegisterCreatedObjectUndo(parent, "Create Unit Root");

        // 2) 필수 컴포넌트들 추가
        AddCoreComponents(parent);

        // 3) AICore 설정·보조 컴포넌트 설정
        ConfigureCore(parent, isRanged, unitClassName);

        // 4) 원형 충돌기 기본값
        var col = parent.GetComponent<CircleCollider2D>();
        col.radius = 0.5f;

        // 5) SPUM 프리팹 자식으로 부착
        GameObject visual = null;
        if (spumPrefab != null)
        {
            visual = InstantiatePrefabEditor(spumPrefab, parent.transform, "Visual");
            SetVisualOffset(visual); // (0, -0.3, 0)
        }

        // 6) 공격 프리팹 자식 부착
        if (isRanged && rangedAttackPrefab != null)
        {
            InstantiatePrefabEditor(rangedAttackPrefab, parent.transform, "RangedAttackObject");
        }
        else if (!isRanged && meleeAttackPrefab != null)
        {
            InstantiatePrefabEditor(meleeAttackPrefab, parent.transform, "MeleeAttack");
        }

        // 7) CharacterID / FamilyID 채우기
        ApplyFamilyAndCharacterIDs(parent, unitFullName);

        return parent;
    }

    private static GameObject InstantiatePrefabEditor(GameObject prefab, Transform parent, string suggestedName)
    {
        GameObject inst = null;
        if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(prefab))
        {
            inst = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
            UnityEditor.Undo.RegisterCreatedObjectUndo(inst, "Instantiate Prefab Child");
            inst.transform.SetParent(parent, false);
        }
        else
        {
            inst = UnityEngine.Object.Instantiate(prefab, parent, false);
            inst.name = prefab.name;
            UnityEditor.Undo.RegisterCreatedObjectUndo(inst, "Instantiate Child");
        }

        if (!string.IsNullOrEmpty(suggestedName))
            inst.name = suggestedName;

        return inst;
    }
#endif

    /// <summary>
    /// 런타임에서도 동일하게 생성할 수 있는 API.
    /// (Undo/Prefab connection 없이 일반 Instantiate)
    /// </summary>
    public static GameObject CreateUnitAtRuntime(
        string unitFullName,
        bool isRanged,
        UnitClass unitClassName,
        GameObject spumPrefab,
        GameObject rangedAttackPrefab,
        GameObject meleeAttackPrefab
    )
    {
        // 1) 부모 빈 오브젝트
        var parent = new GameObject(unitFullName);

        // 2) 필수 컴포넌트
        AddCoreComponents(parent);

        // 3) AICore 설정
        ConfigureCore(parent, isRanged, unitClassName);

        // 4) 원형 충돌기 반지름
        var col = parent.GetComponent<CircleCollider2D>();
        col.radius = 0.5f;

        // 5) SPUM 프리팹
        if (spumPrefab != null)
        {
            var visual = Object.Instantiate(spumPrefab, parent.transform, false);
            visual.name = "Visual";
            SetVisualOffset(visual);
        }

        // 6) 공격 프리팹
        if (isRanged && rangedAttackPrefab != null)
        {
            var ra = Object.Instantiate(rangedAttackPrefab, parent.transform, false);
            ra.name = "RangedAttackObject";
        }
        else if (!isRanged && meleeAttackPrefab != null)
        {
            var ma = Object.Instantiate(meleeAttackPrefab, parent.transform, false);
            ma.name = "MeleeAttack";
        }

        // 7) ID 세팅
        ApplyFamilyAndCharacterIDs(parent, unitFullName);

        return parent;
    }

    // ---- 공통 유틸 ----

    private static void AddCoreComponents(GameObject go)
    {
        // 중복 추가 방지 + 필요한 것 전부 붙이기
        EnsureComponent<AICore>(go);
        EnsureComponent<CircleCollider2D>(go);
        EnsureComponent<AIDestinationSetter>(go);
        EnsureComponent<AIPath>(go);
        EnsureComponent<RVOController>(go);
        EnsureComponent<SkillCooldownManager>(go);
        EnsureComponent<SkillUse>(go);
        EnsureComponent<PlayerObjC>(go);
        EnsureComponent<CharacterID>(go);
        EnsureComponent<FamilyID>(go);
    }

    private static void ConfigureCore(GameObject go, bool isRanged, UnitClass unitClassName)
    {
        var ai = go.GetComponent<AICore>();

        // (1) 공격 유형
        SetBoolIfExists(ai, "isRanged", isRanged);

        // (2) unitClass(enum) 주입: 필드 우선, 없으면 프로퍼티
        var t = ai.GetType();
        var f = t.GetField("unitClass", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (f != null && f.FieldType == typeof(UnitClass))
        {
            f.SetValue(ai, unitClassName);
        }
        else
        {
            var p = t.GetProperty("unitClass", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (p != null && p.PropertyType == typeof(UnitClass) && p.CanWrite)
            {
                p.SetValue(ai, unitClassName, null);
            }
        }
        if(ai.unitClass is UnitClass.Archer or UnitClass.Mage)  ai.isRanged = true;
        ai.attackRange = 0.9f;
        if (isRanged) ai.attackRange = 5f;
        ai.moveSpeed = 2;
        ai.sightRange = 9f;

        // (3) 이동 컴포넌트 기본값(안전한 최소 세팅)
        var aiPath = go.GetComponent<AIPath>();
        aiPath.maxSpeed = 3.5f;
        aiPath.canMove = true;
        aiPath.orientation = OrientationMode.YAxisForward;
        aiPath.enableRotation = false;
        aiPath.gravity = new Vector3(0, 0, 0);
        // 나머지 세부 옵션(orientation, rotation 등)은 프로젝트 규칙에 맞춰 Inspector에서 조정 권장

        // (4) RVO 기본값 (AIPath와 속도 일치)
        var rvo = go.GetComponent<RVOController>();
        rvo.radius = 0.3f;
    }

    private static void SetVisualOffset(GameObject visual)
    {
        // RectTransform이면 anchoredPosition, 아니면 Transform localPosition
        var rt = visual.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchoredPosition3D = new Vector3(0f, -0.3f, 0f);
        }
        else
        {
            visual.transform.localPosition = new Vector3(0f, -0.3f, 0f);
        }
    }

    private static void ApplyFamilyAndCharacterIDs(GameObject parent, string fullName)
    {
        // 규칙:
        //  - FamilyID: 첫 번째 '_' 앞부분 (없으면 전체)
        //  - CharacterID: 전체 fullName 그대로
        int idx = fullName.IndexOf('_');
        string family = (idx >= 0) ? fullName.Substring(0, idx) : fullName;
        string character = fullName;

        var fam = parent.GetComponent<FamilyID>();
        var chr = parent.GetComponent<CharacterID>();

        // FamilyID 후보 필드/프로퍼티에 시도
        SetStringIfExists(fam, "Family", family);
        SetStringIfExists(fam, "family", family);
        SetStringIfExists(fam, "familyId", family);
        SetStringIfExists(fam, "FamilyID", family);
        SetStringIfExists(fam, "FamilyKey", family);
        SetStringIfExists(fam, "familyKey", family);

        // CharacterID 후보 필드/프로퍼티에 시도
        SetStringIfExists(chr, "Character", character);
        SetStringIfExists(chr, "character", character);
        SetStringIfExists(chr, "characterId", character);
        SetStringIfExists(chr, "CharacterID", character);
        SetStringIfExists(chr, "CharacterKey", character);
        SetStringIfExists(chr, "characterKey", character);
    }

    // 컴포넌트가 없으면 붙이고 반환
    private static T EnsureComponent<T>(GameObject go) where T : Component
    {
        var c = go.GetComponent<T>();
        if (c == null) c = go.AddComponent<T>();
        return c;
    }

    // --- 리플렉션 유틸: 프로젝트 필드명 차이를 흡수 ---
    private static void SetStringIfExists(object obj, string fieldOrProperty, string value)
    {
        if (obj == null) return;
        var t = obj.GetType();

        var f = t.GetField(fieldOrProperty, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (f != null && f.FieldType == typeof(string))
        {
            f.SetValue(obj, value);
            return;
        }

        var p = t.GetProperty(fieldOrProperty, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (p != null && p.PropertyType == typeof(string) && p.CanWrite)
        {
            p.SetValue(obj, value, null);
        }
    }

    private static void SetBoolIfExists(object obj, string fieldOrProperty, bool value)
    {
        if (obj == null) return;
        var t = obj.GetType();

        var f = t.GetField(fieldOrProperty, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (f != null && f.FieldType == typeof(bool))
        {
            f.SetValue(obj, value);
            return;
        }

        var p = t.GetProperty(fieldOrProperty, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (p != null && p.PropertyType == typeof(bool) && p.CanWrite)
        {
            p.SetValue(obj, value, null);
        }
    }
}
