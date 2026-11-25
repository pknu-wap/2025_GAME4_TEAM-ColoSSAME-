using System.Collections.Generic;
using System.IO;
using System.Linq;
using BattleK.Scripts.AI;
using BattleK.Scripts.AI.Skill.Base;
using BattleK.Scripts.AI.Skill.SkillExample;
using BattleK.Scripts.Data.ClassInfo;
using BattleK.Scripts.Manager;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BattleK.Test.EditorTools
{
    // testbattle 씬(매니저, 직업별 유닛 구성)을 복사해서 전투 테스트 씬을 만듦
    public static class CombatTestSceneBuilder
    {
        private const string Template = "Assets/BattleK/Scenes/testbattle.unity";
        private const string ScenePath = "Assets/BattleK/Test/CombatTest.unity";
        private const string SkillRoot = "Assets/BattleK/SO/Skills";
        private const string ExampleFolder = "0_Example";
        private const int EnemyCount = 5;

        private static readonly Dictionary<UnitClass, string> SkillFolders = new()
        {
            { UnitClass.Magician, "1_Magician" },
            { UnitClass.Gladiator, "2_Gladiator" },
            { UnitClass.Legionary, "3_Legionary" },
            { UnitClass.Skirmisher, "4_Skimisher" },
            { UnitClass.Assasin, "5_Assasin" },
            { UnitClass.Priest, "6_Priest" },
        };

        [MenuItem("Tools/전투 테스트/씬 열기")]
        public static void Open()
        {
            if (!File.Exists(ScenePath)) { Build(); return; }
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(ScenePath);
        }

        [MenuItem("Tools/전투 테스트/씬 다시 만들기")]
        public static void Build()
        {
            if (File.Exists(ScenePath) &&
                !EditorUtility.DisplayDialog("전투 테스트", "기존 CombatTest 씬을 지우고 새로 만들까요?", "새로 만들기", "취소"))
                return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            if (File.Exists(ScenePath)) AssetDatabase.DeleteAsset(ScenePath);
            if (!AssetDatabase.CopyAsset(Template, ScenePath))
            {
                Debug.LogError($"[전투 테스트] {Template} 복사 실패");
                return;
            }

            var scene = EditorSceneManager.OpenScene(ScenePath);
            var showcase = Object.FindObjectOfType<NormalAttackShowcase>(true);
            if (!showcase)
            {
                Debug.LogError("[전투 테스트] testbattle에서 NormalAttackShowcase를 찾지 못함");
                return;
            }

            // 쇼케이스에 연결된 직업별 유닛과 평타 SO를 그대로 가져옴
            var so = new SerializedObject(showcase);
            var manager = so.FindProperty("_battleManager").objectReferenceValue as AI_Manager;
            var actorsRoot = so.FindProperty("_actorsRoot").objectReferenceValue as GameObject;
            var target = so.FindProperty("_target").objectReferenceValue as StaticAICore;
            var entries = so.FindProperty("_normalAttacks");

            var casters = new List<CombatTestBench.Caster>();
            for (var i = 0; i < entries.arraySize; i++)
            {
                var e = entries.GetArrayElementAtIndex(i);
                var unitClass = (UnitClass)e.FindPropertyRelative("UnitClass").intValue;
                casters.Add(new CombatTestBench.Caster
                {
                    UnitClass = unitClass,
                    Actor = e.FindPropertyRelative("Attacker").objectReferenceValue as StaticAICore,
                    NormalAttack = e.FindPropertyRelative("NormalAttackSO").objectReferenceValue as SkillSO,
                    Skills = SkillFolders.TryGetValue(unitClass, out var folder) ? LoadSkills(folder) : new List<SkillSO>(),
                });
            }

            // 허수아비: 쇼케이스 대상을 복사해서 적 5명, 아군 1명
            var enemies = new List<StaticAICore>();
            if (target)
            {
                target.name = "적 1";
                enemies.Add(target);
                for (var i = 2; i <= EnemyCount; i++) enemies.Add(Duplicate(target, $"적 {i}"));
            }
            var ally = target ? Duplicate(target, "아군") : null;

            var examples = LoadSkills(ExampleFolder).Where(s => s.name.StartsWith("Example")).ToList();

            // 쇼케이스와 시작 버튼 UI는 제거
            var canvas = FindByName(scene, "TestBattleCanvas");
            if (canvas) Object.DestroyImmediate(canvas);
            Object.DestroyImmediate(showcase.gameObject);

            var bench = new GameObject("CombatTestBench").AddComponent<CombatTestBench>();
            bench.Setup(manager, actorsRoot, casters, enemies, ally, examples);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[전투 테스트] {ScenePath} 생성: 시전자 {casters.Count}명, 예제 스킬 {examples.Count}개. 플레이하면 왼쪽 위 패널로 조작");
        }

        private static StaticAICore Duplicate(StaticAICore source, string name)
        {
            var copy = Object.Instantiate(source.gameObject, source.transform.parent);
            copy.name = name;
            return copy.GetComponent<StaticAICore>();
        }

        private static List<SkillSO> LoadSkills(string folder)
        {
            var path = $"{SkillRoot}/{folder}";
            if (!AssetDatabase.IsValidFolder(path)) return new List<SkillSO>();
            return AssetDatabase.FindAssets("t:SkillSO", new[] { path })
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(p => p)
                .Select(AssetDatabase.LoadAssetAtPath<SkillSO>)
                .Where(s => s)
                .ToList();
        }

        private static GameObject FindByName(UnityEngine.SceneManagement.Scene scene, string name)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == name) return root;
                var t = root.GetComponentsInChildren<Transform>(true).FirstOrDefault(x => x.name == name);
                if (t) return t.gameObject;
            }
            return null;
        }
    }
}
