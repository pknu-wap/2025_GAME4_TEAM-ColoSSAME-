using System.Collections.Generic;
using BattleK.Scripts.CharacterCreator;
using BattleK.Scripts.Data.ClassInfo;
using BattleK.Scripts.Data.Type;
using Skill;
using UnityEditor;
using UnityEngine;

namespace BattleK.Scripts.Editor
{
    public class UnitCreatorWindow : EditorWindow
    {
        private FamilyName _familyName = FamilyName.Astra;
        private bool _isUsingSpumName;
        private string _unitName = "New Unit";
        private Sprite _unitImage;
        private GameObject _spumPrefab;
        private GameObject _rangedAttack;
        private GameObject _meleeAttack;
        private GameObject _hpBar;

        private UnitClass _unitClass;
        private ClassDefinitionDatabase _classDefinitionDatabase;
        private ClassDefinitionSO _currentClassDefinition;
        private List<ClassSkillPoolSO.SkillRef> _classSkills = new();
        private UnitClass _lastLoadedClass;
        private bool _hasLoadedOnce;

        private Vector2 _scrollPosition;
        private static readonly Vector2 MinWindowSize = new(380f, 420f);

        [MenuItem("Tools/Colossame/Create Unit")]
        public static void ShowWindow()
        {
            var window = GetWindow<UnitCreatorWindow>("Unit Creator");
            window.minSize = MinWindowSize;
        }

        private void OnEnable()
        {
            minSize = MinWindowSize;

            var guids = AssetDatabase.FindAssets("t:ClassDefinitionDatabase");
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _classDefinitionDatabase = AssetDatabase.LoadAssetAtPath<ClassDefinitionDatabase>(path);
            }
            else
            {
                Debug.LogWarning("[UnitCreatorWindow] ClassDefinitionDatabase 애셋을 찾을 수 없습니다.");
            }
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            GUILayout.Label("유닛 생성 설정", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            GUILayout.Label("유닛 이름 설정");
            _isUsingSpumName = EditorGUILayout.Toggle("스펌 프리팹 이름 사용", _isUsingSpumName);
            _unitName = EditorGUILayout.TextField("유닛 이름", _unitName);
            _familyName = (FamilyName)EditorGUILayout.EnumPopup(new GUIContent("가문명"), _familyName);

            EditorGUILayout.Space();
            _unitClass = (UnitClass)EditorGUILayout.EnumPopup(new GUIContent("유닛 직업"), _unitClass);
            if (_unitClass != _lastLoadedClass || !_hasLoadedOnce)
            {
                LoadClassDefinition(_unitClass);
                _lastLoadedClass = _unitClass;
                _hasLoadedOnce = true;
            }

            if (_currentClassDefinition == null)
            {
                EditorGUILayout.HelpBox("이 직업에 대한 ClassDefinitionSO가 등록되어 있지 않습니다. ClassDefinitionDatabase에 추가하세요.", MessageType.Warning);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("이미지 설정", EditorStyles.boldLabel);
            _unitImage = (Sprite)EditorGUILayout.ObjectField(new GUIContent("캐릭터 이미지"), _unitImage, typeof(Sprite), true);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("프리팹 설정", EditorStyles.boldLabel);
            _spumPrefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent("SPUM Prefab"), _spumPrefab, typeof(GameObject), false);
            _rangedAttack = (GameObject)EditorGUILayout.ObjectField(new GUIContent("RangedAttack Prefab"), _rangedAttack, typeof(GameObject), false);
            _meleeAttack = (GameObject)EditorGUILayout.ObjectField(new GUIContent("MeleeAttack Prefab"), _meleeAttack, typeof(GameObject), false);
            _hpBar = (GameObject)EditorGUILayout.ObjectField(new GUIContent("HP Bar"), _hpBar, typeof(GameObject), false);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("직업 공통 스킬 (참고용, 자동)", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            {
                if (_classSkills.Count == 0)
                    EditorGUILayout.HelpBox("이 직업에 등록된 공통 스킬이 없습니다.", MessageType.Info);

                foreach (var skillRef in _classSkills)
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.TextField(skillRef.skillName);
                    }
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);
            using (new EditorGUI.DisabledScope(_currentClassDefinition == null))
            {
                if (GUILayout.Button("유닛 생성", GUILayout.Height(35)))
                {
                    if (ValidateInputs()) CreateUnitEditor();
                }
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.EndScrollView();
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(_unitName))
            {
                EditorUtility.DisplayDialog("입력 오류", "unitName을 지정하세요. {가문명}_{unitName}", "확인");
                return false;
            }
            if (!_spumPrefab)
            {
                EditorUtility.DisplayDialog("입력 오류", "SPUM Prefab을 지정하세요.", "확인");
                return false;
            }
            if (_currentClassDefinition == null)
            {
                EditorUtility.DisplayDialog("입력 오류", "선택한 직업에 대한 ClassDefinitionSO가 없습니다.", "확인");
                return false;
            }
            switch (_currentClassDefinition.IsRangedDefault)
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

        private void LoadClassDefinition(UnitClass unitClass)
        {
            _currentClassDefinition = _classDefinitionDatabase != null
                ? _classDefinitionDatabase.GetDefinition(unitClass)
                : null;

            _classSkills = _currentClassDefinition?.CommonSkillPool != null
                ? new List<ClassSkillPoolSO.SkillRef>(_currentClassDefinition.CommonSkillPool.skills)
                : new List<ClassSkillPoolSO.SkillRef>();
        }

        private void CreateUnitEditor()
        {
            var created = UnitCreator.CreateUnit(
                familyName: _familyName,
                characterName: _unitName,
                isUsingSPUMName: _isUsingSpumName,
                classDefinition: _currentClassDefinition,
                unitImage: _unitImage,
                spumPrefab: _spumPrefab,
                rangedPrefab: _rangedAttack,
                meleePrefab: _meleeAttack,
                hpBarPrefab: _hpBar
            );

            if (!created) return;
            Selection.activeGameObject = created;
            EditorGUIUtility.PingObject(created);
            Debug.Log($"[UnitFactory] Created '{created.name}' successfully.");
        }
    }
}