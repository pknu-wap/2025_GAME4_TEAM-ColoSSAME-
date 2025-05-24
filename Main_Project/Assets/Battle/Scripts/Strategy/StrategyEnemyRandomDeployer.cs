using System.Collections.Generic;
using System.IO;
using Battle.Scripts.Value.Data;
using Newtonsoft.Json;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Battle.Scripts.Strategy
{
    public class StrategyEnemyRandomDeployer : MonoBehaviour
    {
        public string fileName = "EnemySave.json";

        private string SavePath => Path.Combine(Application.persistentDataPath, fileName);

        private readonly Vector2[][] formations = new Vector2[][]
        {
            // OneTwoOne
            new Vector2[]
            {
                new Vector2(1f, -1.5f),
                new Vector2(4f, -1.5f),
                new Vector2(2.5f, -0.2f),
                new Vector2(2.5f, -2.8f)
            },
            // TwoTwo
            new Vector2[]
            {
                new Vector2(1.5f, -1.9f),
                new Vector2(4.1f, -1.9f),
                new Vector2(1.5f, -0.8f),
                new Vector2(4.1f, -0.8f)
            },
            // OneOneTwo
            new Vector2[]
            {
                new Vector2(0.8f, -1.5f),
                new Vector2(2.8f, -1.5f),
                new Vector2(4f, -3.2f),
                new Vector2(4f, 0.2f)
            }
        };

        [ContextMenu("랜덤 배치 저장 (Enemy)")]
        public void DeployRandomEnemies()
        {
            if (!File.Exists(SavePath))
            {
                Debug.LogWarning("EnemySave.json 파일이 존재하지 않습니다: " + SavePath);
                return;
            }

            string json = File.ReadAllText(SavePath);
            CharacterData data = JsonConvert.DeserializeObject<CharacterData>(json);

            // 모든 key를 리스트로 가져와 랜덤 셔플
            var keys = new List<string>(data.characters.Keys);
            keys.Shuffle();

            // 4명 선택
            var selectedKeys = keys.GetRange(0, Mathf.Min(4, keys.Count));

            // 랜덤 배치 선택
            Vector2[] formation = formations[Random.Range(0, formations.Length)];

            for (int i = 0; i < selectedKeys.Count; i++)
            {
                string key = selectedKeys[i];
                if (!data.characters.ContainsKey(key)) continue;

                var character = data.characters[key];
                character.IsDeployed = true;
                character.x = formation[i].x;
                character.y = formation[i].y;
                character.z = 0f;
            }

            string updatedJson = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(SavePath, updatedJson);
            Debug.Log("랜덤 적 배치 저장 완료");
        }
    }

    // 리스트 셔플 확장 메서드
    public static class ListExtensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }
    }
}
