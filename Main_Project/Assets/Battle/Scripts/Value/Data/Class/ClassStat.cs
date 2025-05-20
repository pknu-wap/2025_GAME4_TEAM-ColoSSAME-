// JobStats.cs

using Battle.Scripts.Ai;

namespace Battle.Scripts.Value.Data.Class
{
    [System.Serializable]
    public class ClassStat
    {
        public float hp;
        public float attack;
        public float defense;
        public float moveSpeed;
        public float attackRange;
        public float attackDelay;
        public WeaponType weaponType;
        public string attackSoundPath;
    }
}