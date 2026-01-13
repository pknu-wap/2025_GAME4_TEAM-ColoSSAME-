using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace BattleK.Scripts.Data
{
    [CreateAssetMenu(menuName="Battle/Character Address Book")]
    public class CharacterAddressBook : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public string characterKey;
            public AssetReferenceGameObject prefabReference;
        }

        public List<Entry> entries = new();

        public bool TryGet(string key, out AssetReferenceGameObject ar)
        {
            key = (key ?? "").Trim();
            foreach (var e in entries.Where(e => string.Equals(e.characterKey?.Trim(), key, StringComparison.Ordinal)))
            {
                ar = e.prefabReference;
                return true;
            }
            ar = null;
            return false;
        }
    }
}