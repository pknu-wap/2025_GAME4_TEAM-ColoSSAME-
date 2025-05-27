using System.IO;
using Battle.Scripts.Value.Data;
using TMPro;
using UnityEngine;
using Newtonsoft.Json; // ← 필요
using CharacterInfo = Battle.Scripts.Value.Data.CharacterInfo;

namespace Battle.Scripts.UI
{
    public class StrategyUIText : MonoBehaviour
    {
        public TextMeshProUGUI Status;
        public CharacterID id;
        public string teamTag;
        public int orderIndex;

        private CharacterInfo myInfo;

        private void Start()
        {
            LoadJsonData();
        }

        public void LoadJsonData()
        {
            if (id == null || string.IsNullOrEmpty(id.characterKey))
            {
                return;
            }

            string fileName = teamTag == "Player" ? "PlayerSave.json" : "EnemySave.json";
            string filePath = Path.Combine(Application.persistentDataPath, fileName);

            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"StatText: {filePath} 파일을 찾을 수 없습니다.");
                return;
            }

            string json = File.ReadAllText(filePath);

            //Dictionary 포함된 JSON은 JsonUtility 대신 JsonConvert 사용
            CharacterData data = JsonConvert.DeserializeObject<CharacterData>(json);

            if (data.characters.TryGetValue(id.characterKey, out var info))
            {
                myInfo = info;
            }
            else
            {
                Debug.LogWarning($"StatText: characterKey {id.characterKey} 에 해당하는 데이터를 찾을 수 없습니다.");
            }
        }

        private void Update()
        {
            if (myInfo != null)
            {
                Status.text = $"ATK : {myInfo.ATK}\nDef : {myInfo.DEF}\nHp: {myInfo.CON}";
            }
        }
    }
}