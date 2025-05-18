using UnityEngine;

namespace Battle.Scripts.Value
{
    public class PersistentCharacterSpawner : MonoBehaviour
    {
        public GameObject warriorPrefab;
        public GameObject archerPrefab;

        public Transform spawnParent;

        // 클래스별 생성
        public void SpawnCharacter(string className, GameObject prefab, int count)
        {
            int lastIndex = PlayerPrefs.GetInt($"CharacterCount_{className}", 0);

            for (int i = 0; i < count; i++)
            {
                int newIndex = lastIndex + i + 1;
                GameObject obj = Instantiate(prefab, spawnParent);
                obj.name = $"{className}_{newIndex}";
                Debug.Log($"📦 생성됨: {obj.name}");
            }

            PlayerPrefs.SetInt($"CharacterCount_{className}", lastIndex + count);
            PlayerPrefs.Save();
        }

        // 예시 호출용
        [ContextMenu("전사 생성")]
        public void SpawnWarriors()
        {
            SpawnCharacter("Warrior", warriorPrefab, 1);
        }

        [ContextMenu("궁수 생성")]
        public void Spawn1Archer()
        {
            SpawnCharacter("Archer", archerPrefab, 1);
        }

        [ContextMenu("캐릭터 카운터 초기화 (테스트용)")]
        public void ResetCounters()
        {
            PlayerPrefs.DeleteKey("CharacterCount_Warrior");
            PlayerPrefs.DeleteKey("CharacterCount_Archer");
            Debug.Log("카운터 초기화됨");
        }
    }
}