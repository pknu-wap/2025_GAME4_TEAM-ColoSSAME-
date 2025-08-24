using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(menuName="Battle/Character Address Book")]
public class CharacterAddressBook : ScriptableObject
{
    [Serializable]
    public class Entry {
        public string characterKey;                       // UIDrag.characterKey와 동일
        public AssetReferenceGameObject prefabReference;  // Addressable 프리팹
    }

    public List<Entry> entries = new();

    public bool TryGet(string key, out AssetReferenceGameObject ar)
    {
        key = (key ?? "").Trim();
        foreach (var e in entries)
        {
            if (string.Equals(e.characterKey?.Trim(), key, StringComparison.Ordinal)) {
                ar = e.prefabReference; return true;
            }
        }
        ar = null; return false;
    }
}