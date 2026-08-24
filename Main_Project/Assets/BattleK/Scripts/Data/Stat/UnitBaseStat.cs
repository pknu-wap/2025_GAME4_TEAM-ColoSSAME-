using BattleK.Scripts.Data.ClassInfo;

namespace BattleK.Scripts.Data.Stat
{
    public class UnitBaseStat
    {
        public string UnitId, UnitName;
        public int Level, Rarity;

        public int BaseAtk, BaseDef, BaseHp, BaseAgi;
        public float BaseEvasionRate;
        public float BaseAttackSpeed;

        public int BaseSkillPoint;
        public float BaseMoveSpeed;
        public float BaseAttackDelay;

        public InjuryStatus CurrentInjury;
        
        public static UnitBaseStat FromUnitStat(UnitStat stat, int level, int rarity, int baseAgi, string unitId = null)
        {
            return new UnitBaseStat
            {
                UnitId = unitId ?? stat.Name,
                UnitName = stat.Name,
                Level = level,
                Rarity = rarity,
                BaseAtk = stat.AttackDamage,
                BaseDef = stat.Defense,
                BaseHp = stat.MaxHP,
                BaseAgi = baseAgi,
                BaseEvasionRate = stat.EvasionRate,
                BaseAttackSpeed = stat.AttackSpeed,
                BaseSkillPoint = stat.SkillPoint,
                BaseMoveSpeed = stat.MoveSpeed,
                BaseAttackDelay = stat.AttackDelay,
                CurrentInjury = stat.InjuryLevel
            };
        }
    }
}