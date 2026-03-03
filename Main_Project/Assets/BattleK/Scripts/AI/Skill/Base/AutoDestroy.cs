using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base
{
    public class AutoDestroy : MonoBehaviour 
    {
        public float Duration;
        void Start() => Destroy(gameObject, Duration);
    }
}