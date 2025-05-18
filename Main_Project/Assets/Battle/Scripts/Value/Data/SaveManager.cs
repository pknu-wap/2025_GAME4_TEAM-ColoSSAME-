using System.IO;
using System.Collections.Generic;
using Battle.Scripts.Ai.CharacterCreator;
using Newtonsoft.Json;
using UnityEngine;

namespace Battle.Scripts.Value.Data
{
    public class SaveManager : MonoBehaviour
    {
        private string savePath => Application.persistentDataPath + "/save.json";
        public string targetTag = "Player"; // 저장할 대상 태그

        public void Save(CharacterData data)
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(savePath, json);
            Debug.Log("저장 완료: " + savePath);
        }

        public CharacterData Load()
        {
            if (File.Exists(savePath))
            {
                string json = File.ReadAllText(savePath);
                CharacterData data = JsonConvert.DeserializeObject<CharacterData>(json);
                Debug.Log("불러오기 완료");
                return data;
            }
            Debug.LogWarning("저장 파일 없음");
            return new CharacterData();
        }

        public void SaveFromButton()
        {
            CharacterData data = new CharacterData();
            GameObject[] characters = GameObject.FindGameObjectsWithTag(targetTag);

            foreach (var obj in characters)
            {
                var spum = obj.GetComponent<SPUM_Prefabs>();
                var id = obj.GetComponent<CharacterID>();
                if (spum == null || id == null) continue;

                var info = new CharacterInfo
                {
                    characterKey = id.characterKey,
                    name = obj.name,

                    bodyPath1 = spum.ImageElement[0].ItemPath,
                    bodyPath2 = spum.ImageElement[1].ItemPath,
                    bodyPath3 = spum.ImageElement[2].ItemPath,
                    bodyPath4 = spum.ImageElement[3].ItemPath,
                    bodyPath5 = spum.ImageElement[4].ItemPath,
                    bodyPath6 = spum.ImageElement[5].ItemPath,
                    eyePath1 = spum.ImageElement[6].ItemPath,
                    eyePath2 = spum.ImageElement[7].ItemPath,
                    eyeColor = new SerializableColor(spum.ImageElement[7].Color),
                    hairPath = spum.ImageElement[8].ItemPath,
                    hairColor = new SerializableColor(spum.ImageElement[8].Color),
                    hairMaskIndex = spum.ImageElement[8].MaskIndex,
                    faceHairPath = spum.ImageElement[9].ItemPath,
                    faceHairColor = new SerializableColor(spum.ImageElement[9].Color),
                    helmetPath = spum.ImageElement[10].ItemPath,
                    helmetMaskIndex = spum.ImageElement[10].MaskIndex,
                    clothPath1 = spum.ImageElement[11].ItemPath,
                    clothPath2 = spum.ImageElement[12].ItemPath,
                    clothPath3 = spum.ImageElement[13].ItemPath,
                    pantsPath1 = spum.ImageElement[14].ItemPath,
                    pantsPath2 = spum.ImageElement[15].ItemPath,
                    armorPath1 = spum.ImageElement[16].ItemPath,
                    armorPath2 = spum.ImageElement[17].ItemPath,
                    armorPath3 = spum.ImageElement[18].ItemPath,
                    backPath = spum.ImageElement[19].ItemPath,
                    weaponPath1 = spum.ImageElement[20].ItemPath,
                    weaponPath2 = spum.ImageElement[21].ItemPath
                };

                data.characters[id.characterKey] = info;
            }

            Save(data);
        }

        public void LoadFromButton()
        {
            CharacterData loaded = Load();
            GameObject[] characters = GameObject.FindGameObjectsWithTag(targetTag);

            foreach (var obj in characters)
            {
                var id = obj.GetComponent<CharacterID>();
                var spum = obj.GetComponent<SPUM_Prefabs>();
                var createTest = obj.GetComponent<CreateTest>();
                if (id == null || spum == null || !loaded.characters.ContainsKey(id.characterKey)) continue;

                var info = loaded.characters[id.characterKey];

                spum.ImageElement[0].ItemPath = info.bodyPath1;
                spum.ImageElement[1].ItemPath = info.bodyPath2;
                spum.ImageElement[2].ItemPath = info.bodyPath3;
                spum.ImageElement[3].ItemPath = info.bodyPath4;
                spum.ImageElement[4].ItemPath = info.bodyPath5;
                spum.ImageElement[5].ItemPath = info.bodyPath6;
                spum.ImageElement[6].ItemPath = info.eyePath1;
                spum.ImageElement[7].ItemPath = info.eyePath2;
                spum.ImageElement[7].Color = info.eyeColor.ToUnityColor();
                spum.ImageElement[8].ItemPath = info.hairPath;
                spum.ImageElement[8].Color = info.hairColor.ToUnityColor();
                spum.ImageElement[8].MaskIndex = info.hairMaskIndex;
                spum.ImageElement[9].ItemPath = info.faceHairPath;
                spum.ImageElement[9].Color = info.faceHairColor.ToUnityColor();
                spum.ImageElement[10].ItemPath = info.helmetPath;
                spum.ImageElement[10].MaskIndex = info.helmetMaskIndex;
                spum.ImageElement[11].ItemPath = info.clothPath1;
                spum.ImageElement[12].ItemPath = info.clothPath2;
                spum.ImageElement[13].ItemPath = info.clothPath3;
                spum.ImageElement[14].ItemPath = info.pantsPath1;
                spum.ImageElement[15].ItemPath = info.pantsPath2;
                spum.ImageElement[16].ItemPath = info.armorPath1;
                spum.ImageElement[17].ItemPath = info.armorPath2;
                spum.ImageElement[18].ItemPath = info.armorPath3;
                spum.ImageElement[19].ItemPath = info.backPath;
                spum.ImageElement[20].ItemPath = info.weaponPath1;
                spum.ImageElement[21].ItemPath = info.weaponPath2;

                createTest?.RerenderAllParts(obj);
            }

            Debug.Log("모든 캐릭터 로드 완료");
        }
    }
}