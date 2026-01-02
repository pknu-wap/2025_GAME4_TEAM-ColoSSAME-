using System.IO;
using UnityEngine;
using BattleK.Scripts.JSON;

namespace BattleK.Scripts.Manager
{
    public class UnitLoadManager : MonoBehaviour
    {
        [Header("Absolute Path")]
        [SerializeField] private string _absolutePath = @"";

        [Header("Is Load on Awake")]
        [SerializeField] private bool _loadOnAwake = true;

        public User LoadedUser { get; private set; }

        #region Base

        private void Awake()
        {
            if (string.IsNullOrWhiteSpace(_absolutePath))
                _absolutePath = Path.Combine(Application.persistentDataPath, "UserSave.json");

            if (!_loadOnAwake) return;
            if (!TryLoad(out var message))
                Debug.LogWarning($"[UnitLoadManager] Load Failed → {_absolutePath} Reason: {message}");
        }

        #endregion

        #region Load

        public bool TryLoad(out string message)
        {
            var ok = JsonFileLoader.TryLoadJsonFile<User>(_absolutePath, out var user, out message);

            if (!ok)
            {
                LoadedUser = null;
                return false;
            }

            UserDefaults.Ensure(user);

            LoadedUser = user;
            return true;
        }

        #endregion
    }
}