using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
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
        MissingMemberHandling   = MissingMemberHandling.Ignore,
        NullValueHandling       = NullValueHandling.Include,
        // 기존 컬렉션/사전이 있더라도 JSON 값으로 교체
        ObjectCreationHandling  = ObjectCreationHandling.Replace,
        // 포맷은 디버그용 (파일 저장은 이 매니저 책임이 아님)
        Formatting              = Formatting.Indented
    };

    private void Awake()
    {
        if (string.IsNullOrWhiteSpace(absolutePath))
            absolutePath = Path.Combine(Application.persistentDataPath, "UserSave.json");

        Debug.Log($"[UnitLoadManager] Target Path → {absolutePath}");

        if (loadOnAwake)
        {
            if (LoadFromAbsolutePath(out string msg))
                Debug.Log($"[UnitLoadManager] 로드 성공: {msg}");
            else
                Debug.LogWarning($"[UnitLoadManager] 로드 실패: {msg}");
        }
    }

    /// <summary>인스펙터에서 절대 경로를 바꿨을 때 호출</summary>
    public void SetAbsolutePath(string newAbsolutePath)
    {
        absolutePath = newAbsolutePath;
        Debug.Log($"[UnitLoadManager] 경로 변경: {absolutePath}");
    }

    /// <summary>현재 설정된 absolutePath에서 UserSave.json 읽기</summary>
    public bool LoadFromAbsolutePath(out string message)
    {
        return LoadUserFromFile(absolutePath, out message);
    }

    /// <summary>지정 경로에서 UserSave.json 읽기</summary>
    public bool LoadUserFromFile(string filePath, out string message)
    {
        LastMessage = message = string.Empty;

        try
        {
            // 1) 경로 검증
            if (string.IsNullOrWhiteSpace(filePath))
            {
                LoadedUser = null;
                return Fail("파일 경로가 비어 있습니다.", out message);
            }
            if (!File.Exists(filePath))
            {
                LoadedUser = null;
                return Fail($"세이브 파일이 존재하지 않습니다.\n→ {filePath}", out message);
            }

            // 2) 읽기 + 역직렬화
            string json = File.ReadAllText(filePath);
            var data = JsonConvert.DeserializeObject<User>(json, jsonSettings);
            if (data == null)
            {
                LoadedUser = null;
                return Fail("역직렬화 결과가 null입니다. JSON 내용/스키마를 확인하세요.", out message);
            }

            // 3) 컬렉션 결측 보정
            if (data.inventory == null) data.inventory = new Dictionary<string, int>();
            if (data.myUnits   == null) data.myUnits   = new List<Unit>();

            // 4) 간단 유효성(경고만)
            if (string.IsNullOrWhiteSpace(data.userName))
                Debug.LogWarning("[UnitLoadManager] 경고: userName이 비어 있습니다.");

            LoadedUser = data;

            // 5) 안전한 요약 메시지 구성 (하드코드된 유닛 키 접근 제거)
            var sb = new StringBuilder();
            sb.Append($"User:{data.userName ?? "(none)"}");
            sb.Append($", Lv:{data.level}");
            sb.Append($", Gold:{data.money}");
            sb.Append($", Units:{data.myUnits.Count}");

            // 원한다면 특정 유닛을 '존재하면'만 덧붙임
            // (이전처럼 .Find(...).level로 바로 접근하지 않음)
            // 예시: "Astra_Aetherius" 레벨 표시
            var aetherius = data.myUnits.Find(u => string.Equals(u.unitId, "Astra_Aetherius", StringComparison.Ordinal));
            if (aetherius != null)
                sb.Append($", Astra_Aetherius Lv:{aetherius.level}");

            message = sb.ToString();
            LastMessage = message;

            // 6) 이벤트 통지 (핸들러 예외는 여기서 잡아 로그만)
            try { OnUserLoaded?.Invoke(LoadedUser); }
            catch (Exception ex)
            {
                Debug.LogError($"[UnitLoadManager] OnUserLoaded 핸들러 예외: {ex}");
            }

            return true;
        }
        catch (Exception ex)
        {
            LoadedUser = null;
            return Fail($"예외 발생: {ex.Message}", out message);
        }
    }

    private bool Fail(string reason, out string message)
    {
        message = reason;
        LastMessage = message;
        return false;
    }

    // ===== 유틸 =====
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
