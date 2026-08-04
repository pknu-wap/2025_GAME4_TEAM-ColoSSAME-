using System.Collections.Generic;
using System.Linq;
using BattleK.Scripts.AI.Skill.Base;
using BattleK.Scripts.CharacterCreator;
using BattleK.Scripts.Data;
using BattleK.Scripts.Data.ClassInfo;
using BattleK.Scripts.Data.Type;
using UnityEditor;
using UnityEngine;

namespace BattleK.Scripts.Editor
{
    public class UnitCreatorWindow : EditorWindow
    {
        private FamilyName _familyName = FamilyName.Astra;
        private bool _isUsingSpumName;
        private string _unitName = "New Unit";
        private bool _isRecruit;
        private bool _isRanged;
        private UnitClass _unitClass;
        private Sprite _unitImage;
        private GameObject spumPrefab;
        private GameObject _rangedAttack;
        private GameObject _meleeAttack;
        private GameObject _hpBar;
        private ClassSkillDatabase _skillDatabase;
        private List<SkillSO> _classSkills = new();
        private List<SkillSO> _uniqueSkills = new();
        private UnitClass _lastLoadedClass;

        private const float BaseHeight = 520f;
        private const float SkillSlotHeight = 23f;
        private const float WindowWidth = 450f;
        
        [MenuItem("Tools/Colossame/Create Unit")]
        public static void ShowWindow()
        {
            var window = GetWindow<UnitCreatorWindow>("Unit Creator");
            window.UpdateWindowSize();
        }
        
        private void OnEnable()
        {
            var guids = AssetDatabase.FindAssets("t:ClassSkillDatabase");
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _skillDatabase = AssetDatabase.LoadAssetAtPath<ClassSkillDatabase>(path);
            }
        }
        
        private void OnGUI()
        {
            GUILayout.Label("유닛 생성 설정", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            GUILayout.Label("유닛 이름 설정");
            _isUsingSpumName = EditorGUILayout.Toggle("스펌 프리팹 이름 사용", _isUsingSpumName);
            _unitName = EditorGUILayout.TextField("유닛 이름", _unitName);
            _familyName = (FamilyName)EditorGUILayout.EnumPopup(new GUIContent("가문명"), _familyName);
            _isRecruit = EditorGUILayout.Toggle("훈련병", _isRecruit);
            
            _isRanged = EditorGUILayout.Toggle("원거리 유닛", _isRanged);
            _unitClass = (UnitClass)EditorGUILayout.EnumPopup(new GUIContent("유닛 직업"), _unitClass);
            if (_unitClass != _lastLoadedClass)
            {
                LoadSkillsForClass(_unitClass);
                _lastLoadedClass = _unitClass;
            }
        
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("이미지 설정", EditorStyles.boldLabel);
            _unitImage = (Sprite)EditorGUILayout.ObjectField(new GUIContent("캐릭터 이미지"), _unitImage, typeof(Sprite), true);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("프리팹 설정", EditorStyles.boldLabel);
            spumPrefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent("SPUM Prefab"), spumPrefab, typeof(GameObject), false);
            _rangedAttack = (GameObject)EditorGUILayout.ObjectField(new GUIContent("RangedAttack Prefab"), _rangedAttack, typeof(GameObject), false);
            _meleeAttack = (GameObject)EditorGUILayout.ObjectField(new GUIContent("MeleeAttack Prefab"), _meleeAttack, typeof(GameObject), false);
            _hpBar = (GameObject)EditorGUILayout.ObjectField(new GUIContent("HP Bar"), _hpBar, typeof(GameObject), false);
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("직업 공통 스킬 (자동)", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            {
                if (_classSkills.Count == 0)
                    EditorGUILayout.HelpBox("이 직업에 등록된 공통 스킬이 없습니다.", MessageType.Info);

                foreach (var skill in _classSkills)
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.ObjectField(skill, typeof(SkillSO), false);
                    }
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("전용기 (수동 추가)", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            {
                _uniqueSkills ??= new List<SkillSO>();
                var indexToRemove = -1;
                for (var i = 0; i < _uniqueSkills.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label($"Unique {i + 1}", GUILayout.Width(60));
                    _uniqueSkills[i] = EditorGUILayout.ObjectField(_uniqueSkills[i], typeof(SkillSO), false) as SkillSO;

                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        indexToRemove = i;
                    }
                    EditorGUILayout.EndHorizontal();
                }

                if (indexToRemove >= 0)
                {
                    _uniqueSkills.RemoveAt(indexToRemove);
                    GUI.FocusControl(null);
                    UpdateWindowSize();
                }

                GUILayout.Space(5);
                if (GUILayout.Button("+ 전용기 추가", GUILayout.Height(30)))
                {
                    _uniqueSkills.Add(null);
                    UpdateWindowSize();
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void UpdateWindowSize()
        {
            var count = (_classSkills?.Count ?? 0) + (_uniqueSkills?.Count ?? 0);
            var targetHeight = BaseHeight + (count * SkillSlotHeight);
            var newSize = new Vector2(WindowWidth, targetHeight);

            this.minSize = newSize;
            this.maxSize = newSize;
        }
        
        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(_unitName))
            {
                EditorUtility.DisplayDialog("입력 오류", "unitName을 지정하세요. {가문명}_{unitName}", "확인");
                return false;
            }
            if (!spumPrefab)
            {
                EditorUtility.DisplayDialog("입력 오류", "SPUM Prefab을 지정하세요.", "확인");
                return false;
            }
            switch (_isRanged)
            {
                case true when !_rangedAttack:
                    EditorUtility.DisplayDialog("입력 오류", "RangedAttack Prefab을 지정하세요.", "확인");
                    return false;
                case false when !_meleeAttack:
                    EditorUtility.DisplayDialog("입력 오류", "MeleeAttack Prefab을 지정하세요.", "확인");
                    return false;
                default:
                    return true;
            }
        }
        
        private void LoadSkillsForClass(UnitClass unitClass)
        {
            _classSkills = _skillDatabase != null ? new List<SkillSO>(_skillDatabase.GetSkillsForClass(unitClass)) : new List<SkillSO>();
            UpdateWindowSize();
        }
        
        private void CreateUnitEditor()
        {
            var allSkills = _classSkills.Concat(_uniqueSkills.Where(s => s != null)).Distinct().ToList();
            
            var created = UnitCreator.CreateUnit(
                familyName: _familyName,
                characterName: _unitName,
                isRecruit: _isRecruit,
                isUsingSPUMName: _isUsingSpumName,
                isRanged: _isRanged,
                unitClassName: _unitClass,
                unitImage: _unitImage,
                spumPrefab: spumPrefab,
                rangedPrefab: _rangedAttack,
                meleePrefab: _meleeAttack,
                hpBarPrefab: _hpBar,
                allPossibleSkills: allSkills
            );

            if (!created) return;
            Selection.activeGameObject = created;
            EditorGUIUtility.PingObject(created);
            Debug.Log($"[UnitFactory] Created '{created.name}' successfully.");
        }
    }
}
