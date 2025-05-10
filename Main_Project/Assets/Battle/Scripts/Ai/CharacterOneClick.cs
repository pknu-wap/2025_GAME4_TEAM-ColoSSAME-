using Pathfinding;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Battle.Scripts.Ai
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BattleAI))]
    public class CharacterOneClick : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            BattleAI battleAI = (BattleAI)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("무기 선택", EditorStyles.boldLabel);
            battleAI.weaponType = (WeaponType)EditorGUILayout.EnumPopup("무기 종류", battleAI.weaponType);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("팀 설정", EditorStyles.boldLabel);
            battleAI.team = (TeamType)EditorGUILayout.EnumPopup("Team", battleAI.team);

            if (GUILayout.Button("전사 설정"))
            {
                if (battleAI.GetComponent<AIPath>() == null)
                {
                    battleAI.gameObject.AddComponent<AIPath>();
                    Debug.Log("AIPath 컴포넌트를 추가했습니다.");
                }
                
                // AIDestinationSetter 컴포넌트 자동 추가
                if (battleAI.GetComponent<AIDestinationSetter>() == null)
                {
                    battleAI.gameObject.AddComponent<AIDestinationSetter>();
                    Debug.Log("AIDestinationSetter 컴포넌트를 추가했습니다.");
                }
                AddWeapon(battleAI);
                AddHealthBar(battleAI);
                
                battleAI.SetupComponents();
                battleAI.aiAnimator.ChooseWeapon();
            }
        }

        private void AddWeapon(BattleAI battleAI)
        {
            Transform existing = battleAI.transform.Find("Weapon");
            if (existing != null)
            {
                Debug.LogWarning("이미 Weapon이 존재합니다.");
                return;
            }
            GameObject Weapon = new GameObject(); 
            Weapon.transform.SetParent(battleAI.transform); 
            Weapon.transform.localPosition = Vector3.zero; 
            Weapon.name = "Weapon";
        }
        private void AddHealthBar(BattleAI warriorAI)
        {
            // 프리팹 경로
            string path = "Assets/Battle/Prefabs/HealthBar/HealthBar 1.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null)
            {
                Debug.LogError("HealthBar.prefab을 찾을 수 없습니다. 경로를 확인하세요.");
                return;
            }

            Transform unitRoot = warriorAI.transform.Find("UnitRoot");

            if (unitRoot == null)
            {
                Debug.LogWarning("UnitRoot를 찾을 수 없어 HealthBar를 WarriorAI 루트에 추가합니다.");
                unitRoot = warriorAI.transform;
            }

            // 이미 HealthBar가 있는지 확인
            Transform existing = unitRoot.Find("HealthBar");
            if (existing != null)
            {
                Debug.LogWarning($"{warriorAI.name}은 이미 HealthBar를 가지고 있습니다. 추가하지 않습니다.");
                return;
            }

            // HealthBar 인스턴스 추가
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.SetParent(unitRoot);
            instance.transform.localPosition = new Vector3(0f, 0f, 0.15f); // 캐릭터 위쪽에 배치
            instance.name = "HealthBar";
            instance.transform.localScale = new Vector3(1f, 1f, 1f);

            Debug.Log($"HealthBar가 {warriorAI.name}에 추가되었습니다.");
        }

    }
}