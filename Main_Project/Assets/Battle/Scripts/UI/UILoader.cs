using System;
using System.Linq;
using Battle.Scripts.Ai;
using Battle.Scripts.ImageManager;
using Battle.Scripts.Value;
using Battle.Scripts.Value.Data;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Battle.Scripts.UI
{
    public class UILoader : MonoBehaviour
    {
        public SaveManager saveManager;
        public DeployedLoader deployedLoader;
        public StatText[] statText;
        public TransparentScreenshot iconGenerator;

        private void Start()
        {
            statText = FindObjectsOfType<StatText>().OrderBy(text => text.orderIndex).ToArray();
        }

        public void LoadAndSpawnCharacters()
        {
            // 1. StatText를 미리 비활성화
            foreach (var text in statText)
            {
                text.gameObject.SetActive(false);
            }

            // 2. 캐릭터 생성
            saveManager.TargetPlayer();
            deployedLoader.fileTag = "Player";
            deployedLoader.fileName = "PlayerSave.json";
            deployedLoader.LoadFromDeployedJson();
            UISetup(0);
            saveManager.TargetEnemy();
            deployedLoader.fileTag = "Enemy";
            deployedLoader.fileName = "EnemySave.json";
            deployedLoader.LoadFromDeployedJson();
            UISetup(4);
            
            // 5. 아이콘 이미지 불러오기
            iconGenerator.LoadAllSprites();
            
            // 6. 모든 캐릭터의 IsDeployed를 false로 저장
            deployedLoader.fileTag = "Player";
            deployedLoader.fileName = "PlayerSave.json";
            deployedLoader.ClearAllIsDeployedFlags();
            deployedLoader.fileTag = "Enemy";
            deployedLoader.fileName = "EnemySave.json";
            deployedLoader.ClearAllIsDeployedFlags();
        }

        void UISetup(int start)
        {
            foreach (var spawned in deployedLoader.spawnedCharacters)
            {
                var spawnedID = spawned.GetComponent<CharacterID>();
                if (spawnedID == null) continue;

                // statText 연결
                if (start < statText.Length)
                {
                    var statTag = statText[start].teamTag;
                    Debug.Log($"[비교] spawned: {spawned.name} ({spawned.tag}) vs statText[{start}] teamTag: {statTag}");
                    if (spawned.CompareTag(statTag))
                    {
                        var statID = statText[start].GetComponent<CharacterID>();
                        if (statID != null)
                        {
                            statID.characterKey = spawnedID.characterKey;
                        }
                    }
                }

                // Pannel 연결
                if (start < iconGenerator.Pannels.Count)
                {
                    var panelID = iconGenerator.Pannels[start].GetComponent<CharacterID>();
                    if (panelID != null)
                    {
                        panelID.characterKey = spawnedID.characterKey;
                    }
                }

                start++;
                spawned.GetComponent<BattleAI>().isWinner = FindObjectOfType<IsWinner>();
            }

            // 4. StatText 재활성화 및 연결
            foreach (var text in statText)
            {
                text.gameObject.SetActive(true);
                text.StatConnect();
            }
            FindObjectOfType<IsWinner>().startSetting();
        }
        
    }
}
