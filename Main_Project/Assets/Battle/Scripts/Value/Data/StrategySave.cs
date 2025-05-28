using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Battle.Scripts.Value.Data
{
    public class StrategySave : MonoBehaviour
    {
        public string targetTag = "PlayerCharacter"; // 또는 "EnemyCharacter"

        private string SaveFileName => $"{targetTag}Save.json";
        private string savePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        public void SavePositionsOnly()
        {
            if (!File.Exists(savePath))
            {
                Debug.LogWarning("저장 파일이 존재하지 않습니다.");
                return;
            }

            // 1. 기존 JSON 불러오기
            string json = File.ReadAllText(savePath);
            CharacterData data = JsonConvert.DeserializeObject<CharacterData>(json);

            // 2. 저장 대상 레이어 설정 (예: "PlayerCharacterSeat" 또는 "EnemyCharacterSeat")
            int targetLayer = 9;

            // 3. 모든 GameObject에서 레이어 기준 필터링
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (var obj in allObjects)
            {
                if (obj.layer != targetLayer) continue;

                var id = obj.GetComponent<CharacterID>();
                if (id == null || string.IsNullOrEmpty(id.characterTeamKey)) continue;

                string fullKey = $"{id.characterTeamKey}_{id.characterKey}";
                if (!data.characters.ContainsKey(fullKey)) continue;

                // 4. 위치 저장
                var character = data.characters[fullKey];
                character.IsDeployed = true;
                character.x = obj.transform.position.x + 2.6f;
                character.y = obj.transform.position.y + 0.8f;
                character.z = obj.transform.position.z;
            }

            // 5. JSON 파일로 다시 저장
            string updatedJson = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(savePath, updatedJson);
            Debug.Log($"{targetTag} 위치만 저장 완료 (레이어 기반)");
        }
    }
}