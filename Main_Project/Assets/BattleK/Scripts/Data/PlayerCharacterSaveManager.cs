using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

// (선택) 가능한 한 일찍 초기화하고 싶다면
[DefaultExecutionOrder(-100)]
public class PlayerCharacterSaveManager : MonoBehaviour
{
    public static PlayerCharacterSaveManager Instance { get; private set; }

    private string SavePath => Path.Combine(Application.persistentDataPath, "PlayerCharacter.json");

    public PlayerCharacters Data { get; private set; } = new PlayerCharacters();

    private bool _dirty = false;
    private Coroutine _debounceCo = null;

    [Tooltip("타겟 변경 후 몇 초 뒤 저장할지")]
    public float saveDebounceSeconds = 0.25f;

    private void Awake()
    {
        // 🔸 씬마다 1개만 유지(중복 방지)
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[SaveManager] Duplicate in scene. Destroying this one.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // ❌ DontDestroyOnLoad 제거: 씬 전환 시 파기되어도 OK (우리는 디스크에 저장함)
        LoadFromDisk();
        // 경로 확인이 필요하면 한 번 찍어두세요
        // Debug.Log($"[SaveManager] persistentDataPath = {Application.persistentDataPath}");
    }

    private void OnDisable()
    {
        // 🔸 씬이 언로드되기 전에 디바운스 대기분을 즉시 저장(유실 방지)
        FlushPendingSaves();
        if (Instance == this) Instance = null;
    }

    private void OnDestroy()
    {
        // 🔸 파괴 시에도 한 번 더 안전 저장
        FlushPendingSaves();
        if (Instance == this) Instance = null;
    }

    private void FlushPendingSaves()
    {
        if (_dirty) SaveToDiskImmediate();
    }

    public void LoadFromDisk()
    {
        if (!File.Exists(SavePath))
        {
            Data = new PlayerCharacters { characters = new List<CharacterRecord>() };
            return;
        }

        string json = File.ReadAllText(SavePath);
        Data = JsonUtility.FromJson<PlayerCharacters>(json);
        if (Data == null) Data = new PlayerCharacters { characters = new List<CharacterRecord>() };
    }

    public void SaveToDiskImmediate()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SavePath));
        string json = JsonUtility.ToJson(Data, true);
        File.WriteAllText(SavePath, json);
        _dirty = false;
        // Debug.Log($"[SaveManager] Saved: {SavePath}");
    }

    public void MarkDirtyAndDebounceSave()
    {
        _dirty = true;
        if (_debounceCo != null) StopCoroutine(_debounceCo);
        _debounceCo = StartCoroutine(DebounceSave());
    }

    private IEnumerator DebounceSave()
    {
        yield return new WaitForSecondsRealtime(saveDebounceSeconds);
        if (_dirty) SaveToDiskImmediate();
        _debounceCo = null;
    }

    public CharacterRecord GetOrCreate(string characterKey)
    {
        var rec = Data.characters.FirstOrDefault(c => c.characterKey == characterKey);
        if (rec == null)
        {
            rec = new CharacterRecord { characterKey = characterKey };
            Data.characters.Add(rec);
            MarkDirtyAndDebounceSave();
        }
        return rec;
    }

    public void UpdateTargets(string characterKey, string target1, string target2)
    {
        var rec = GetOrCreate(characterKey);
        rec.target1 = target1;
        rec.target2 = target2;
        MarkDirtyAndDebounceSave();
    }

    public void UpdateStats(string characterKey, CharacterRecord newValues)
    {
        var rec = GetOrCreate(characterKey);
        rec.hp = newValues.hp;
        rec.def = newValues.def;
        rec.moveSpeed = newValues.moveSpeed;
        rec.attackDamage = newValues.attackDamage;
        rec.attackSpeed = newValues.attackSpeed;
        rec.attackRange = newValues.attackRange;
        rec.sightRange = newValues.sightRange;
        rec.evasionRate = newValues.evasionRate;
        rec.skillRange = newValues.skillRange;
        rec.skillDelay = newValues.skillDelay;
        rec.unitClass = newValues.unitClass;
        MarkDirtyAndDebounceSave();
    }
    
    public void UpdateTargetClasses(string characterKey, List<UnitClass> targets)
    {
        var rec = GetOrCreate(characterKey);
        rec.targetClasses = targets ?? new List<UnitClass>();
        MarkDirtyAndDebounceSave();
    }

}
