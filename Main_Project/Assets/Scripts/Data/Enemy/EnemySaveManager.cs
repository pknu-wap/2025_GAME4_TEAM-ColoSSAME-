using System.Collections.Generic;
using System.IO;
using BattleK.Scripts.JSON;
using Newtonsoft.Json;
using UnityEngine;

// 적 팀 로스터/성장 상태를 저장하는 순수 C# 싱글턴.
// MonoBehaviour가 아니라 씬 배치가 필요 없고, 처음 접근 시 자동 생성되며 절대 null이 아님.
public class EnemySaveManager
{
    private static EnemySaveManager _instance;
    public static EnemySaveManager Instance => _instance ??= new EnemySaveManager();

    private const string FileName = "EnemySave.json";

    private readonly string _savePath;
    private readonly Dictionary<int, EnemyTeam> _teamMap = new();

    private EnemySaveManager()
    {
        _savePath = Path.Combine(Application.persistentDataPath, FileName);
        Load();
    }

    private void Load()
    {
        _teamMap.Clear();

        if (JsonFileHandler.TryLoadJsonFile<EnemyTeamList>(_savePath, out var data, out var message))
        {
            if (data?.teams == null) return;

            foreach (var team in data.teams)
            {
                if (team != null) _teamMap[team.id] = team;
            }
        }
        else if (message != "File does not exist.")
        {
            // 세이브 파일이 손상돼도 크래시 없이 빈 상태로 시작
            Debug.LogWarning($"[EnemySaveManager] 로드 실패, 빈 상태로 시작: {message}");
        }
    }

    public void Save()
    {
        try
        {
            var data = new EnemyTeamList { teams = new List<EnemyTeam>(_teamMap.Values) };
            File.WriteAllText(_savePath, JsonConvert.SerializeObject(data, Formatting.Indented));
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[EnemySaveManager] 저장 실패: {e.Message}");
        }
    }

    public EnemyTeam GetTeam(int id) => _teamMap.TryGetValue(id, out var team) ? team : null;

    public bool HasTeam(int id) => _teamMap.ContainsKey(id);

    public void AddTeam(EnemyTeam team)
    {
        if (team == null)
        {
            Debug.LogWarning("[EnemySaveManager] team이 null입니다.");
            return;
        }

        if (_teamMap.ContainsKey(team.id))
        {
            Debug.Log($"[EnemySaveManager] 이미 존재하는 팀입니다: {team.id} / {team.name}");
            return;
        }

        _teamMap[team.id] = team;
        Save();
    }

    public void SaveTeam(EnemyTeam updatedTeam)
    {
        if (updatedTeam == null) return;

        _teamMap[updatedTeam.id] = updatedTeam;
        Save();
    }

    public void Clear()
    {
        _teamMap.Clear();
        Save();
    }
}
