using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BattleK.Scripts.AI;
using BattleK.Scripts.CharacterCreator;
using BattleK.Scripts.Data;
using BattleK.Scripts.Data.ClassInfo;
using BattleK.Scripts.Data.Type;
using BattleK.Scripts.HP;
using BattleK.Scripts.Manager.Battle;
using Pathfinding;
using Pathfinding.RVO;
using Skill;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BattleK.Scripts.Editor
{
    public static class FamilyUnitPrefabGenerator
    {
        private const string FamilyRoot = "Assets/BattleK/Family";
        private const string DefinitionRoot = "Assets/BattleK/SO/UnitCreate";
        private const string PoolRoot = "Assets/Scripts/Skill/SkillPool";
        private const string ReportPath = "Logs/FamilyUnitPrefabReport.tsv";

        [Serializable]
        private sealed class Roster
        {
            public string Family_ID;
            public Character[] Characters;
        }

        [Serializable]
        private sealed class Character
        {
            public string Unit_ID;
            public string Unit_Name;
            public int Class;
        }

        private sealed class Input
        {
            public FamilyName Family;
            public Character Character;
            public UnitClass Class;
            public Sprite Portrait;
            public GameObject Visual;
            public string Path;
            public string Guid;
        }

        // The roster JSON uses legacy class IDs; they are not current enum values.
        private static UnitClass MapRosterClass(int id) => id switch
        {
            1 => UnitClass.Gladiator,
            2 => UnitClass.Skirmisher,
            3 => UnitClass.Magician,
            6 => UnitClass.Assasin,
            7 => UnitClass.Priest,
            8 => UnitClass.Legionary,
            _ => throw new InvalidOperationException($"Unknown roster class: {id}")
        };

        private static List<Input> ReadInputs()
        {
            var inputs = new List<Input>();
            foreach (FamilyName family in Enum.GetValues(typeof(FamilyName)))
            {
                var folder = $"{FamilyRoot}/{family}";
                var roster = JsonUtility.FromJson<Roster>(File.ReadAllText($"{folder}/{family}.json"));
                Require(roster != null && roster.Family_ID == family.ToString() && roster.Characters?.Length == 10,
                    $"Invalid roster: {family}");
                foreach (var character in roster.Characters)
                {
                    Require(character.Unit_ID.StartsWith($"{family}_", StringComparison.Ordinal), "Invalid character ID");
                    var path = $"{folder}/UnitRoot_Prefabs/{character.Unit_ID}.prefab";
                    var visual = AssetDatabase.LoadAssetAtPath<GameObject>($"{folder}/Character_Prefabs/{character.Unit_ID}.prefab");
                    var portrait = AssetDatabase.LoadAssetAtPath<Sprite>($"{folder}/Images/{character.Unit_ID}.png");
                    Require(visual && visual.GetComponent<SPUM_Prefabs>() && portrait, $"Missing input: {character.Unit_ID}");
                    Require(AssetDatabase.LoadAssetAtPath<GameObject>(path), $"Missing existing prefab: {path}");
                    inputs.Add(new Input
                    {
                        Family = family, Character = character, Class = MapRosterClass(character.Class),
                        Visual = visual, Portrait = portrait, Path = path, Guid = AssetDatabase.AssetPathToGUID(path)
                    });
                }
            }
            Require(inputs.Count == 100 && inputs.Select(i => i.Character.Unit_ID).Distinct().Count() == 100,
                "Expected 100 unique units");
            return inputs;
        }

        private static void ConfigureDefinitions(ClassDefinitionDatabase database)
        {
            var configs = new[]
            {
                (UnitClass.Legionary, AttackType.Shieldman, "LegionarySkillPool"),
                (UnitClass.Skirmisher, AttackType.Archer, "SkimisherSkillPool"),
                (UnitClass.Magician, AttackType.Mage, "MagicianSkillPool"),
                (UnitClass.Assasin, AttackType.Thief, "AssasinSkillPool"),
                (UnitClass.Priest, AttackType.Priest, "PriestSkillPool"),
                (UnitClass.Gladiator, AttackType.Swordsman, "GladiatorSkillPool")
            };
            foreach (var (unitClass, attackType, poolName) in configs)
            {
                var definition = database.GetDefinition(unitClass);
                var pool = AssetDatabase.LoadAssetAtPath<ClassSkillPoolSO>($"{PoolRoot}/{poolName}.asset");
                Require(definition && pool, $"Missing class settings: {unitClass}");
                definition.UnitClass = unitClass;
                definition.AttackType = attackType;
                definition.IsRangedDefault = unitClass is UnitClass.Skirmisher or UnitClass.Magician or UnitClass.Priest;
                definition.AttackRange = definition.IsRangedDefault ? 5f : 0.9f;
                definition.MoveSpeed = 2f;
                definition.SightRange = 9f;
                definition.CommonSkillPool = pool;
                pool.unitClass = unitClass;
                EditorUtility.SetDirty(definition);
                EditorUtility.SetDirty(pool);
            }
        }

        // Uses the same creation routine as the single-unit window, without normal attacks.
        public static void GenerateAllFromCommandLine()
        {
            try
            {
                var inputs = ReadInputs();
                var database = AssetDatabase.LoadAssetAtPath<ClassDefinitionDatabase>($"{DefinitionRoot}/TestClassDefDB.asset");
                var settings = AddressableAssetSettingsDefaultObject.Settings;
                Require(database && database.hpBar && database.hpBar.GetComponent<RectTransform>()
                    && database.hpBar.GetComponentInChildren<HPBar>(true), "Missing HPBar settings");
                Require(settings, "Missing Addressables settings");
                foreach (var input in inputs)
                    Require(settings.FindAssetEntry(input.Guid) != null, $"Missing Addressables entry: {input.Path}");

                ConfigureDefinitions(database);
                foreach (var input in inputs)
                {
                    GameObject unit = null;
                    try
                    {
                        var recruit = input.Character.Unit_ID.Contains("_Recruit_");
                        var prefix = recruit ? $"{input.Family}_Recruit_" : $"{input.Family}_";
                        unit = UnitCreator.CreateUnit(input.Family, input.Character.Unit_ID.Substring(prefix.Length),
                            false, recruit, database.GetDefinition(input.Class), input.Portrait, input.Visual,
                            database.hpBar, includeNormalAttack: false);
                        Require(unit, $"Creation failed: {input.Path}");
                        unit.GetComponent<StaticAICore>().runtimeStat.Name = input.Character.Unit_Name;
                        ValidateUnit(unit, input, database);
                        PrefabUtility.SaveAsPrefabAsset(unit, input.Path, out var saved);
                        Require(saved && AssetDatabase.AssetPathToGUID(input.Path) == input.Guid,
                            $"Save failed or GUID changed: {input.Path}");
                        var entry = settings.FindAssetEntry(input.Guid);
                        entry.address = input.Character.Unit_ID;
                    }
                    finally
                    {
                        if (unit) Object.DestroyImmediate(unit);
                    }
                }
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
                ValidateAll();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                if (Application.isBatchMode) EditorApplication.Exit(1);
                else throw;
            }
        }

        [MenuItem("Tools/Colossame/Validate Unit Prefabs")]
        public static void ValidateAll()
        {
            var inputs = ReadInputs();
            var database = AssetDatabase.LoadAssetAtPath<ClassDefinitionDatabase>($"{DefinitionRoot}/TestClassDefDB.asset");
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            Require(database && settings, "Missing project settings");
            var report = new StringBuilder("Unit\tClass\tNormalAttack\tResult\tPath\n");
            foreach (var input in inputs)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(input.Path);
                ValidateUnit(prefab, input, database);
                Require(settings.FindAssetEntry(input.Guid)?.address == input.Character.Unit_ID,
                    $"Incorrect address: {input.Path}");
                var attack = prefab.GetComponent<StaticAICore>().NormalAttack;
                report.AppendLine($"{input.Character.Unit_ID}\t{input.Class}\t{(attack ? attack.name : "None")}\tPASS\t{input.Path}");
            }
            Directory.CreateDirectory("Logs");
            File.WriteAllText(ReportPath, report.ToString(), Encoding.UTF8);
            Debug.Log($"[FamilyUnitPrefabGenerator] PASS: {inputs.Count}/100 unit prefabs. Report: {ReportPath}");
            if (!Application.isBatchMode)
                EditorUtility.DisplayDialog("유닛 프리팹 확인", "100개 유닛의 직업, 이미지, 외형, HPBar, ID, 일반 공격 및 Addressables 연결 확인 완료.", "확인");
        }

        private static void ValidateUnit(GameObject unit, Input input, ClassDefinitionDatabase database)
        {
            var id = input.Character.Unit_ID;
            Require(unit && unit.name == id, $"Incorrect name: {id}");
            Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(unit) == 0, $"Missing root script: {id}");
            foreach (var child in unit.GetComponentsInChildren<Transform>(true))
                Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject) == 0,
                    $"Missing child script: {id}/{child.name}");
            var core = unit.GetComponent<StaticAICore>();
            var definition = database.GetDefinition(input.Class);
            Require(core && core.runtimeStat != null && core.runtimeStat.UnitClass == input.Class, $"Incorrect class: {id}");
            Require(core.runtimeStat.Name == input.Character.Unit_Name && core.runtimeStat.CharacterImage == input.Portrait,
                $"Incorrect portrait/name: {id}");
            Require(core.NormalAttack == definition.NormalAttackData && core.runtimeStat.EquippedSkills.Count == 0,
                $"Incorrect attack/equipped skills: {id}");
            Require(core.runtimeStat.SkillPoolSo == definition.CommonSkillPool && definition.CommonSkillPool
                && definition.CommonSkillPool.unitClass == input.Class, $"Incorrect skill pool: {id}");
            Require(core.AttackIndex == definition.AttackAnimationIndex && core.runtimeStat.IsRanged == definition.IsRangedDefault
                && Mathf.Approximately(core.runtimeStat.AttackRange, definition.AttackRange), $"Incorrect class defaults: {id}");
            Require(core.HPBar && core.HPBar.OwnerAi == core && core.HPBar.transform.IsChildOf(unit.transform), $"Incorrect HPBar: {id}");
            Require(core.player && core.player._prefabs && core.player._prefabs.transform.IsChildOf(unit.transform), $"Missing visual: {id}");
            Require(core.player._prefabs._anim && core.player._prefabs._anim.runtimeAnimatorController,
                $"Missing animator: {id}");
            Require(core.player._prefabs.GetComponentsInChildren<SpriteRenderer>(true).Any(renderer => renderer.sprite),
                $"Missing visual sprites: {id}");
            Require(PrefabUtility.GetCorrespondingObjectFromSource(core.player._prefabs.gameObject) == input.Visual,
                $"Incorrect visual source: {id}");
            Require(unit.GetComponent<CharacterID>()?.characterKey == id
                && unit.GetComponent<FamilyID>()?.FamilyKey == input.Family.ToString(), $"Incorrect ID: {id}");
            Require(core.AiPath == unit.GetComponent<AIPath>() && core.AiPath
                && core.Rigidbody == unit.GetComponent<Rigidbody2D>() && core.Rigidbody
                && unit.GetComponent<CircleCollider2D>() && unit.GetComponent<RVOController>()
                && unit.GetComponent<AIDestinationSetter>() && unit.GetComponent<StatusEffectManager>()?._aiCore == core,
                $"Missing core components: {id}");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
