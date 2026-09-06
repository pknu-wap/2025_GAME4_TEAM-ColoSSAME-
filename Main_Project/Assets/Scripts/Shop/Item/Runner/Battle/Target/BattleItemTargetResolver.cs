using System.Collections.Generic;
using BattleK.Scripts.AI;
using BattleK.Scripts.Manager;
using Shop.Item;
using UnityEngine;

internal sealed class BattleItemTargetResolver
{
    private AI_Manager aiManager;

    public void SetAiManager(AI_Manager source)
    {
        aiManager = source;
    }

    public IEnumerable<StaticAICore> GetTargets(StaticAICore owner, ItemEffectTarget target)
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

    public bool IsSameTeam(StaticAICore a, StaticAICore b)
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

    private IEnumerable<StaticAICore> GetTeamUnits(StaticAICore owner)
    {
        if (TryGetManagerSide(owner, out List<StaticAICore> teamUnits, out _))
        {
            for (int i = 0; i < teamUnits.Count; i++)
            {
                yield return teamUnits[i];
            }

            yield break;
        }

        StaticAICore[] allUnits = UnityEngine.Object.FindObjectsOfType<StaticAICore>();
        for (int i = 0; i < allUnits.Length; i++)
        {
            if (allUnits[i] && allUnits[i].gameObject.layer == owner.gameObject.layer)
                yield return allUnits[i];
        }
    }

    private IEnumerable<StaticAICore> GetEnemyUnits(StaticAICore owner)
    {
        if (TryGetManagerSide(owner, out _, out List<StaticAICore> enemyUnits))
        {
            for (int i = 0; i < enemyUnits.Count; i++)
            {
                yield return enemyUnits[i];
            }

            yield break;
        }

        StaticAICore[] allUnits = UnityEngine.Object.FindObjectsOfType<StaticAICore>();
        for (int i = 0; i < allUnits.Length; i++)
        {
            if (allUnits[i] && allUnits[i].gameObject.layer != owner.gameObject.layer)
                yield return allUnits[i];
        }
    }

    private bool TryGetManagerSide(
        StaticAICore owner,
        out List<StaticAICore> teamUnits,
        out List<StaticAICore> enemyUnits)
    {
        teamUnits = null;
        enemyUnits = null;

        if (!aiManager) return false;

        if (aiManager.playerUnits.Contains(owner))
        {
            teamUnits = aiManager.playerUnits;
            enemyUnits = aiManager.enemyUnits;
            return true;
        }

        if (aiManager.enemyUnits.Contains(owner))
        {
            teamUnits = aiManager.enemyUnits;
            enemyUnits = aiManager.playerUnits;
            return true;
        }

        return false;
    }
}
