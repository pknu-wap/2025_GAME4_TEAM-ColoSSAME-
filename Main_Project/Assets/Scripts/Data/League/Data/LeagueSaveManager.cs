using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class LeagueSaveManager : MonoBehaviour
{
    private string fileName = "LeagueSave.json";
    private string savePath;
    
    public string SavePath => savePath;


    void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, fileName);
    }

    public League LoadLeague()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            League league = JsonConvert.DeserializeObject<League>(json);
            Debug.Log("✅ 리그 데이터 로드 완료");
            return league;
        }
        else
        {
            Debug.LogWarning("❌ 저장 파일이 없습니다.");
            return null;
        }
    }

    public void SaveLeague(League league)
    {
        string json = JsonConvert.SerializeObject(league, Formatting.Indented);
        File.WriteAllText(savePath, json);
        Debug.Log("✅ 리그 데이터 저장 완료");
    }
}