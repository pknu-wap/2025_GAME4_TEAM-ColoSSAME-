using UnityEngine;

namespace BattleK.Scripts.AI.Attack
{
    public interface IProjectileVelocityModifier
    {
        int Priority { get; }
        void ModifyVelocity(ref Vector2 direction, ref float speed, float deltaTime, Transform self);
    }
}