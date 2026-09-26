using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace BattleK.Scripts.Manager
{
    public class PrefabPoolManager : MonoBehaviour
    {
        public static PrefabPoolManager Instance { get; private set; }

        [SerializeField] private int _defaultCapacity = 10;
        [SerializeField] private int _maxSize = 50;

        private readonly Dictionary<GameObject, IObjectPool<GameObject>> _pools = new();
        private readonly Dictionary<GameObject, GameObject> _instanceToPrefab = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private IObjectPool<GameObject> GetOrCreatePool(GameObject prefab)
        {
            if (_pools.TryGetValue(prefab, out var existing)) return existing;

            var pool = new ObjectPool<GameObject>(
                createFunc: () => CreateInstance(prefab),
                actionOnGet: OnGetInstance,
                actionOnRelease: OnReleaseInstance,
                actionOnDestroy: Destroy,
                collectionCheck: false,
                defaultCapacity: _defaultCapacity,
                maxSize: _maxSize);

            _pools[prefab] = pool;
            return pool;
        }

        private GameObject CreateInstance(GameObject prefab)
        {
            var instance = Instantiate(prefab);
            _instanceToPrefab[instance] = prefab;
            instance.SetActive(false);
            return instance;
        }

        private static void OnGetInstance(GameObject instance)
        {
            instance.SetActive(true);
        }

        private static void OnReleaseInstance(GameObject instance)
        {
            instance.SetActive(false);
        }

        public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            var instance = Spawn(prefab.gameObject, position, rotation);
            return instance.GetComponent<T>();
        }
        
        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            var pool = GetOrCreatePool(prefab);
            var instance = pool.Get();
            instance.transform.SetPositionAndRotation(position, rotation);
            return instance;
        }

        public void Release(GameObject instance)
        {
            if (!instance) return;

            if (_instanceToPrefab.TryGetValue(instance, out var prefab) &&
                _pools.TryGetValue(prefab, out var pool))
            {
                pool.Release(instance);
            }
            else
            {
                Destroy(instance);
            }
        }
    }
}