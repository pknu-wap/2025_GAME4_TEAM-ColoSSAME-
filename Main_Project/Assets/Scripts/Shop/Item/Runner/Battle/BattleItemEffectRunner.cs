using System.Collections.Generic;
using BattleK.Scripts.AI;
using BattleK.Scripts.Manager;
using UnityEngine;

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
        if (!battleStartApplied) return;

        RegisterEquippedItemsDuringBattle();
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
            context.StatApplier.RestoreOriginalStats();
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
        context.StatApplier.RestoreOriginalStats();
        ClearRuntimeState();
    }

    public bool TryReviveOnDeath(StaticAICore owner)
    {
        return battleStartApplied && dispatcher.TryReviveOnDeath(owner);
    }

    public void NotifyUnitDeath(StaticAICore deadUnit, StaticAICore killer = null)
    {
        if (!battleStartApplied) return;

        dispatcher.NotifyUnitDeath(deadUnit, killer);
    }

    public void NotifyUnitKill(StaticAICore killer, StaticAICore victim)
    {
        if (!battleStartApplied) return;

        dispatcher.NotifyUnitKill(killer, victim);
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

    private void RegisterEquippedItemsDuringBattle()
    {
        if (!aiManager)
        {
            aiManager = AI_Manager.Instance;
            context.SetAiManager(aiManager);
        }

        if (!aiManager) return;

        RegisterUnits(aiManager.playerUnits);
        RegisterUnits(aiManager.enemyUnits);
    }

    private void ClearRuntimeState()
    {
        registry.Clear();
        context.StatApplier.ClearSnapshots();
        battleStartApplied = false;
    }
}
