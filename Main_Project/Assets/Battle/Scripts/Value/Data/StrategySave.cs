using Newtonsoft.Json;
using System.IO;
using UnityEngine;

namespace Battle.Scripts.Value.Data
{
	public class StrategySave : MonoBehaviour
	{
		public string targetTag = "Player"; // or "Enemy"

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

			// 2. 현재 씬의 해당 캐릭터들 순회
			GameObject[] characters = GameObject.FindGameObjectsWithTag("Character");
			foreach (var obj in characters)
			{
				var id = obj.GetComponent<CharacterID>();
				if (id == null || !data.characters.ContainsKey(id.characterKey)) continue;

				// 3. 현재 위치를 기존 데이터에 반영
				data.characters[id.characterKey].x = obj.transform.position.x;
				data.characters[id.characterKey].y = obj.transform.position.y;
				data.characters[id.characterKey].z = obj.transform.position.z;
			}

			// 4. 수정된 전체 데이터를 다시 저장
			string updatedJson = JsonConvert.SerializeObject(data, Formatting.Indented);
			File.WriteAllText(savePath, updatedJson);
			Debug.Log($"{targetTag} 위치만 저장 완료");
		}
	}
}
