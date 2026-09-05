namespace BattleK.Scripts.AI.CCState
{
    public enum StatSourceCategory
    {
        Item,
        Skill,
        Buff,
        Debuff,
        Base
    }
    
    public enum FlatStatusType
    {
        MaxHpFlat,
        AttackDamageFlat,
        DefenseFlat,
        EvasionRateFlat,
        AttackSpeedFlat,
        MaxHpPercent
    }

    public readonly struct StatModifierEntry
    {
        public readonly object Source;
        public readonly StatSourceCategory Category;
        public readonly string Label;
        public readonly float Delta;

        public StatModifierEntry(object source, StatSourceCategory category, string label, float delta)
        {
            Source = source;
            Category = category;
            Label = label;
            Delta = delta;
        }
    }
}