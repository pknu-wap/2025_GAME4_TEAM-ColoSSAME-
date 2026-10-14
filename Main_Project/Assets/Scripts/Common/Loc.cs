using System;
using UnityEngine.Localization.Settings;

public static class Loc
{
    private const string Table = "UI";

    private static Action changed;
    private static bool hooked;

    public static event Action Changed
    {
        add
        {
            if (!hooked)
            {
                hooked = true;
                LocalizationSettings.SelectedLocaleChanged += _ => changed?.Invoke();
            }
            changed += value;
        }
        remove => changed -= value;
    }

    public static bool IsKorean
    {
        get
        {
            var locale = LocalizationSettings.SelectedLocale;
            return locale == null || locale.Identifier.Code == "ko";
        }
    }

    public static string Get(string key, params object[] args)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString(Table, key, args != null && args.Length > 0 ? args : null);
    }

    // 표에 없는 키는 fallback (세이브 데이터에 저장된 이름 등)
    public static string GetOr(string key, string fallback)
    {
        var table = LocalizationSettings.StringDatabase.GetTable(Table);
        var entry = table != null ? table.GetEntry(key) : null;
        return entry != null && !string.IsNullOrEmpty(entry.Value) ? entry.GetLocalizedString() : fallback;
    }

    public static string TeamName(Team team) => team == null ? "" : GetOr($"team_{team.fid?.ToLowerInvariant()}", team.name);

    public static string TeamDescription(Team team) => team == null ? "" : GetOr($"team_{team.fid?.ToLowerInvariant()}_desc", team.explanation);

    public static string FamilyName(string familyId, string fallback) =>
        string.IsNullOrEmpty(familyId) ? fallback : GetOr($"team_{familyId.ToLowerInvariant()}", fallback);

    public static string UnitName(string unitId, string fallback) =>
        string.IsNullOrEmpty(unitId) ? fallback : GetOr($"char_{unitId}_name", fallback);

    public static string UnitDescription(string unitId, string fallback) =>
        string.IsNullOrEmpty(unitId) ? fallback : GetOr($"char_{unitId}_desc", fallback);

    public static string ItemName(ItemData item) => item == null ? "" : GetOr($"item_{item.id}_name", item.itemName);

    public static string ItemDescription(ItemData item) => item == null ? "" : GetOr($"item_{item.id}_desc", item.description);
}
