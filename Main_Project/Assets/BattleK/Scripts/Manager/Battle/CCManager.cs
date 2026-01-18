using System.Collections;
using System.Collections.Generic;
using BattleK.Scripts.AI;
using BattleK.Scripts.Data.Type.AIDataType.CC;
using UnityEngine;

namespace BattleK.Scripts.Manager.Battle
{
    public class CCManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private StaticAICore _aiCore;
        private readonly Dictionary<CCType, Coroutine> _activeCCs = new();

        public void ApplyCC(CCData data)
        {
            if(_activeCCs.ContainsKey(data.ccType))
            {
                if (_activeCCs[data.ccType] != null)
                {
                    StopCoroutine(_activeCCs[data.ccType]);
                    CleanupCC(data);
                }
                _activeCCs.Remove(data.ccType);
            }
            var newRoutine = StartCoroutine(ProcessCCRoutine(data));
            _activeCCs.Add(data.ccType, newRoutine);
        }

        private IEnumerator ProcessCCRoutine(CCData data)
        {
            if(data.isHardCC) _aiCore.EnterCCState(data.duration, data.animName);
            if(!Mathf.Approximately(data.speedMultiplier, 1.0f)) _aiCore.SetMoveSpeedMultiplier(data.speedMultiplier);
            GameObject vfxInstance = null;
            if(data.vfxPrefab) vfxInstance = Instantiate(data.vfxPrefab, transform.position, Quaternion.identity, transform);

            var timer = 0f;
            var dotTimer = 0f;

            while (timer < data.duration)
            {
                var deltaTime = Time.deltaTime;
                timer += deltaTime;

                if (data.isDoT)
                {
                    dotTimer += deltaTime;
                    if (!(dotTimer >= data.tickInterval)) continue;
                    ApplyDamage(data.damagePerTick);
                    dotTimer = 0f;
                }
                yield return null;
            }
            
            CleanupCC(data);
            if(vfxInstance) Destroy(vfxInstance);
            if(_activeCCs.ContainsKey(data.ccType)) _activeCCs.Remove(data.ccType);
        }

        private void CleanupCC(CCData data)
        {
            if(!Mathf.Approximately(data.speedMultiplier, 1.0f)) _aiCore.ResetMoveSpeed();
            if(data.isHardCC) _aiCore.ExitCCState();
        }

        private void ApplyDamage(float amount)
        {
            Debug.Log($"<color=red>DoT 데미지: {amount}</color>");
        }
        
    }
}
