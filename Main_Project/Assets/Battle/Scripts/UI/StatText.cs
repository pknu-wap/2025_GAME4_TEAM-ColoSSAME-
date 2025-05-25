using System;
using Battle.Scripts.Ai;
using Battle.Scripts.Value;
using Battle.Scripts.Value.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battle.Scripts.UI
{
    public class StatText : MonoBehaviour
    {
        public BattleAI ai;
        public TextMeshProUGUI Status;
        public CharacterID id;
        public string teamTag;
        public int orderIndex;
        public void StatConnect()
        {
            if (id == null)
            {
                Debug.LogWarning("StatText: CharacterID가 연결되지 않았습니다.");
                return;
            }

            string myKey = id.characterKey;

            // 씬에 있는 모든 BattleAI 컴포넌트를 가진 오브젝트 탐색
            BattleAI[] allAIs = FindObjectsOfType<BattleAI>();
            foreach (var candidate in allAIs)
            {
                var candidateID = candidate.GetComponent<CharacterID>();
                var candidateTag = candidate.tag;
                if (candidateID != null && candidateID.characterKey == myKey && candidate.gameObject != this.gameObject && candidateTag == teamTag)
                {
                    ai = candidate;
                    break;
                }
            }
        }

        
        private void Update()
        {
            if(ai) Status.text = $"ATK : {ai.damage}\nDef : {ai.defense}\nHp: {ai.characterValue.currentHp}/{ai.hp}";
        }
    }
}
