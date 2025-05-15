using System.Collections.Generic;
using Battle.Scripts.Ai;
using UnityEngine;

namespace Battle.Scripts.Value
{
    public class DataManager : MonoBehaviour
    {
        
        List<BattleAI> aiList = new List<BattleAI>();
        public void CharacterSetting()
        {
            foreach (var Character in aiList)
            {
                int Job = Random.Range(0, 6);
                switch (Job)
                {
                    //Knight
                    case 0:
                        Character.hp = 100;
                        Character.defense = 3;
                        Character.damage = Random.Range(2, 5);
                        Character.AttackDelay = Random.Range(0.3f, 0.5f);
                        break;
                }
            }
        }
    }
}
