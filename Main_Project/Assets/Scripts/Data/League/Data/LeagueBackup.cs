using System.IO;
using UnityEngine;

public static class LeagueBackup
{
    static readonly (string main, string backup)[] Files =
    {
        ("LeagueSave.json", "LeagueSave.backup.json"),
        ("EnemySave.json",  "EnemySave.backup.json"),
        ("UserSave.json",   "UserSave.backup.json"),
    };

    static string Path_(string f) => Path.Combine(Application.persistentDataPath, f);

    public static void Backup()   
    {
        foreach (var (main, bak) in Files)
            if (File.Exists(Path_(main))) File.Copy(Path_(main), Path_(bak), true);
        Debug.Log("리그 시작점 저장");
    }

    public static void Restore()  
    {
        foreach (var (main, bak) in Files)
            if (File.Exists(Path_(bak))) File.Copy(Path_(bak), Path_(main), true);
        Debug.Log("리그 시작점으로 복원");
    }

    public static bool HasBackup() => File.Exists(Path_("LeagueSave.backup.json"));
}