using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BattleK.Scripts.Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BattleK.Scripts.Manager
{
    public class AddressableAssetLoader<T> where T : UnityEngine.Object
    {
        private static readonly Dictionary<AddressableAssetType, string> PrefixMap = new()
        {
            { AddressableAssetType.Character, "Portrait" },
            { AddressableAssetType.Item, "Item" },
        };

        private readonly Dictionary<string, AsyncOperationHandle<T>> _handles = new(StringComparer.Ordinal);

        public static string BuildKey(AddressableAssetType type, string name)
        {
            if (PrefixMap.TryGetValue(type, out var prefix)) return $"{prefix}/{name}";
            Debug.LogError($"[AddressableAssetLoader] 매핑되지 않은 타입: {type}");
            return null;
        }

        public IEnumerator LoadAsync(AddressableAssetType type, string name, Action<T> onSuccess, Action onFail = null)
        {
            var key = BuildKey(type, name);
            if (string.IsNullOrWhiteSpace(key))
            {
                onFail?.Invoke();
                yield break;
            }

            yield return LoadByKeyAsync(key, onSuccess, onFail);
        }

        public IEnumerator LoadByKeyAsync(string key, Action<T> onSuccess, Action onFail = null)
        {
            if (_handles.TryGetValue(key, out var existing) && existing.IsValid())
            {
                switch (existing.Status)
                {
                    case AsyncOperationStatus.Succeeded:
                        onSuccess?.Invoke(existing.Result);
                        yield break;
                    case AsyncOperationStatus.None:
                        yield return existing;
                        break;
                    case AsyncOperationStatus.Failed:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                existing = Addressables.LoadAssetAsync<T>(key);
                _handles[key] = existing;
                yield return existing;
            }

            if (existing.Status == AsyncOperationStatus.Succeeded)
            {
                onSuccess?.Invoke(existing.Result);
            }
            else
            {
                Debug.LogWarning($"[AddressableAssetLoader] 로드 실패. Key: {key}");
                ReleaseKey(key);
                onFail?.Invoke();
            }
        }

        public void ReleaseKey(string key)
        {
            if (!_handles.TryGetValue(key, out var handle)) return;
            if (handle.IsValid()) Addressables.Release(handle);
            _handles.Remove(key);
        }

        public void ReleaseAll()
        {
            foreach (var handle in _handles.Values.Where(handle => handle.IsValid()))
            {
                Addressables.Release(handle);
            }

            _handles.Clear();
        }
    }
}