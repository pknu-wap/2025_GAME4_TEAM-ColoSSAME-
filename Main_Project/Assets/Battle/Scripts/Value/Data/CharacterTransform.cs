using Newtonsoft.Json;
using System.IO;
using UnityEngine;

namespace Battle.Scripts.Value.Data
{
    public class CharacterTransform : MonoBehaviour
    {
        public string targetTag = "Player"; // or "Enemy"

        private string SaveFileName => $"{targetTag}Save.json";
        private string savePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        public void LoadTransform()
        {
            if (!File.Exists(savePath))
            {
                Debug.LogWarning("불러올 위치 파일이 존재하지 않습니다.");
                return;
            }

            // 1. JSON 파일 불러오기
            string json = File.ReadAllText(savePath);
            CharacterData data = JsonConvert.DeserializeObject<CharacterData>(json);

            // 2. 씬의 모든 캐릭터 순회
            GameObject[] characters = GameObject.FindGameObjectsWithTag("Player");
            foreach (var obj in characters)
            {
                var id = obj.GetComponent<CharacterID>();
                if (id == null || !data.characters.ContainsKey(id.characterKey)) continue;

                // 3. 저장된 좌표를 Transform에 적용
                var saved = data.characters[id.characterKey];
                obj.transform.position = new Vector3(saved.x, saved.y, saved.z);
            }

            Debug.Log($"{targetTag} 위치 불러오기 완료");
        }
    }
}