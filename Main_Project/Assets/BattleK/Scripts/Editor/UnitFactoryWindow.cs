// ==========================
// File: Assets/Editor/UnitFactoryWindow.cs
// ==========================
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Pathfinding;           // A* Pathfinding Project
using Pathfinding.RVO;      // RVOController
using System;

public class UnitFactoryWindow : EditorWindow
{
    public enum AttackType { Melee, Ranged }

    // --- 입력 필드 ---
    private string unitFullName = "Astra_Freyja"; // "가문_이름" 형식
    private AttackType attackType = AttackType.Ranged;

    // 프로젝트의 유닛클래스 enum을 연결한다. 없으면 문자열로 입력받도록 fallback.
    private UnitClass unitClassName = UnitClass.Archer;

    private GameObject spumPrefab;            // SPUM 유형 프리팹
    private GameObject rangedAttackPrefab;    // 원거리 공격 오브젝트 프리팹 (예: RangedAttackObject)
    private GameObject meleeAttackPrefab;     // 근거리 공격 오브젝트 프리팹 (예: MeleeAttack)

    [MenuItem("Tools/Colossam/Unit Factory")]
    public static void Open()
    {
        var win = GetWindow<UnitFactoryWindow>("Unit Factory");
        win.minSize = new Vector2(420, 420);
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Create Unit From Script", EditorStyles.boldLabel);

        unitFullName = EditorGUILayout.TextField(new GUIContent("Full Name (Family_Character)"), unitFullName);
        attackType = (AttackType)EditorGUILayout.EnumPopup(new GUIContent("Attack Type"), attackType);
        unitClassName = (UnitClass)EditorGUILayout.EnumPopup(new GUIContent("Unit Class Name"), unitClassName);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Prefabs", EditorStyles.boldLabel);
        spumPrefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent("SPUM Prefab"), spumPrefab, typeof(GameObject), false);
        rangedAttackPrefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent("RangedAttack Prefab"), rangedAttackPrefab, typeof(GameObject), false);
        meleeAttackPrefab  = (GameObject)EditorGUILayout.ObjectField(new GUIContent("MeleeAttack Prefab"), meleeAttackPrefab, typeof(GameObject), false);

        EditorGUILayout.Space(10);
        if (GUILayout.Button("Create Unit"))
        {
            if (ValidateInputs())
            {
                CreateUnitEditor();
            }
        }
    }

    private bool ValidateInputs()
    {
        if (string.IsNullOrWhiteSpace(unitFullName) || !unitFullName.Contains("_"))
        {
            EditorUtility.DisplayDialog("입력 오류", "Full Name은 반드시 'Family_Character' 형식이어야 합니다.", "확인");
            return false;
        }
        if (spumPrefab == null)
        {
            EditorUtility.DisplayDialog("입력 오류", "SPUM Prefab을 지정하세요.", "확인");
            return false;
        }
        if (attackType == AttackType.Ranged && rangedAttackPrefab == null)
        {
            EditorUtility.DisplayDialog("입력 오류", "RangedAttack Prefab을 지정하세요.", "확인");
            return false;
        }
        if (attackType == AttackType.Melee && meleeAttackPrefab == null)
        {
            EditorUtility.DisplayDialog("입력 오류", "MeleeAttack Prefab을 지정하세요.", "확인");
            return false;
        }
        return true;
    }

    private void CreateUnitEditor()
    {
        try
        {
            // 에디터 Undo를 지원하는 생성 API 호출
            var created = UnitFactory.CreateUnitInEditor(
                unitFullName: unitFullName,
                isRanged: attackType == AttackType.Ranged,
                unitClassName: unitClassName,
                spumPrefab: spumPrefab,
                rangedAttackPrefab: rangedAttackPrefab,
                meleeAttackPrefab: meleeAttackPrefab
            );

            if (created != null)
            {
                Selection.activeGameObject = created;
                EditorGUIUtility.PingObject(created);
                Debug.Log($"[UnitFactory] Created '{created.name}' successfully.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[UnitFactory] Create failed: {ex}");
        }
    }
}
#endif
