using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using Battle.Scripts.Value.Data.Class;
using Battle.Scripts.Ai;

namespace Battle.Scripts.Value.Data
{
    public class DeployedLoader : MonoBehaviour
    {
        public GameObject characterPrefab;       // 공통 프리팹
        public SaveManager saveManager;          // SaveManager 연결
        public string fileName = "Save.json";
        public string fileTag;

        private string FilePath => Path.Combine(Application.persistentDataPath, fileName);
        public List<GameObject> spawnedCharacters = new(); // 추가

        public void LoadFromDeployedJson()
        {
            if (!File.Exists(FilePath))
            {
                Debug.LogWarning("파일이 존재하지 않습니다: " + FilePath);
                return;
            }
            string json = File.ReadAllText(FilePath);
            CharacterData data = JsonConvert.DeserializeObject<CharacterData>(json);
            spawnedCharacters.Clear(); // 기존 리스트 초기화

            foreach (var pair in data.characters)
            {
                CharacterInfo info = pair.Value;
                if (!info.IsDeployed) continue;

                GameObject obj = Instantiate(characterPrefab, new Vector3(info.x, info.y, info.z), Quaternion.identity);
                obj.name = "basic" + info.team + info.characterKey;
                obj.tag = fileTag;

                var id = obj.GetComponent<CharacterID>();
                if (id != null)
                    id.characterKey = info.characterKey;

                spawnedCharacters.Add(obj); // 리스트에 추가
            }

            saveManager.LoadFromButton();
        }
        public void ClearAllIsDeployedFlags()
        {
            if (!File.Exists(FilePath)) return;

            string json = File.ReadAllText(FilePath);
            CharacterData data = JsonConvert.DeserializeObject<CharacterData>(json);

            bool modified = false;
            foreach (var character in data.characters.Values)
            {
                if (character.IsDeployed)
                {
                    character.x = 0;
                    character.y = 0;
                    character.z = 0;
                    character.IsDeployed = false;
                    modified = true;
                }
            }

            if (modified)
            {
                string updatedJson = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(FilePath, updatedJson);
                Debug.Log("모든 IsDeployed 값이 false로 초기화되었습니다.");
            }
        }
    }
}
