using UnityEditor;
using UnityEngine;

namespace Battle.Scripts.Class.Close.Warrior
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(WarriorAI))]
    public class WarriorOneClick : Editor
    {
        public GameObject healthBarPrefab;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            WarriorAI warriorAI = (WarriorAI)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("무기 선택", EditorStyles.boldLabel);
            warriorAI.weaponType = (WeaponType)EditorGUILayout.EnumPopup("무기 종류", warriorAI.weaponType);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("팀 설정", EditorStyles.boldLabel);
            warriorAI.team = (TeamType)EditorGUILayout.EnumPopup("Team", warriorAI.team);

            if (GUILayout.Button("전사 설정"))
            {
                warriorAI.SetupComponents();
                warriorAI.warriorAnimator.ChooseWeapon();
            }

            if (GUILayout.Button("체력바 추가"))
            {
                AddHealthBar(warriorAI);
            }
        }

        private void AddHealthBar(WarriorAI warriorAI)
        {
            // 프리팹 경로
            string path = "Assets/Battle/HealthBar/HealthBar.prefab";
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
            instance.transform.localPosition = new Vector3(0f, 1f, 0f); // 캐릭터 위쪽에 배치
            instance.name = "HealthBar";

            Debug.Log($"HealthBar가 {warriorAI.name}에 추가되었습니다.");
        }

    }
}