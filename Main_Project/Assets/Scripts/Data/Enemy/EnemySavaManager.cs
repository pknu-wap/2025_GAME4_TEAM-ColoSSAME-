using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EnemySaveManager : MonoBehaviour
{
    public static EnemySaveManager Instance { get; private set; }

    private const string FileName = "EnemySave.json";

    private string saveDirectory;
    private string savePath;

    public EnemyTeamList CurrentData { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        
        this.savePath = Path.Combine(Application.persistentDataPath, FileName);

        if (!Directory.Exists(this.saveDirectory))
        {
            Directory.CreateDirectory(this.saveDirectory);
        }

        Load();
    }

    public void Load()
    {
        if (!File.Exists(this.savePath))
        {
            this.CurrentData = new EnemyTeamList();
            Save();
            return;
        }

        string json = File.ReadAllText(this.savePath);
        this.CurrentData = JsonUtility.FromJson<EnemyTeamList>(json);

        if (this.CurrentData == null)
        {
            this.CurrentData = new EnemyTeamList();
        }

        if (this.CurrentData.teams == null)
        {
            this.CurrentData.teams = new List<EnemyTeam>();
        }
    }

    public void Save()
    {
        if (this.CurrentData == null)
        {
            this.CurrentData = new EnemyTeamList();
        }

        if (this.CurrentData.teams == null)
        {
            this.CurrentData.teams = new List<EnemyTeam>();
        }

        string json = JsonUtility.ToJson(this.CurrentData, true);
        File.WriteAllText(this.savePath, json);
    }

    public EnemyTeam GetTeam(int id)
    {
        if (this.CurrentData == null)
        {
            Load();
        }

        return this.CurrentData.teams.Find(team => team.id == id);
    }

    public bool HasTeam(int id)
    {
        if (this.CurrentData == null)
        {
            Load();
        }

        return this.CurrentData.teams.Exists(team => team.id == id);
    }

    public void AddTeam(EnemyTeam team)
    {
        if (team == null)
        {
            Debug.LogWarning("[EnemySaveManager] team이 null입니다.");
            return;
        }

        if (HasTeam(team.id))
        {
            Debug.Log($"[EnemySaveManager] 이미 존재하는 팀입니다: {team.id} / {team.name}");
            return;
        }

        this.CurrentData.teams.Add(team);
        Save();
    }

    public void SaveTeam(EnemyTeam updatedTeam)
    {
        if (updatedTeam == null)
        {
            return;
        }

        if (this.CurrentData == null)
        {
            Load();
        }

        int index = this.CurrentData.teams.FindIndex(team => team.id == updatedTeam.id);

        if (index < 0)
        {
            this.CurrentData.teams.Add(updatedTeam);
        }
        else
        {
            this.CurrentData.teams[index] = updatedTeam;
        }

        Save();
    }

    public void Clear()
    {
        this.CurrentData = new EnemyTeamList();
        Save();
    }
}