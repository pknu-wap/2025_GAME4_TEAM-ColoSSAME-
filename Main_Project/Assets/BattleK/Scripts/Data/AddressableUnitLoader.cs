using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BattleK.Scripts.Data
{
    public class AddressableUnitLoader
    {
        private readonly Dictionary<string, GameObject> _instanceCache = new();
        private readonly Dictionary<string, AsyncOperationHandle<GameObject>> _handleCache = new();

        public IEnumerator LoadOrGetAsync(string logicalKey, AssetReferenceGameObject assetRef, Transform root, Action<GameObject> onLoaded)
        {
            if (_instanceCache.TryGetValue(logicalKey, out var cached))
            {
                onLoaded?.Invoke(cached);
                yield break;
            }

            var handle = assetRef.InstantiateAsync(root);
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                var go = handle.Result;
                go.name = logicalKey;
                _instanceCache[logicalKey] = go;
                _handleCache[logicalKey] = handle;
                onLoaded?.Invoke(go);
            }
            else
            {
                Debug.LogError($"[AddressableUnitLoader] Failed to spawn {logicalKey}");
                onLoaded?.Invoke(null);
            }
        }

        private void Release(string logicalKey)
        {
            if (!_instanceCache.TryGetValue(logicalKey, out var go)) return;
            if (go)
            {
                Addressables.ReleaseInstance(go);
            }
            _instanceCache.Remove(logicalKey);
            _handleCache.Remove(logicalKey);
        }

        public void ReleaseAll()
        {
            foreach (var key in new List<string>(_instanceCache.Keys))
                Release(key);
        }

        public bool TryGetCached(string logicalKey, out GameObject go) => _instanceCache.TryGetValue(logicalKey, out go);
    }
}