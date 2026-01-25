using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BattleK.Scripts.JSON;
using Newtonsoft.Json;
using UnityEngine;

namespace BattleK.Scripts.Data
{
    [DefaultExecutionOrder(-100)]
    public class PlayerCharacterSaveManager : MonoBehaviour
    {
        public static PlayerCharacterSaveManager Instance { get; private set; }

        private string _savePath;
        private readonly Dictionary<string, CharacterRecord> _recordMap = new();
    
        private bool _isDirty;
        private Coroutine _saveCoroutine;

        [Tooltip("데이터 변경 후 저장까지 대기 시간 (초)")]
        public float saveDebounceTime = 0.5f;

        [Serializable]
        private class SaveWrapper
        {
            public List<CharacterRecord> characters = new();
        }

        private void Awake()
        {
            if (Instance && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _savePath = Path.Combine(Application.persistentDataPath, "PlayerCharacter.json");
        
            Load();
        }

        private void OnDisable() => SaveImmediate();
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) SaveImmediate();
        }
        private void Load()
        {
            _recordMap.Clear();
            if (JsonFileHandler.TryLoadJsonFile<SaveWrapper>(_savePath, out var wrapper, out var message))
            {
                if (wrapper?.characters == null) return;
                foreach (var record in wrapper.characters.Where(r => !string.IsNullOrEmpty(r.characterKey)))
                {
                    _recordMap.TryAdd(record.characterKey, record);
                }
            }
            else
            {
                if (message != "File does not exist.")
                {
                    Debug.LogWarning($"[SaveManager] Load Failed: {message}");
                }
            }
        }
        public void SaveImmediate()
        {
            if (!_isDirty) return;

            try
            {
                var wrapper = new SaveWrapper { characters = _recordMap.Values.ToList() };
                
                var json = JsonConvert.SerializeObject(wrapper, Formatting.Indented);
            
                var dir = Path.GetDirectoryName(_savePath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir!);

                File.WriteAllText(_savePath, json);
                _isDirty = false;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Save Error: {e.Message}");
            }
        }

        private void RequestSave()
        {
            _isDirty = true;
            if (_saveCoroutine != null) StopCoroutine(_saveCoroutine);
            _saveCoroutine = StartCoroutine(CoDebounceSave());
        }

        private IEnumerator CoDebounceSave()
        {
            yield return new WaitForSecondsRealtime(saveDebounceTime);
            SaveImmediate();
            _saveCoroutine = null;
        }

        public CharacterRecord GetRecord(string characterKey)
        {
            if (string.IsNullOrEmpty(characterKey)) return null;

            if (_recordMap.TryGetValue(characterKey, out var record)) return record;
            record = new CharacterRecord { characterKey = characterKey };
            _recordMap.Add(characterKey, record);
            RequestSave();
            return record;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="characterKey">캐릭터 키를 입력하는 변수</param>
        /// <param name="action"></param>
        public void ModifyRecord(string characterKey, Action<CharacterRecord> action)
        {
            var record = GetRecord(characterKey);
            if (record == null) return;
            action(record);
            RequestSave();
        }
    }
}