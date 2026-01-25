using System.IO;
using UnityEngine;
using BattleK.Scripts.JSON;

namespace BattleK.Scripts.Manager
{
    public class UnitLoadManager : MonoBehaviour
    {
        [Header("Absolute Path")] [SerializeField]
        private string _absolutePath = "";
        [SerializeField] private bool _loadOnAwake = true;

        public User LoadedUser { get; private set; }

        private string SavePath => string.IsNullOrWhiteSpace(_absolutePath)
            ? Path.Combine(Application.persistentDataPath, "UserSave.json")
            : _absolutePath;
        

        private void Awake()
        {
            if (!_loadOnAwake) return;
            TryLoad(out var message);
            if(LoadedUser == null) Debug.LogWarning($"[UnitLoadManager] Load Failed → {_absolutePath} Reason: {message}");
        }

        public bool TryLoad(out string message)
        {
            if (JsonFileHandler.TryLoadJsonFile<User>(SavePath, out var user, out message))
            {
                UserDefaults.Ensure(user);
                LoadedUser = user;
                return true;
            }
            
            LoadedUser = null;
            return false;
        }
    }
}