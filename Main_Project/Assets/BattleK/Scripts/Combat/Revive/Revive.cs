using BattleK.Scripts.HP;

namespace BattleK.Scripts.Combat.Revive
{
    public readonly struct ReviveResult
    {
        public readonly int Hp;
        public readonly float InvincibleSeconds;
        public readonly bool ClearStatusEffects;
        public readonly StatusVisualType Visual;

        public ReviveResult(
            int hp, float invincibleSeconds = 1f, bool clearStatusEffects = true,
            StatusVisualType visual = StatusVisualType.Normal)
        {
            Hp = hp;
            InvincibleSeconds = invincibleSeconds;
            ClearStatusEffects = clearStatusEffects;
            Visual = visual;
        }
    }
}