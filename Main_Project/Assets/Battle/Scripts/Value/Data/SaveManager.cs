using System.IO;
using System.Collections.Generic;
using Battle.Scripts.Ai;
using Battle.Scripts.Ai.CharacterCreator;
using Newtonsoft.Json;
using UnityEngine;

namespace Battle.Scripts.Value.Data
{
    public class SaveManager : MonoBehaviour
    {
        public string targetTag = "Player"; // 저장 대상 태그 ("Player" or "Enemy")
        private string SaveFileName => $"{targetTag}Save.json";
        private string savePath => Path.Combine(Application.persistentDataPath, SaveFileName);
        public void TargetPlayer ()
        {
            targetTag = "Player";
        }

        public void TargetEnemy ()
        {
            targetTag = "Enemy";
        }

        private void Save(CharacterData data)
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(savePath, json);
            Debug.Log($"{targetTag} 저장 완료: {savePath}");
        }

        private CharacterData Load()
        {
            if (File.Exists(savePath))
            {
                string json = File.ReadAllText(savePath);
                CharacterData data = JsonConvert.DeserializeObject<CharacterData>(json);
                Debug.Log($"{targetTag} 불러오기 완료");
                return data;
            }
            Debug.LogWarning($"{targetTag} 저장 파일 없음");
            return new CharacterData();
        }

        public void SaveFromButton()
        {
            // 1. 기존 저장 데이터 불러오기
            CharacterData data = Load();
        
            // 2. 현재 씬의 대상 캐릭터 찾기
            GameObject[] characters = GameObject.FindGameObjectsWithTag(targetTag);
        
            foreach (var obj in characters)
            {
                var spum = obj.GetComponent<SPUM_Prefabs>();
                var id = obj.GetComponent<CharacterID>();
                BattleAI ai = obj.GetComponent<BattleAI>();
                if (spum == null || id == null) continue;

                var info = new CharacterInfo
                {
                    characterKey = id.characterKey,
                    characterTeamKey = id.characterTeamKey,
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
                    weaponType1 = spum.ImageElement[20].PartSubType,
                    weaponPath1 = spum.ImageElement[20].ItemPath,
                    weaponType2 = spum.ImageElement[21].PartSubType,
                    weaponPath2 = spum.ImageElement[21].ItemPath,
                    ATK = ai.damage,
                    CON = ai.hp,
                    classType = ai.Class,
                    weaponType = ai.weaponType,
                    IsDeployed = false,
                    team = ai.team
                };
        
                // 3. 기존에 있으면 덮어쓰기, 없으면 추가
                // SaveFromButton 안에서 수정
                string fullKey = $"{id.characterTeamKey}_{id.characterKey}";
                data.characters[fullKey] = info;
            }
        
            // 4. 저장
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
                var resetCharacter = obj.GetComponent<RandomCharacter>();
                BattleAI ai = obj.GetComponent<BattleAI>();
                resetCharacter?.Reset();
                string fullKey = $"{id.characterTeamKey}_{id.characterKey}";
                if (!loaded.characters.ContainsKey(fullKey)) continue;
                var info = loaded.characters[fullKey];

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
                spum.ImageElement[20].PartSubType = info.weaponType1;
                spum.ImageElement[20].ItemPath = info.weaponPath1;
                spum.ImageElement[21].PartSubType = info.weaponType2;
                spum.ImageElement[21].ItemPath = info.weaponPath2;
                ai.ApplyLoadedStats(info);

                createTest?.RerenderAllParts(obj);
                if (targetTag == "Player")
                {
                    ai.FlipToRight();
                }
                else
                {
                    ai.FlipToLeft();
                }
                ai.SetupComponents();
            }

            Debug.Log($"{targetTag} 캐릭터 로드 완료");
        }
    }
}
