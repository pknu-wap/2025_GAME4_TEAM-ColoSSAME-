using System;
using System.Collections;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class UserSaveManager : MonoBehaviour
{
    [SerializeField] private float _saveDebounceTime = 0.5f;
    
    private string _fileName = "UserSave.json";
    private string _savePath;
    private User _pendingData;
    private bool _isDirty;
    private Coroutine _saveCoroutine;

    void Awake()
    {
        _savePath = Path.Combine(Application.persistentDataPath, _fileName);
    }

    public void SaveUserImmediate(User data)
    {
        _pendingData = data;
        FlushSave();
    }
    
    public void SaveUser(User data)
    {
        _pendingData = data;
        _isDirty = true;
        if(_saveCoroutine != null) StopCoroutine(_saveCoroutine);
        _saveCoroutine = StartCoroutine(CoDebounceSave());
    }

    private IEnumerator CoDebounceSave()
    {
        yield return new WaitForSecondsRealtime(_saveDebounceTime);
        FlushSave();
        _saveCoroutine = null;
    }

    private void FlushSave()
    {
        if (!_isDirty || _pendingData == null) return;
        var json = JsonConvert.SerializeObject(_pendingData, Formatting.Indented);
        File.WriteAllText(_savePath, json);
        _isDirty = false;
    }

    public User LoadUser()
    {
        if (!File.Exists(_savePath))
        {
            return null;
        }
        var json = File.ReadAllText(_savePath);
        return JsonConvert.DeserializeObject<User>(json);
    }

    private void OnDisable() => FlushSave();
    private void OnApplicationPause(bool pause) { if(pause) FlushSave(); }
}