using System.Collections.Generic;
using BattleK.Scripts.AI.SO.Base;
using BattleK.Scripts.CharacterCreator;
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
        private List<SkillSO> _skillPrefabs = new();

        private const float BaseHeight = 520f;
        private const float SkillSlotHeight = 23f;
        private const float WindowWidth = 450f;
        
        [MenuItem("Tools/Colossame/Create Unit")]
        public static void ShowWindow()
        {
            var window = GetWindow<UnitCreatorWindow>("Unit Creator");
            window.UpdateWindowSize();
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
            EditorGUILayout.LabelField("스킬 설정", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            {
                _skillPrefabs ??= new List<SkillSO>();
                var indexToRemove = -1;
                for (var i = 0; i < _skillPrefabs.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label($"Skill {i + 1}", GUILayout.Width(60));
                    _skillPrefabs[i] = EditorGUILayout.ObjectField(_skillPrefabs[i], typeof(SkillSO), false) as SkillSO;

                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        indexToRemove = i;
                    }
                    EditorGUILayout.EndHorizontal();
                }

                if (indexToRemove >= 0)
                {
                    _skillPrefabs.RemoveAt(indexToRemove);
                    GUI.FocusControl(null);
                    UpdateWindowSize();
                }
                
                GUILayout.Space(5);
                if (GUILayout.Button("+", GUILayout.Height(30)))
                {
                    _skillPrefabs.Add(null);
                    UpdateWindowSize();
                }
            }
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            if (!GUILayout.Button("유닛 생성 (Create)", GUILayout.Height(30))) return;
            if (ValidateInputs())
            {
                CreateUnitEditor();
            }
        }

        private void UpdateWindowSize()
        {
            var count =_skillPrefabs?.Count ?? 0;
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
        
        private void CreateUnitEditor()
        {
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
                skillPrefabs: _skillPrefabs
            );

            if (!created) return;
            Selection.activeGameObject = created;
            EditorGUIUtility.PingObject(created);
            Debug.Log($"[UnitFactory] Created '{created.name}' successfully.");
        }
    }
}
