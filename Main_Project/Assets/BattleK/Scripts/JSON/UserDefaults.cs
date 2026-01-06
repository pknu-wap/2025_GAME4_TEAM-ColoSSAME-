using System.Collections.Generic;

namespace BattleK.Scripts.JSON
{
    public static class UserDefaults
    {
        public static void Ensure(User data)
        {
            if (data == null) return;

            data.inventory ??= new Dictionary<string, int>();
            data.myUnits   ??= new List<Unit>();
        }
    }
}