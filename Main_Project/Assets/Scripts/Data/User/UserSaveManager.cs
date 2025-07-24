using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class UserSaveManager : MonoBehaviour
{
    private string fileName = "UserSave.json";
    private string savePath;

    void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, fileName);
        Debug.Log($"저장 경로: {savePath}");
    }

    public void SaveUser(User data)
    {
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(savePath, json);
        Debug.Log("✅ 유저 데이터 저장 완료");
    }

    public User LoadUser()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            User data = JsonConvert.DeserializeObject<User>(json);
            Debug.Log("✅ 유저 데이터 로드 완료");
            return data;
        }
        else
        {
            Debug.LogWarning("❌ 저장된 유저 데이터가 없습니다.");
            return null;
        }
    }
}