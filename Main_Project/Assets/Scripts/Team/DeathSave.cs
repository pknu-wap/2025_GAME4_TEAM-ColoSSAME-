using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Battle.Scripts.Ai;
using Battle.Scripts.Ai.CharacterCreator;
using Battle.Scripts.Value.Data;
using System.IO;
using Scripts.Team.Fightermanage;
using Battle.Scripts.ImageManager;

namespace Scripts.Team.DeathManage
{
    public class DeathSave : MonoBehaviour
    {
        private string savePath => $"{Application.persistentDataPath}/DeathSave.json";
        public SaveManager savemanage; 
        public State state;
        public TransparentScreenshot changeimage;


        void Start()
        {
            savemanage.LoadFromButton();
        }

        private void Save(CharacterData data)
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(savePath, json);
            Debug.Log($"사망한 캐릭터 정보 저장 완료: {savePath}");
        }

        private CharacterData Load()
        {
            if (File.Exists(savePath))
            {
                string json = File.ReadAllText(savePath);
                CharacterData data = JsonConvert.DeserializeObject<CharacterData>(json);
                Debug.Log($"사망 정보 불러오기 완료");
                return data;
            }
            Debug.LogWarning($"사망 정보 저장 파일 없음");
            return new CharacterData();
        }


        public void SaveFromButton()
        {
            // 1. 기존 저장 데이터 불러오기
            CharacterData data = Load();
        
            // 2. 현재 씬의 대상 캐릭터 찾기
            GameObject[] characters = GameObject.FindGameObjectsWithTag("Player");
        
            foreach (var obj in characters)
            {
                var spum = obj.GetComponent<SPUM_Prefabs>();
                var id = obj.GetComponent<CharacterID>();
                BattleAI ai = obj.GetComponent<BattleAI>();
                if (spum == null || id == null) continue; 

                Debug.Log(state.fighterCount);
                if (id.characterKey != (state.fighterCount+1).ToString()) continue;

                var info = new Battle.Scripts.Value.Data.CharacterInfo
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
                    DEF = ai.defense,
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

        
    }
}