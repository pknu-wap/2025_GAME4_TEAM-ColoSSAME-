using BattleK.Scripts.Data;
using BattleK.Scripts.Data.Stat;

namespace BattleK.Scripts.Manager
{
    public static class UnitBaseStatProvider
    {
        public static UnitBaseStat Get(string unitId, bool isPlayer)
        {
            var family = UnitDataManager.Instance.GetCharacterData(unitId);
            if (family == null) return null;

            var savedUnit = isPlayer ? FindUserUnit(unitId) : FindEnemyUnit(unitId);
            return UnitBaseStat.FromFamilyAndSave(family, savedUnit);
        }

        private static Unit FindUserUnit(string unitId)
        {
            var myUnits = UserManager.Instance?.user?.myUnits;
            return myUnits?.Find(u => u.Id == unitId);
        }

        private static Unit FindEnemyUnit(string unitId)
        {
            var league = LeagueManager.Instance?.league;
            if (league == null) return null;

            var team = EnemySaveManager.Instance?.GetTeam(league.currentEnemyTeamId);
            return team?.units?.Find(u => u.Id == unitId);
        }
    }
}