using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Battle.Scripts.Ai
{
    public class AddPrefab : MonoBehaviour
    {
        public string HpPrefabAddress = "Prefabs/HealthBar/HealthBar1";
        public string WeaponPrefabAddress = "Prefabs/Weapon/";

        private Dictionary<string, Transform> weaponCache = new();

        public void LoadHpPrefab()
        {
            Addressables.LoadAssetAsync<GameObject>(HpPrefabAddress).Completed += OnHpPrefabLoaded;
        }

        public void AddWeapon(BattleAI battleAI)
        {
            string weaponName = battleAI.weaponType.ToString();
            string fullAddress = WeaponPrefabAddress + weaponName;

            // ✅ 캐시에 있는 무기 제거
            if (weaponCache.TryGetValue(weaponName, out var existing))
            {
                Destroy(existing.gameObject);
                weaponCache.Remove(weaponName);
                Debug.Log($"기존 무기 제거: {weaponName}");
            }

            // ✅ Addressable로 무기 로드
            Addressables.LoadAssetAsync<GameObject>(fullAddress).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    GameObject prefab = handle.Result;
                    GameObject instance = Instantiate(prefab, battleAI.transform);

                    instance.name = weaponName;
                    instance.transform.localPosition = Vector3.zero;
                    instance.transform.localRotation = Quaternion.identity;
                    instance.layer = LayerMask.NameToLayer("Hidden");

                    // ✅ 캐시 갱신
                    weaponCache[weaponName] = instance.transform;

                    Debug.Log($"무기 생성 완료: {weaponName}");
                }
                else
                {
                    Debug.LogError($"무기 프리팹 로드 실패: {fullAddress}");
                }
            };
        }

        private void OnHpPrefabLoaded(AsyncOperationHandle<GameObject> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject prefab = handle.Result;
                Instantiate(prefab, Vector3.zero, Quaternion.identity);
                Debug.Log("HP 프리팹 인스턴스 생성 완료");
            }
            else
            {
                Debug.LogError($"HP 프리팹 로드 실패: {HpPrefabAddress}");
            }
        }
    }
}