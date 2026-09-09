using System.Collections.Generic;
using BattleK.Scripts.AI;
using BattleK.Scripts.Manager;
using Shop.Item.Runner.Battle.Stat;
using UnityEngine;

namespace Shop.Item.Runner.Battle
{
    [DisallowMultipleComponent]
    public class BattleItemEffectRunner : MonoBehaviour
    {
        public static BattleItemEffectRunner Instance { get; private set; }

        [Header("Manager")]
        [SerializeField] private AI_Manager aiManager;

        private BattleItemEffectContext context;
        private BattleItemEffectRegistry registry;
        private BattleItemEffectDispatcher dispatcher;
        private bool battleStartApplied;
        private readonly List<StaticAICore> observedUnits = new();
        private readonly Dictionary<StaticAICore, StaticAICore> lastDamageSources = new();
        private readonly HashSet<StaticAICore> notifiedDeadUnits = new();

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
            EnsurePipeline();

            if (!aiManager) aiManager = AI_Manager.Instance;
            context.SetAiManager(aiManager);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            TryAutoStartBattle();

            if (!battleStartApplied) return;

            RegisterEquippedItemsDuringBattle();
            NotifyObservedUnitDeaths();

            if (aiManager && aiManager.IsAlreadyDone)
            {
                ApplyBattleEndEffects();
                return;
            }

            dispatcher.UpdateTickEffects(Time.deltaTime);
            dispatcher.UpdateLowHpEffects();
        }

        public void StartBattle(AI_Manager source)
        {
            if (battleStartApplied) return;

            BindFromAiManager(source);
            ApplyBattleStartEffects();
        }

        public void BindFromAiManager(AI_Manager source)
        {
            EnsurePipeline();

            if (battleStartApplied)
            {
                context.StatApplier.RemoveAllAppliedEffects();
            }

            aiManager = source ? source : AI_Manager.Instance;
            context.SetAiManager(aiManager);
            ClearRuntimeState();

            if (!aiManager) return;

            RegisterUnits(aiManager.playerUnits);
            RegisterUnits(aiManager.enemyUnits);
        }

        public void RegisterUnit(StaticAICore unit)
        {
            EnsurePipeline();
            TrackObservedUnit(unit);
            registry.RegisterUnit(unit);
        }

        public void ApplyBattleStartEffects()
        {
            if (battleStartApplied) return;

            battleStartApplied = true;
            dispatcher.Dispatch(ItemEffectTrigger.BattleStart);
        }

        public void ApplyBattleEndEffects()
        {
            if (!battleStartApplied) return;

            dispatcher.Dispatch(ItemEffectTrigger.BattleEnd);
            context.StatApplier.RemoveAllAppliedEffects();
            ClearRuntimeState();
        }

        public bool TryReviveOnDeath(StaticAICore owner)
        {
            return battleStartApplied && dispatcher.TryReviveOnDeath(owner);
        }

        public void NotifyUnitDeath(StaticAICore deadUnit, StaticAICore killer = null)
        {
            if (!battleStartApplied) return;
            if (!deadUnit || !notifiedDeadUnits.Add(deadUnit)) return;

            dispatcher.NotifyUnitDeath(deadUnit, killer);
        }

        public void NotifyUnitKill(StaticAICore killer, StaticAICore victim)
        {
            if (!battleStartApplied) return;

            dispatcher.NotifyUnitKill(killer, victim);
        }

        public void RecordDamageSource(StaticAICore target, StaticAICore attacker)
        {
            if (!target || !attacker || target == attacker) return;

            lastDamageSources[target] = attacker;
        }

        private void EnsurePipeline()
        {
            if (context != null) return;

            registry = new BattleItemEffectRegistry();
            BattleItemTargetResolver targetResolver = new BattleItemTargetResolver();
            BattleItemStatApplier statApplier = new BattleItemStatApplier();

            context = new BattleItemEffectContext(registry, targetResolver, statApplier);
            dispatcher = new BattleItemEffectDispatcher(context);
        }

        private void RegisterUnits(List<StaticAICore> units)
        {
            if (units == null) return;

            for (int i = 0; i < units.Count; i++)
            {
                RegisterUnit(units[i]);
            }
        }

        private void TrackObservedUnit(StaticAICore unit)
        {
            if (!unit || observedUnits.Contains(unit)) return;

            observedUnits.Add(unit);
        }

        private void TryAutoStartBattle()
        {
            if (battleStartApplied) return;
            if (!TryResolveAiManager()) return;
            if (aiManager.IsAlreadyDone) return;
            if (!HasInitializedUnits(aiManager.playerUnits)) return;
            if (!HasInitializedUnits(aiManager.enemyUnits)) return;

            StartBattle(aiManager);
        }

        private bool TryResolveAiManager()
        {
            if (aiManager) return true;

            aiManager = AI_Manager.Instance;
            context.SetAiManager(aiManager);
            return aiManager != null;
        }

        private static bool HasInitializedUnits(List<StaticAICore> units)
        {
            if (units == null || units.Count <= 0) return false;

            for (int i = 0; i < units.Count; i++)
            {
                StaticAICore unit = units[i];
                if (!unit || unit.runtimeStat == null || !unit.IsInitialized)
                    return false;
            }

            return true;
        }

        private void RegisterEquippedItemsDuringBattle()
        {
            if (!TryResolveAiManager()) return;

            RegisterUnits(aiManager.playerUnits);
            RegisterUnits(aiManager.enemyUnits);
        }

        private void NotifyObservedUnitDeaths()
        {
            for (int i = 0; i < observedUnits.Count; i++)
            {
                StaticAICore deadUnit = observedUnits[i];
                if (!deadUnit || !deadUnit.IsDead) continue;
                if (!notifiedDeadUnits.Add(deadUnit)) continue;

                lastDamageSources.TryGetValue(deadUnit, out StaticAICore killer);
                dispatcher.NotifyUnitDeath(deadUnit, killer);
            }
        }

        private void ClearRuntimeState()
        {
            registry.Clear();
            context.StatApplier.ClearRecords();
            observedUnits.Clear();
            lastDamageSources.Clear();
            notifiedDeadUnits.Clear();
            battleStartApplied = false;
        }
    }
}