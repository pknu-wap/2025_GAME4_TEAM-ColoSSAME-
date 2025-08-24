using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Battle/Enemy Strategy/Strategy Set", fileName = "EnemyStrategySet")]
public class EnemyStrategySet : ScriptableObject
{
    [Serializable]
    public struct WeightedStrategy
    {
        public EnemyStrategyBase strategy;
        [Min(0f)] public float weight;
    }

    public List<WeightedStrategy> strategies = new();

    public EnemyStrategyBase PickRandom()
    {
        float sum = 0f;
        foreach (var w in strategies) sum += Mathf.Max(0f, w.weight);

        if (sum <= 0f)
        {
            foreach (var w in strategies) if (w.strategy != null) return w.strategy;
            return null;
        }

        float t = UnityEngine.Random.value * sum;
        foreach (var w in strategies)
        {
            float ww = Mathf.Max(0f, w.weight);
            if (t <= ww) return w.strategy;
            t -= ww;
        }
        return null;
    }
}