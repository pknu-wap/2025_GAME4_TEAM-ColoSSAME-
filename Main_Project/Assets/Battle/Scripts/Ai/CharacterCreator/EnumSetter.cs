using UnityEngine;
namespace Battle.Scripts.Ai.CharacterCreator {
	public class EnumSetter : MonoBehaviour
	{
		public BattleAI target;

		public void SetToPlayer()
		{
			if (target != null)
			{
				target.team = TeamType.Player;
				target.tag = "Player";
			}
		}

		public void SetToEnemy()
		{
			if (target != null)
			{
				target.team = TeamType.Enemy;
				target.tag = "Enemy";
			}
		}
	}
}
