using System;
using Battle.Scripts.Ai;
using Battle.Scripts.Value;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battle.Scripts.UI
{
    public class StatText : MonoBehaviour
    {
        public BattleAI ai;
        public TextMeshProUGUI Status;

        private void Update()
        {
            Status.text = $"ATK : {ai.damage}\nDef : {ai.defense}\nHp: {ai.characterValue.currentHp}/{ai.hp}";
        }
    }
}
