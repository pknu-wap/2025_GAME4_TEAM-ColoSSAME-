using BattleK.Scripts.Data.Type.AIDataType.CC;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base
{
    [CreateAssetMenu(fileName = "StatusProfileSO", menuName = "BattleK/StatusProfileSO")]
    public class StatusProfileSO : ScriptableObject
    {
        [Header("Status Settings")] public StatusData Data;
    }
}
