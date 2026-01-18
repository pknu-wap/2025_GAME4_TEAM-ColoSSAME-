using System.Reflection;
using BattleK.Scripts.AI;
using BattleK.Scripts.Data;
using Pathfinding;
using Pathfinding.RVO;
using UnityEngine;
// A* Pathfinding Project

// RVOController

namespace BattleK.Scripts.CharacterCreator
{
    public static class UnitFactory
    {
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
            var parent = new GameObject(unitFullName)
            {
                transform =
                {
                    localScale = new Vector3(0.7f, 0.7f, 1)
                }
            };
            UnityEditor.Undo.RegisterCreatedObjectUndo(parent, "Create Unit Root");

            AddCoreComponents(parent);

            ConfigureCore(parent, isRanged, unitClassName);

            // 4) 원형 충돌기 기본값
            var col = parent.GetComponent<CircleCollider2D>();
            col.radius = 0.5f;

            // 5) SPUM 프리팹 자식으로 부착
            if (spumPrefab)
            {
                var visual = InstantiatePrefabEditor(spumPrefab, parent.transform, "Visual");
                SetVisualOffset(visual); // (0, -0.3, 0)
            }

            switch (isRanged)
            {
                // 6) 공격 프리팹 자식 부착
                case true when rangedAttackPrefab:
                    InstantiatePrefabEditor(rangedAttackPrefab, parent.transform, "RangedAttackObject");
                    break;
                case false when meleeAttackPrefab:
                    InstantiatePrefabEditor(meleeAttackPrefab, parent.transform, "MeleeAttack");
                    break;
            }

            // 7) CharacterID / FamilyID 채우기
            ApplyFamilyAndCharacterIDs(parent, unitFullName);

            return parent;
        }

        private static GameObject InstantiatePrefabEditor(GameObject prefab, Transform parent, string suggestedName)
        {
            GameObject inst;
            if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(prefab))
            {
                inst = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
                UnityEditor.Undo.RegisterCreatedObjectUndo(inst, "Instantiate Prefab Child");
                inst.transform.SetParent(parent, false);
            }
            else
            {
                inst = Object.Instantiate(prefab, parent, false);
                inst.name = prefab.name;
                UnityEditor.Undo.RegisterCreatedObjectUndo(inst, "Instantiate Child");
            }

            if (!string.IsNullOrEmpty(suggestedName))
                inst.name = suggestedName;

            return inst;
        }
#endif
        
        private static void AddCoreComponents(GameObject go)
        {
            // 중복 추가 방지 + 필요한 것 전부 붙이기
            EnsureComponent<StaticAICore>(go);
            EnsureComponent<CircleCollider2D>(go);
            EnsureComponent<AIDestinationSetter>(go);
            EnsureComponent<AIPath>(go);
            EnsureComponent<RVOController>(go);
            EnsureComponent<PlayerObjC>(go);
            EnsureComponent<CharacterID>(go);
            EnsureComponent<FamilyID>(go);
        }

        private static void ConfigureCore(GameObject go, bool isRanged, UnitClass unitClassName)
        {
            var ai = go.GetComponent<StaticAICore>();

            SetBoolIfExists(ai, "isRanged", isRanged);

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
            if(ai.Stat.UnitClass is UnitClass.Archer or UnitClass.Mage)  ai.Stat.IsRanged = true;
            ai.Stat.AttackRange = 0.9f;
            if (isRanged) ai.Stat.AttackRange = 5f;
            ai.Stat.MoveSpeed = 2;
            ai.Stat.SightRange = 9f;

            var aiPath = go.GetComponent<AIPath>();
            aiPath.maxSpeed = 3.5f;
            aiPath.canMove = true;
            aiPath.orientation = OrientationMode.YAxisForward;
            aiPath.enableRotation = false;
            aiPath.gravity = new Vector3(0, 0, 0);

            // (4) RVO 기본값 (AIPath와 속도 일치)
            var rvo = go.GetComponent<RVOController>();
            rvo.radius = 0.3f;
        }

        private static void SetVisualOffset(GameObject visual)
        {
            var rt = visual.GetComponent<RectTransform>();
            if (rt) rt.anchoredPosition3D = new Vector3(0f, -0.3f, 0f);
            else visual.transform.localPosition = new Vector3(0f, -0.3f, 0f);
        }

        private static void ApplyFamilyAndCharacterIDs(GameObject parent, string fullName)
        {
            // 규칙:
            //  - FamilyID: 첫 번째 '_' 앞부분 (없으면 전체)
            //  - CharacterID: 전체 fullName 그대로
            var idx = fullName.IndexOf('_');
            var family = (idx >= 0) ? fullName.Substring(0, idx) : fullName;

            var fam = parent.GetComponent<FamilyID>();
            var chr = parent.GetComponent<CharacterID>();

            SetStringIfExists(fam, "Family", family);
            SetStringIfExists(fam, "family", family);
            SetStringIfExists(fam, "familyId", family);
            SetStringIfExists(fam, "FamilyID", family);
            SetStringIfExists(fam, "FamilyKey", family);
            SetStringIfExists(fam, "familyKey", family);

            // CharacterID 후보 필드/프로퍼티에 시도
            SetStringIfExists(chr, "Character", fullName);
            SetStringIfExists(chr, "character", fullName);
            SetStringIfExists(chr, "characterId", fullName);
            SetStringIfExists(chr, "CharacterID", fullName);
            SetStringIfExists(chr, "CharacterKey", fullName);
            SetStringIfExists(chr, "characterKey", fullName);
        }

        // 컴포넌트가 없으면 붙이고 반환
        private static void EnsureComponent<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            if (!c) c = go.AddComponent<T>();
        }

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
                p.SetValue(obj, value, null);
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
}
