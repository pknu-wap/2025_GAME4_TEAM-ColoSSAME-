using BattleK.Scripts.Manager;
using UnityEngine;

namespace BattleK.Scripts.Debugging
{
    public class GameResultDebugger : MonoBehaviour
    {
        public AI_Manager target;
        public enum ResultType
        {
            Win,
            Lose,
            Draw
            
        }
        [Header("테스트 예정인 결과")]
        public ResultType selectedResult;

        public void TriggerSelectedResult()
        {
            switch (selectedResult)
            {
                case ResultType.Win:
                    target.KillEnemies();
                    break;
                case ResultType.Lose:
                    target.KillPlayers();
                    break;
                case ResultType.Draw:
                    target.KillAll();
                    break;
            }
            target.IsWinner();
        }
    }
}
