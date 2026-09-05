using BattleK.Scripts.Data.ClassInfo;

namespace BattleK.Scripts.Data.Stat
{
    public readonly struct FinalStat
    {
        public readonly int MaxHp;
        public readonly int CurrentHp;
        public readonly int AttackDamage;
        public readonly int Defense;
        public readonly float MoveSpeed;
        public readonly float EvasionRate;
        public readonly float AttackSpeed;
        public readonly float AttackDelay;
        public readonly InjuryStatus CurrentInjury;

        public FinalStat(
            int maxHp,
            int currentHp,
            int attackDamage,
            int defense,
            float moveSpeed,
            float evasionRate,
            float attackSpeed,
            float attackDelay,
            InjuryStatus currentInjury)
        {
            MaxHp = maxHp;
            CurrentHp = currentHp;
            AttackDamage = attackDamage;
            Defense = defense;
            MoveSpeed = moveSpeed;
            EvasionRate = evasionRate;
            AttackSpeed = attackSpeed;
            AttackDelay = attackDelay;
            CurrentInjury = currentInjury;
        }

        public void ApplyTo(UnitRuntimeStat runtimeStat)
        {
            runtimeStat.MaxHP = MaxHp;
            runtimeStat.CurrentHP = CurrentHp;
            runtimeStat.AttackDamage = AttackDamage;
            runtimeStat.Defense = Defense;
            runtimeStat.MoveSpeed = MoveSpeed;
            runtimeStat.EvasionRate = EvasionRate;
            runtimeStat.AttackSpeed = AttackSpeed;
            runtimeStat.AttackDelay = AttackDelay;
            runtimeStat.InjuryLevel = CurrentInjury;
        }
    }
}