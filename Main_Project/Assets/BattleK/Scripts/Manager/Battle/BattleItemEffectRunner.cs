using System.Collections.Generic;
using BattleK.Scripts.AI;
using BattleK.Scripts.Data.ClassInfo;
using BattleK.Scripts.Data.Type.AIDataType.CC;
using UnityEngine;

namespace BattleK.Scripts.Manager.Battle
{
    [DisallowMultipleComponent]
    public class BattleItemEffectRunner : MonoBehaviour
    {
        public static BattleItemEffectRunner Instance { get; private set; }

        [Header("Manager")]
        [SerializeField] private AI_Manager aiManager;

        private readonly List<ItemEffectRuntime> effects = new();
        private readonly Dictionary<StaticAICore, List<ItemEffectRuntime>> effectsByOwner = new();
        private readonly Dictionary<StaticAICore, UnitStatSnapshot> statSnapshots = new();
        private bool battleStartApplied;

        public static BattleItemEffectRunner EnsureInstance()
        {
            if (Instance) return Instance;

            GameObject runnerObject = new GameObject(nameof(BattleItemEffectRunner));
            return runnerObject.AddComponent<BattleItemEffectRunner>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            if (!aiManager) aiManager = AI_Manager.Instance;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            if (!battleStartApplied) return;

            UpdateTickEffects(Time.deltaTime);
            UpdateLowHpEffects();
        }

        public void StartBattle(AI_Manager source)
        {
            if (battleStartApplied) return;

            BindFromAiManager(source);
            ApplyBattleStartEffects();
        }

        public void BindFromAiManager(AI_Manager source)
        {
            if (battleStartApplied)
            {
                RestoreOriginalStats();
            }

            aiManager = source ? source : AI_Manager.Instance;
            effects.Clear();
            effectsByOwner.Clear();
            statSnapshots.Clear();
            battleStartApplied = false;

            if (!aiManager) return;

            RegisterUnits(aiManager.playerUnits);
            RegisterUnits(aiManager.enemyUnits);
        }

        public void RegisterUnit(StaticAICore unit)
        {
            if (!unit || unit.Stat == null || unit.Stat.Item == null) return;

            ItemData item = unit.Stat.Item;
            if (!item.HasEffectDomain(ItemEffectDomain.Battle)) return;

            if (!statSnapshots.ContainsKey(unit))
            {
                statSnapshots.Add(unit, new UnitStatSnapshot(unit.Stat));
            }

            if (!effectsByOwner.TryGetValue(unit, out List<ItemEffectRuntime> ownerEffects))
            {
                ownerEffects = new List<ItemEffectRuntime>();
                effectsByOwner.Add(unit, ownerEffects);
            }

            foreach (ItemEffectDefinition effect in item.GetEffects(ItemEffectDomain.Battle))
            {
                if (effect == null) continue;

                ItemEffectRuntime runtime = new ItemEffectRuntime(item, effect, unit);
                effects.Add(runtime);
                ownerEffects.Add(runtime);
            }
        }

        public void ApplyBattleStartEffects()
        {
            if (battleStartApplied) return;

            battleStartApplied = true;
            ApplyTriggeredEffects(ItemEffectTrigger.BattleStart);
        }

        public void ApplyBattleEndEffects()
        {
            if (!battleStartApplied) return;

            ApplyTriggeredEffects(ItemEffectTrigger.BattleEnd);
            RestoreOriginalStats();

            effects.Clear();
            effectsByOwner.Clear();
            statSnapshots.Clear();
            battleStartApplied = false;
        }

        public bool TryReviveOnDeath(StaticAICore owner)
        {
            if (!owner || !TryGetOwnerEffects(owner, out List<ItemEffectRuntime> ownerEffects))
                return false;

            for (int i = 0; i < ownerEffects.Count; i++)
            {
                ItemEffectRuntime runtime = ownerEffects[i];
                ItemEffectDefinition effect = runtime.Effect;

                if (!IsTrigger(runtime, ItemEffectTrigger.Death)) continue;
                if (effect.effectKind != ItemEffectKind.ReviveOnce) continue;
                if (!CanTrigger(runtime, 1)) continue;

                int reviveHp = ResolveReviveHp(owner, effect);
                owner.Stat.CurrentHP = Mathf.Clamp(reviveHp, 1, owner.Stat.MaxHP);
                UpdateHpBar(owner);
                runtime.TriggerCount++;
                return true;
            }

            return false;
        }

        public void NotifyUnitDeath(StaticAICore deadUnit, StaticAICore killer = null)
        {
            if (!battleStartApplied || !deadUnit) return;

            ApplyOwnerDeathEffects(deadUnit);
            ApplyTeamDeathStackEffects(deadUnit);

            if (killer && killer != deadUnit)
            {
                NotifyUnitKill(killer, deadUnit);
            }
        }

        public void NotifyUnitKill(StaticAICore killer, StaticAICore victim)
        {
            if (!battleStartApplied || !killer || !victim) return;
            if (!TryGetOwnerEffects(killer, out List<ItemEffectRuntime> ownerEffects)) return;

            for (int i = 0; i < ownerEffects.Count; i++)
            {
                ItemEffectRuntime runtime = ownerEffects[i];
                ItemEffectDefinition effect = runtime.Effect;

                if (!IsTrigger(runtime, ItemEffectTrigger.Kill)) continue;
                if (effect.effectKind != ItemEffectKind.KillStackingStatPercentBonus) continue;
                if (!CanTrigger(runtime)) continue;

                runtime.TriggerCount++;
                ApplyStackedStatPercent(runtime);
            }
        }

        private void RegisterUnits(List<StaticAICore> units)
        {
            if (units == null) return;

            for (int i = 0; i < units.Count; i++)
            {
                RegisterUnit(units[i]);
            }
        }

        private void ApplyTriggeredEffects(ItemEffectTrigger trigger)
        {
            HashSet<StaticAICore> flatTouchedUnits = new();

            for (int i = 0; i < effects.Count; i++)
            {
                ItemEffectRuntime runtime = effects[i];
                if (!IsTrigger(runtime, trigger)) continue;
                if (runtime.Effect.effectKind != ItemEffectKind.StatFlatBonus) continue;
                if (!CanTrigger(runtime)) continue;

                foreach (StaticAICore target in GetTargets(runtime.Owner, runtime.Effect.target))
                {
                    if (ApplyFlatStat(target, runtime.Effect))
                    {
                        flatTouchedUnits.Add(target);
                    }
                }

                runtime.TriggerCount++;
            }

            foreach (StaticAICore touchedUnit in flatTouchedUnits)
            {
                if (!touchedUnit || touchedUnit.Stat == null) continue;
                touchedUnit.SetInitialStats();
                UpdateHpBar(touchedUnit);
            }

            for (int i = 0; i < effects.Count; i++)
            {
                ItemEffectRuntime runtime = effects[i];
                if (!IsTrigger(runtime, trigger)) continue;
                if (runtime.Effect.effectKind == ItemEffectKind.StatFlatBonus) continue;
                if (!CanTrigger(runtime)) continue;

                ExecuteEffect(runtime);
            }
        }

        private void ExecuteEffect(ItemEffectRuntime runtime)
        {
            switch (runtime.Effect.effectKind)
            {
                case ItemEffectKind.StatPercentBonus:
                    ApplyPercentToTargets(runtime, runtime.Effect.percentValue);
                    runtime.TriggerCount++;
                    break;

                case ItemEffectKind.PeriodicHealMaxHpPercent:
                case ItemEffectKind.LowHpStatPercentBonus:
                case ItemEffectKind.ReviveOnce:
                case ItemEffectKind.TeamDeathStackingStatPercentBonus:
                case ItemEffectKind.KillStackingStatPercentBonus:
                case ItemEffectKind.DeathExplosionMaxHpPercent:
                    break;
            }
        }

        private void UpdateTickEffects(float deltaTime)
        {
            for (int i = 0; i < effects.Count; i++)
            {
                ItemEffectRuntime runtime = effects[i];
                ItemEffectDefinition effect = runtime.Effect;

                if (!IsTrigger(runtime, ItemEffectTrigger.Tick)) continue;
                if (effect.effectKind != ItemEffectKind.PeriodicHealMaxHpPercent) continue;
                if (!CanTrigger(runtime)) continue;

                float interval = Mathf.Max(0.01f, effect.tickInterval);
                runtime.TickTimer += deltaTime;
                if (runtime.TickTimer < interval) continue;

                runtime.TickTimer = 0f;
                HealPercentOfMaxHp(runtime);
                runtime.TriggerCount++;
            }
        }

        private void UpdateLowHpEffects()
        {
            for (int i = 0; i < effects.Count; i++)
            {
                ItemEffectRuntime runtime = effects[i];
                ItemEffectDefinition effect = runtime.Effect;

                if (!IsTrigger(runtime, ItemEffectTrigger.LowHp)) continue;
                if (effect.effectKind != ItemEffectKind.LowHpStatPercentBonus) continue;
                if (!runtime.Owner || runtime.Owner.Stat == null || runtime.Owner.IsDead) continue;

                float maxHp = Mathf.Max(1, runtime.Owner.Stat.MaxHP);
                float hpRatio = runtime.Owner.Stat.CurrentHP / maxHp;
                float threshold = Mathf.Clamp01(effect.hpThresholdRatio);
                bool shouldBeActive = hpRatio <= threshold;

                if (shouldBeActive && !runtime.IsActive && CanTrigger(runtime))
                {
                    ApplyPercentToTargets(runtime, effect.percentValue);
                    runtime.IsActive = true;
                    runtime.TriggerCount++;
                }
                else if (!shouldBeActive && runtime.IsActive)
                {
                    RemovePercentFromTargets(runtime);
                    runtime.IsActive = false;
                }
            }
        }

        private void ApplyOwnerDeathEffects(StaticAICore deadUnit)
        {
            if (!TryGetOwnerEffects(deadUnit, out List<ItemEffectRuntime> ownerEffects)) return;

            for (int i = 0; i < ownerEffects.Count; i++)
            {
                ItemEffectRuntime runtime = ownerEffects[i];
                ItemEffectDefinition effect = runtime.Effect;

                if (!IsTrigger(runtime, ItemEffectTrigger.Death)) continue;
                if (effect.effectKind == ItemEffectKind.ReviveOnce) continue;
                if (!CanTrigger(runtime)) continue;

                if (effect.effectKind == ItemEffectKind.DeathExplosionMaxHpPercent)
                {
                    ApplyDeathExplosion(runtime);
                    runtime.TriggerCount++;
                }
            }
        }

        private void ApplyTeamDeathStackEffects(StaticAICore deadUnit)
        {
            for (int i = 0; i < effects.Count; i++)
            {
                ItemEffectRuntime runtime = effects[i];
                ItemEffectDefinition effect = runtime.Effect;

                if (!runtime.Owner || runtime.Owner == deadUnit || runtime.Owner.IsDead) continue;
                if (!IsSameTeam(runtime.Owner, deadUnit)) continue;
                if (!IsTrigger(runtime, ItemEffectTrigger.Death)) continue;
                if (effect.effectKind != ItemEffectKind.TeamDeathStackingStatPercentBonus) continue;
                if (!CanTrigger(runtime)) continue;

                runtime.TriggerCount++;
                ApplyStackedStatPercent(runtime);
            }
        }

        private void HealPercentOfMaxHp(ItemEffectRuntime runtime)
        {
            foreach (StaticAICore target in GetTargets(runtime.Owner, runtime.Effect.target))
            {
                if (!target || target.IsDead || target.Stat == null) continue;

                float percent = ResolvePercent(runtime.Effect);
                int healAmount = Mathf.RoundToInt(target.Stat.MaxHP * percent);
                if (healAmount <= 0) continue;

                target.OnHeal(healAmount);
            }
        }

        private void ApplyDeathExplosion(ItemEffectRuntime runtime)
        {
            StaticAICore owner = runtime.Owner;
            if (!owner || owner.Stat == null) return;

            float percent = ResolvePercent(runtime.Effect);
            int damage = Mathf.RoundToInt(owner.Stat.MaxHP * percent);
            if (damage <= 0) return;

            foreach (StaticAICore target in GetTargets(owner, ItemEffectTarget.Enemies))
            {
                if (!target || target.IsDead || target.Stat == null) continue;
                if (runtime.Effect.radius > 0f &&
                    Vector2.Distance(owner.transform.position, target.transform.position) > runtime.Effect.radius)
                    continue;

                target.OnTakeDamage(damage, owner, true);
            }
        }

        private void ApplyPercentToTargets(ItemEffectRuntime runtime, float percent)
        {
            foreach (StaticAICore target in GetTargets(runtime.Owner, runtime.Effect.target))
            {
                ApplyPercentStat(target, runtime, percent, 1);
            }
        }

        private void RemovePercentFromTargets(ItemEffectRuntime runtime)
        {
            foreach (StaticAICore target in GetTargets(runtime.Owner, runtime.Effect.target))
            {
                RemovePercentStat(target, runtime);
            }
        }

        private void ApplyStackedStatPercent(ItemEffectRuntime runtime)
        {
            foreach (StaticAICore target in GetTargets(runtime.Owner, runtime.Effect.target))
            {
                ApplyPercentStat(target, runtime, runtime.Effect.percentValue, runtime.TriggerCount);
            }
        }

        private bool ApplyFlatStat(StaticAICore target, ItemEffectDefinition effect)
        {
            if (!target || target.Stat == null) return false;

            int amount = Mathf.RoundToInt(effect.flatValue);
            if (amount == 0) return false;

            UnitStat stat = target.Stat;
            switch (effect.statType)
            {
                case ItemStatType.MaxHp:
                    stat.MaxHP += amount;
                    stat.CurrentHP = Mathf.Clamp(stat.CurrentHP + amount, 1, stat.MaxHP);
                    return true;

                case ItemStatType.Attack:
                    stat.AttackDamage += amount;
                    return true;

                case ItemStatType.Defense:
                    stat.Defense += amount;
                    return true;

                case ItemStatType.Agility:
                    stat.EvasionRate = Mathf.Clamp(stat.EvasionRate + amount * 0.03f, 0f, 0.35f);
                    stat.AttackSpeed += amount * 1.05f;
                    return true;
            }

            return false;
        }

        private void ApplyPercentStat(StaticAICore target, ItemEffectRuntime runtime, float percent, int stackCount)
        {
            if (!target || target.Stat == null) return;

            float multiplier = 1f + percent * Mathf.Max(1, stackCount);
            switch (runtime.Effect.statType)
            {
                case ItemStatType.MaxHp:
                    ApplyMaxHpPercent(target, percent * Mathf.Max(1, stackCount));
                    break;

                case ItemStatType.Attack:
                    target.SetStatMultiplier(StatusType.AttackDamageMultiplier, runtime, multiplier);
                    break;

                case ItemStatType.Defense:
                    target.SetStatMultiplier(StatusType.DefenseMultiplier, runtime, multiplier);
                    break;

                case ItemStatType.Agility:
                    target.SetStatMultiplier(StatusType.AttackSpeedMultiplier, runtime, multiplier);
                    target.SetStatMultiplier(StatusType.EvasionRateMultiplier, runtime, multiplier);
                    break;
            }
        }

        private void RemovePercentStat(StaticAICore target, ItemEffectRuntime runtime)
        {
            if (!target) return;

            switch (runtime.Effect.statType)
            {
                case ItemStatType.Attack:
                    target.RemoveStatMultiplier(StatusType.AttackDamageMultiplier, runtime);
                    break;

                case ItemStatType.Defense:
                    target.RemoveStatMultiplier(StatusType.DefenseMultiplier, runtime);
                    break;

                case ItemStatType.Agility:
                    target.RemoveStatMultiplier(StatusType.AttackSpeedMultiplier, runtime);
                    target.RemoveStatMultiplier(StatusType.EvasionRateMultiplier, runtime);
                    break;
            }
        }

        private void ApplyMaxHpPercent(StaticAICore target, float percent)
        {
            int increase = Mathf.RoundToInt(target.Stat.MaxHP * percent);
            if (increase == 0) return;

            target.Stat.MaxHP += increase;
            target.Stat.CurrentHP = Mathf.Clamp(target.Stat.CurrentHP + increase, 1, target.Stat.MaxHP);
            target.SetInitialStats();
            UpdateHpBar(target);
        }

        private IEnumerable<StaticAICore> GetTargets(StaticAICore owner, ItemEffectTarget target)
        {
            if (!owner) yield break;

            switch (target)
            {
                case ItemEffectTarget.Owner:
                    yield return owner;
                    break;

                case ItemEffectTarget.Team:
                    foreach (StaticAICore unit in GetTeamUnits(owner))
                    {
                        if (unit) yield return unit;
                    }
                    break;

                case ItemEffectTarget.Enemies:
                    foreach (StaticAICore unit in GetEnemyUnits(owner))
                    {
                        if (unit) yield return unit;
                    }
                    break;
            }
        }

        private IEnumerable<StaticAICore> GetTeamUnits(StaticAICore owner)
        {
            if (aiManager)
            {
                List<StaticAICore> units = aiManager.playerUnits.Contains(owner)
                    ? aiManager.playerUnits
                    : aiManager.enemyUnits.Contains(owner)
                        ? aiManager.enemyUnits
                        : null;

                if (units != null)
                {
                    for (int i = 0; i < units.Count; i++)
                    {
                        yield return units[i];
                    }

                    yield break;
                }
            }

            StaticAICore[] allUnits = FindObjectsOfType<StaticAICore>();
            for (int i = 0; i < allUnits.Length; i++)
            {
                if (allUnits[i] && allUnits[i].gameObject.layer == owner.gameObject.layer)
                    yield return allUnits[i];
            }
        }

        private IEnumerable<StaticAICore> GetEnemyUnits(StaticAICore owner)
        {
            if (aiManager)
            {
                List<StaticAICore> units = aiManager.playerUnits.Contains(owner)
                    ? aiManager.enemyUnits
                    : aiManager.enemyUnits.Contains(owner)
                        ? aiManager.playerUnits
                        : null;

                if (units != null)
                {
                    for (int i = 0; i < units.Count; i++)
                    {
                        yield return units[i];
                    }

                    yield break;
                }
            }

            StaticAICore[] allUnits = FindObjectsOfType<StaticAICore>();
            for (int i = 0; i < allUnits.Length; i++)
            {
                if (allUnits[i] && allUnits[i].gameObject.layer != owner.gameObject.layer)
                    yield return allUnits[i];
            }
        }

        private bool IsSameTeam(StaticAICore a, StaticAICore b)
        {
            if (!a || !b) return false;

            if (aiManager)
            {
                bool bothPlayer = aiManager.playerUnits.Contains(a) && aiManager.playerUnits.Contains(b);
                bool bothEnemy = aiManager.enemyUnits.Contains(a) && aiManager.enemyUnits.Contains(b);
                if (bothPlayer || bothEnemy) return true;
            }

            return a.gameObject.layer == b.gameObject.layer;
        }

        private bool TryGetOwnerEffects(StaticAICore owner, out List<ItemEffectRuntime> ownerEffects)
        {
            return effectsByOwner.TryGetValue(owner, out ownerEffects);
        }

        private static bool IsTrigger(ItemEffectRuntime runtime, ItemEffectTrigger trigger)
        {
            return runtime.Effect.domain == ItemEffectDomain.Battle &&
                   runtime.Effect.trigger == trigger;
        }

        private static bool CanTrigger(ItemEffectRuntime runtime, int naturalLimit = 0)
        {
            int limit = runtime.Effect.maxTriggerCount > 0
                ? runtime.Effect.maxTriggerCount
                : naturalLimit;

            return limit <= 0 || runtime.TriggerCount < limit;
        }

        private static float ResolvePercent(ItemEffectDefinition effect)
        {
            if (effect.maxPercentValue > effect.percentValue)
            {
                return Random.Range(effect.percentValue, effect.maxPercentValue);
            }

            return effect.percentValue;
        }

        private static int ResolveReviveHp(StaticAICore owner, ItemEffectDefinition effect)
        {
            if (effect.flatValue > 0f)
                return Mathf.RoundToInt(effect.flatValue);

            float percent = ResolvePercent(effect);
            if (percent > 0f)
                return Mathf.RoundToInt(owner.Stat.MaxHP * percent);

            return owner.Stat.MaxHP;
        }

        private static void UpdateHpBar(StaticAICore unit)
        {
            if (unit && unit.HPBar)
            {
                unit.HPBar.UpdateHPBar();
            }
        }

        private void RestoreOriginalStats()
        {
            foreach (KeyValuePair<StaticAICore, UnitStatSnapshot> pair in statSnapshots)
            {
                StaticAICore unit = pair.Key;
                if (!unit || unit.Stat == null) continue;

                pair.Value.Restore(unit.Stat);
                unit.SetInitialStats();
                UpdateHpBar(unit);
            }
        }

        private sealed class ItemEffectRuntime
        {
            public readonly ItemData Item;
            public readonly ItemEffectDefinition Effect;
            public readonly StaticAICore Owner;
            public float TickTimer;
            public int TriggerCount;
            public bool IsActive;

            public ItemEffectRuntime(ItemData item, ItemEffectDefinition effect, StaticAICore owner)
            {
                Item = item;
                Effect = effect;
                Owner = owner;
            }
        }

        private readonly struct UnitStatSnapshot
        {
            private readonly int maxHp;
            private readonly int currentHp;
            private readonly int attackDamage;
            private readonly int defense;
            private readonly float attackSpeed;
            private readonly float attackDelay;
            private readonly float evasionRate;
            private readonly float moveSpeed;

            public UnitStatSnapshot(UnitStat stat)
            {
                maxHp = stat.MaxHP;
                currentHp = stat.CurrentHP;
                attackDamage = stat.AttackDamage;
                defense = stat.Defense;
                attackSpeed = stat.AttackSpeed;
                attackDelay = stat.AttackDelay;
                evasionRate = stat.EvasionRate;
                moveSpeed = stat.MoveSpeed;
            }

            public void Restore(UnitStat stat)
            {
                stat.MaxHP = maxHp;
                stat.CurrentHP = Mathf.Clamp(currentHp, 0, maxHp);
                stat.AttackDamage = attackDamage;
                stat.Defense = defense;
                stat.AttackSpeed = attackSpeed;
                stat.AttackDelay = attackDelay;
                stat.EvasionRate = evasionRate;
                stat.MoveSpeed = moveSpeed;
            }
        }
    }
}
