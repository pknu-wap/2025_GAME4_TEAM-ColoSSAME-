using BattleK.Scripts.Data.Type.AIDataType.CC;
using UnityEngine;

namespace BattleK.Scripts.AI.SO.Base
{
    [CreateAssetMenu(fileName = "CCProfileSO", menuName = "BattleK/CCProfileSO")]
    public class CCProfileSO : ScriptableObject
    {
        [Header("CC Settings")] public CCData Data;
    }
}
