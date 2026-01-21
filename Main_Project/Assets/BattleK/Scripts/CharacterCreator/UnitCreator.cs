using System.Collections.Generic;
using BattleK.Scripts.AI;
using BattleK.Scripts.AI.SO.Base;
using BattleK.Scripts.AI.StaticScoreState.Attack;
using BattleK.Scripts.Data;
using BattleK.Scripts.Data.ClassInfo;
using BattleK.Scripts.Data.Type;
using BattleK.Scripts.HP;
using Pathfinding;
using Pathfinding.RVO;
using UnityEngine;

namespace BattleK.Scripts.CharacterCreator
{
    public static class UnitCreator
    {
        public static GameObject CreateUnit(
            FamilyName familyName,
            string characterName,
            bool isUsingSPUMName,
            bool isRecruit,
            bool isRanged,
            UnitClass unitClassName,
            Sprite unitImage,
            GameObject spumPrefab,
            GameObject rangedPrefab,
            GameObject meleePrefab,
            GameObject hpBarPrefab,
            List<SkillSO> skillPrefabs)
        {
            var unitFullName = isRecruit ? $"{familyName}_Recruit_{characterName}": $"{familyName}_{characterName}";
            var parent = new GameObject(unitFullName)
            {
                transform = { localScale = new Vector3(0.7f, 0.7f, 1f) }
            };
            UnityEditor.Undo.RegisterCreatedObjectUndo(parent.gameObject, unitFullName);

            AddCoreComponents(parent);
            
            var visual = InstantiatePrefab(spumPrefab, parent.transform, "Visual");
            var rectTransform = visual.GetComponent<RectTransform>();
            if(rectTransform) rectTransform.anchoredPosition3D = new Vector3(0, -0.3f, 0);
            else visual.transform.localPosition = new Vector3(0, -0.3f, 0);
            
            var weapon = isRanged ? InstantiatePrefab(rangedPrefab, parent.transform, "Ranged") : InstantiatePrefab(meleePrefab, parent.transform, "Melee"); 
            weapon.transform.localPosition = new Vector3(-0.5f, 0, 0);
            
            var hpBar = InstantiatePrefab(hpBarPrefab, parent.transform, "HP Bar");
            
            ConfigureCore(parent, isRanged, unitClassName, visual, hpBar, unitImage, skillPrefabs);
            if (isUsingSPUMName)
            {
                unitFullName = spumPrefab.gameObject.name;
                parent.name = unitFullName;
            }
            
            ApplyFamilyAndCharacterIDs(parent, familyName, unitFullName);
            return parent;
        }

        private static void AddCoreComponents(GameObject parent)
        {
            parent.AddComponent<StaticAICore>();
            parent.AddComponent<Rigidbody2D>();
            parent.AddComponent<PlayerObjC>();
            parent.AddComponent<CharacterID>();
            parent.AddComponent<FamilyID>();
            parent.AddComponent<CircleCollider2D>();
            parent.AddComponent<AIDestinationSetter>();
            parent.AddComponent<AIPath>();
            parent.AddComponent<RVOController>();
        }
        
        private static void ConfigureCore(GameObject parent, bool isRanged, UnitClass unitClassName, GameObject spumInstance, GameObject hpBar, Sprite unitImage, List<SkillSO> skills)
        {
            var aiCore = parent.GetComponent<StaticAICore>();
            aiCore.Stat = new UnitStat
            {
                IsRanged = isRanged,
                UnitClass = unitClassName,
                CharacterImage = unitImage,
                AttackRange = isRanged ? 5f : 0.9f,
                MoveSpeed = 2f,
                SightRange = 9f,
                Skills = skills ?? new List<SkillSO>()
            };
            if(aiCore.Stat.UnitClass is UnitClass.Archer or UnitClass.Mage or UnitClass.Priest)  aiCore.Stat.IsRanged = true;
            aiCore.AttackIndex = unitClassName switch
            {
                UnitClass.Archer => 2,
                UnitClass.Mage or UnitClass.Priest => 4,
                _ => 0
            };

            var playerObj = parent.GetComponent<PlayerObjC>();
            playerObj._prefabs = spumInstance.GetComponent<SPUM_Prefabs>();
            
            var aiPath = parent.GetComponent<AIPath>();
            aiPath.maxSpeed = 3.5f;
            aiPath.canMove = true;
            aiPath.orientation = OrientationMode.YAxisForward;
            aiPath.enableRotation = false;
            aiPath.gravity = new Vector3(0, 0, 0);
            
            var rvo = aiCore.GetComponent<RVOController>();
            rvo.radius = 0.3f;
            
            var col = parent.GetComponent<CircleCollider2D>();
            col.radius = 0.5f;

            var rb = parent.GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            var hpBarComponent = hpBar.GetComponentInChildren<HPBar>();
            hpBar.GetComponent<RectTransform>().localPosition = new Vector3(0, -0.45f, 0);
            hpBarComponent.OwnerAi = aiCore;
            
            aiCore.AiPath = aiPath;
            aiCore.Rigidbody = rb;
            aiCore.player = playerObj;
            aiCore.HPBar = hpBarComponent;
            if (isRanged) aiCore.RangedWeapon = parent.GetComponentInChildren<StaticRangedAttack>();
            else aiCore.MeleeWeapon = parent.GetComponentInChildren<StaticMeleeAttack>();
        }

        private static GameObject InstantiatePrefab(GameObject prefab, Transform parent, string name)
        {
            var instance = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
            UnityEditor.Undo.RegisterCreatedObjectUndo(instance, "Instantiate Prefab Child");
            instance.transform.SetParent(parent, false);
            instance.name = name;
            return instance;
        }
        
        private static void ApplyFamilyAndCharacterIDs(GameObject parent, FamilyName familyName, string fullName)
        {
            var famComponent = parent.GetComponent<FamilyID>();
            var chrComponent = parent.GetComponent<CharacterID>();
            
            if (famComponent) famComponent.FamilyKey = familyName.ToString();
            if (chrComponent) chrComponent.characterKey = fullName;
        }
    }
}
