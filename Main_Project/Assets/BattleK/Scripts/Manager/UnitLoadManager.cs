using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// 절대 경로의 UserSave.json을 "읽기" 전용으로 로드하는 헬퍼.
/// - UserManager, UserSaveManager와 독립적으로 동작
/// - 디스크의 JSON → User 객체로 역직렬화
/// - 컬렉션(null) 보정 및 기본 유효성 점검
/// </summary>
public class UnitLoadManager : MonoBehaviour
{
    [Header("절대 경로(파일까지 포함). 예) C:\\Users\\User\\AppData\\LocalLow\\DefaultCompany\\StateLearning\\UserSave.json")]
    [Tooltip("비워두면 Application.persistentDataPath/UserSave.json을 사용합니다.")]
    [SerializeField] private string absolutePath = 
        @"C:\Users\User\AppData\LocalLow\DefaultCompany\StateLearning\UserSave.json";

    [Header("시작 시 자동으로 로드할지 여부")]
    [SerializeField] private bool loadOnAwake = true;

    /// <summary>마지막으로 성공적으로 로드된 User 데이터</summary>
    public User LoadedUser { get; private set; }

    /// <summary>로드 성공 시 발생 (LoadedUser가 유효)</summary>
    public event Action<User> OnUserLoaded;

    /// <summary>마지막 로드/오류 메시지</summary>
    public string LastMessage { get; private set; }

    // JSON 역직렬화 옵션: 누락 필드 무시 + Null 포함(스키마 변동 내성)
    private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
    {
        MissingMemberHandling = MissingMemberHandling.Ignore,
        NullValueHandling     = NullValueHandling.Include,
        Formatting            = Formatting.Indented
    };

    private void Awake()
    {
        if (string.IsNullOrWhiteSpace(absolutePath))
        {
            // 절대경로를 지정하지 않으면, 기본적으로 persistentDataPath/UserSave.json 사용
            absolutePath = Path.Combine(Application.persistentDataPath, "UserSave.json");
        }

        Debug.Log($"[UnitLoadManager] Target Path: {absolutePath}");

        if (loadOnAwake)
        {
            if (LoadFromAbsolutePath(out string msg))
                Debug.Log($"[UnitLoadManager] 로드 성공: {msg}");
            else
                Debug.LogWarning($"[UnitLoadManager] 로드 실패: {msg}");
        }
    }

    /// <summary>
    /// 인스펙터에서 절대 경로를 바꿨을 때, 런타임에서도 반영하고 싶을 경우 호출
    /// </summary>
    public void SetAbsolutePath(string newAbsolutePath)
    {
        absolutePath = newAbsolutePath;
        Debug.Log($"[UnitLoadManager] 경로 변경: {absolutePath}");
    }

    /// <summary>
    /// 현재 설정된 absolutePath에서 UserSave.json 읽기 (UserManager 미사용)
    /// </summary>
    public bool LoadFromAbsolutePath(out string message)
    {
        return LoadUserFromFile(absolutePath, out message);
    }

    /// <summary>
    /// 지정 경로에서 UserSave.json 읽기 (정적 헬퍼로도 사용 가능)
    /// </summary>
    public bool LoadUserFromFile(string filePath, out string message)
    {
        LastMessage = message = string.Empty;

        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                message = "파일 경로가 비어 있습니다.";
                LastMessage = message;
                LoadedUser = null;
                return false;
            }

            if (!File.Exists(filePath))
            {
                message = $"세이브 파일이 존재하지 않습니다.\n→ {filePath}";
                LastMessage = message;
                LoadedUser = null;
                return false;
            }

            string json = File.ReadAllText(filePath);
            var data = JsonConvert.DeserializeObject<User>(json, jsonSettings);
            if (data == null)
            {
                message = "역직렬화 결과가 null입니다. JSON 내용/스키마를 확인하세요.";
                LastMessage = message;
                LoadedUser = null;
                return false;
            }

            // 컬렉션 결측 보정
            if (data.inventory == null) data.inventory = new Dictionary<string, int>();
            if (data.myUnits   == null) data.myUnits   = new List<Unit>();

            // 간단 유효성 체크(원하면 더 추가)
            if (string.IsNullOrWhiteSpace(data.userName))
            {
                // userName이 비어도 치명적 에러는 아님—경고만
                Debug.LogWarning("[UnitLoadManager] 경고: userName이 비어 있습니다.");
            }

            LoadedUser = data;
            message = $"User:{data.userName}, Lv:{data.level}, Gold:{data.money}, Units:{data.myUnits.Count}, Aetherius:{data.myUnits.Find(u => u.unitId == "Astra_Aetherius").level}";
            LastMessage = message;

            OnUserLoaded?.Invoke(LoadedUser);
            return true;
        }
        catch (Exception ex)
        {
            message = $"예외 발생: {ex.Message}";
            LastMessage = message;
            LoadedUser = null;
            return false;
        }
    }

    // ===== 유틸: LoadedUser 확인용 간단 액세서 =====

    public bool HasUser() => LoadedUser != null;

    public IReadOnlyList<Unit> GetUnitsOrEmpty()
        => LoadedUser?.myUnits ?? (IReadOnlyList<Unit>)Array.Empty<Unit>();

    public int GetGoldOrZero()
        => LoadedUser?.money ?? 0;

    public string GetUserNameOrEmpty()
        => LoadedUser?.userName ?? string.Empty;

    public bool TryGetItemCount(string itemName, out int count)
    {
        count = 0;
        if (LoadedUser?.inventory == null) return false;
        return LoadedUser.inventory.TryGetValue(itemName, out count);
    }
}
