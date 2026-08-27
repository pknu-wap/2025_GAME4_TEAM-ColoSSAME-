using BattleK.Scripts.Data.ClassInfo;

namespace BattleK.Scripts.Data.Stat
{
    public readonly struct FinalStat
    {
        public readonly int MaxHp;
        public readonly int CurrentHp;
        public readonly int AttackDamage;
        public readonly int Defense;
        public readonly int SkillPoint;
        public readonly float MoveSpeed;
        public readonly float EvasionRate;
        public readonly float AttackSpeed;
        public readonly float AttackDelay;

        public FinalStat(
            int maxHp,
            int currentHp,
            int attackDamage,
            int defense,
            int skillPoint,
            float moveSpeed,
            float evasionRate,
            float attackSpeed,
            float attackDelay)
        {
            MaxHp = maxHp;
            CurrentHp = currentHp;
            AttackDamage = attackDamage;
            Defense = defense;
            SkillPoint = skillPoint;
            MoveSpeed = moveSpeed;
            EvasionRate = evasionRate;
            AttackSpeed = attackSpeed;
            AttackDelay = attackDelay;
        }

        public void ApplyTo(UnitStat stat)
        {
            stat.MaxHP = MaxHp;
            stat.CurrentHP = CurrentHp;
            stat.AttackDamage = AttackDamage;
            stat.Defense = Defense;
            stat.SkillPoint = SkillPoint;
            stat.MoveSpeed = MoveSpeed;
            stat.EvasionRate = EvasionRate;
            stat.AttackSpeed = AttackSpeed;
            stat.AttackDelay = AttackDelay;
        }
    }
}