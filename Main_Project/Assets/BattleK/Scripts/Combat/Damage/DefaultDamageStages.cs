using System.Collections.Generic;
using BattleK.Scripts.Combat.Damage.StageRule;
using BattleK.Scripts.Combat.Damage.Stages;

namespace BattleK.Scripts.Combat.Damage
{
    public static class DefaultDamageStages
    {
        public static List<IDamageStage> Create()
        {
            var diminishing = new DiminishingMitigationRule();
            var rules = new Dictionary<DamageType, IMitigationRule>
            {
                { DamageType.Physical, diminishing },
                { DamageType.Magical, diminishing },
                { DamageType.True, new NoMitigationRule() }
            };

            return new List<IDamageStage>
            {
                new ValidateStage(),
                new InvincibleStage(),
                new EvasionStage(),
                new MitigationStage(rules, diminishing),
                new ApplyHpStage(),
                new KillStage()
            };
        }
    }
}