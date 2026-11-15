using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BattleK.Scripts.Combat.Damage
{
    public sealed class DamageService
    {
        public const int MaxChainDepth = 3;

        private readonly List<IDamageStage> _stages;
        private readonly CombatEvents _events;
        private readonly Queue<DamageRequest> _queue = new();
        private bool _draining;

        public DamageService(IEnumerable<IDamageStage> stages, CombatEvents events)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _stages = (stages ?? Enumerable.Empty<IDamageStage>()).OrderBy(s => s.Order).ToList();
        }

        public void Deal(in DamageRequest request)
        {
            if (!request.Target) return;
            if (request.ChainDepth > MaxChainDepth) return;

            _queue.Enqueue(request);
            if (_draining) return;

            _draining = true;
            try
            {
                while (_queue.Count > 0) Process(_queue.Dequeue());
            }
            finally
            {
                _draining = false;
            }
        }

        private void Process(DamageRequest request)
        {
            try
            {
                var ctx = new DamageContext(request);
                foreach (var stage in _stages)
                {
                    stage.Process(ctx);
                    if (ctx.Cancelled) break;
                }

                if (ctx.Ignored) return;

                var result = new DamageResult(ctx);
                _events.RaiseDamaged(result);
                if (ctx.Killed) _events.RaiseKilled(result);
                if (ctx.Revived) _events.RaiseRevived(result);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}