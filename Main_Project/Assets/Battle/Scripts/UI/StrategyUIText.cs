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
            if (id == null || string.IsNullOrEmpty(id.characterKey) || string.IsNullOrEmpty(id.characterTeamKey))
                return;

            string fileName = teamTag == "Player" ? "PlayerSave.json" : "EnemySave.json";
            string filePath = Path.Combine(Application.persistentDataPath, fileName);

            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"StatText: {filePath} 파일을 찾을 수 없습니다.");
                return;
            }

            string json = File.ReadAllText(filePath);
            CharacterData data = JsonConvert.DeserializeObject<CharacterData>(json);

            string fullKey = $"{id.characterTeamKey}_{id.characterKey}";
            if (data.characters.TryGetValue(fullKey, out var info))
            {
                myInfo = info;
            }
            else
            {
                Debug.LogWarning($"StatText: {fullKey} 에 해당하는 데이터를 찾을 수 없습니다.");
            }
        }

        private void Update()
        {
            if (myInfo != null)
            {
                Status.text = $"Hp: {myInfo.CON}\nATK : {myInfo.ATK}\nDef : {myInfo.DEF}";
            }
        }
    }
}