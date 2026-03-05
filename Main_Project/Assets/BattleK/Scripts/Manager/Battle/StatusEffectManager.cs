using System.Collections;
using System.Collections.Generic;
using BattleK.Scripts.AI;
using BattleK.Scripts.Data.Type.AIDataType.CC;
using UnityEngine;

namespace BattleK.Scripts.Manager.Battle
{
    public class StatusEffectManager : MonoBehaviour
    {
        [Header("References")]
        public StaticAICore _aiCore;
        private readonly Dictionary<StatusType, Coroutine> _activeCCs = new();

        public void ApplyCC(StatusData data)
        {
            if (_activeCCs.TryGetValue(data.StatusType, out var routine))
            {
                if (routine != null) StopCoroutine(routine);
                _activeCCs.Remove(data.StatusType);
            }
    
            _activeCCs.Add(data.StatusType, StartCoroutine(ProcessCCRoutine(data)));
        }

        private IEnumerator ProcessCCRoutine(StatusData data)
        {
            foreach (var action in data.Actions ) action.OnStart(_aiCore, data);

            var timer = 0f;
            while (timer < data.duration)
            {
                timer += Time.deltaTime;
                
                foreach(var action in data.Actions) action.OnTick(_aiCore, data);
                yield return null;
            }
            
            foreach(var action in data.Actions) action.OnEnd(_aiCore, data);
            _activeCCs.Remove(data.StatusType);
        }
    }
}
