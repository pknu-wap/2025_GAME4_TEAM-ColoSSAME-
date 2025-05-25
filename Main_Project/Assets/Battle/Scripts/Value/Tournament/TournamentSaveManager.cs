using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class TournamentSaveManager : MonoBehaviour
{
    private string fileName = "TournamentSave.json";
    private string savePath => Path.Combine(Application.persistentDataPath, fileName);

    public void SaveTournament(TournamentData data)
    {
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(savePath, json);
        Debug.Log($"✅ 토너먼트 저장 완료: {savePath}");
    }

    public TournamentData LoadTournament()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            TournamentData data = JsonConvert.DeserializeObject<TournamentData>(json);
            Debug.Log("✅ 토너먼트 불러오기 완료");
            return data;
        }
        Debug.LogWarning("❌ 토너먼트 저장 파일이 없습니다.");
        return new TournamentData(); // 빈 값 반환
    }
}