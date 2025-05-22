using System.Collections.Generic;
using Battle.Scripts.Ai.Weapon;
using Battle.Scripts.Value.HpBar;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Battle.Scripts.Ai
{
    public class AddPrefab : MonoBehaviour
    {
        public string HpPrefabAddress = "Prefabs/HealthBar/";
        public string WeaponPrefabAddress = "Prefabs/Weapon/";

        private Dictionary<string, Transform> weaponCache = new();

        public void LoadHpPrefab(BattleAI battleAI)
        {
            string fullAddress = HpPrefabAddress + gameObject.tag;

            // ✅ 기존 HP바 제거
            if (battleAI.HealthBar != null)
            {
                Destroy(battleAI.HealthBar);
                battleAI.HealthBar = null;
                Debug.Log($"[{battleAI.name}] 기존 HealthBar 제거 완료");
            }

            Addressables.LoadAssetAsync<GameObject>(fullAddress).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    GameObject prefab = handle.Result;
                    GameObject instance = Instantiate(prefab, battleAI.transform);

                    instance.name = "HealthBar";

                    // 위치 설정
                    Vector3 pos = instance.transform.localPosition;
                    pos.y -= 1.2f;
                    instance.transform.localPosition = pos;

                    // ✅ BattleAI에 연결
                    battleAI.HealthBar = instance;

                    // ✅ CharacterValue에도 HealthBar 컴포넌트 연결
                    var characterValue = battleAI.GetComponent<CharacterValue>();
                    if (characterValue != null)
                    {
                        var hpComponent = instance.GetComponent<HealthBar>();
                        if (hpComponent != null)
                        {
                            characterValue.healthBar = hpComponent;
                            Debug.Log($"[{battleAI.name}] CharacterValue에 HealthBar 컴포넌트 재연결 완료");
                        }
                        else
                        {
                            Debug.LogWarning($"[{battleAI.name}] 인스턴스에서 HealthBar 컴포넌트를 찾을 수 없습니다.");
                        }
                    }

                    Debug.Log($"[{battleAI.name}] HealthBar 생성 및 연결 완료: {instance.name}");
                }
                else
                {
                    Debug.LogError($"[{battleAI.name}] HP 프리팹 로드 실패: {fullAddress}");
                }
            };
        }


        public void AddWeapon(BattleAI battleAI)
        {
            string weaponName = battleAI.weaponType.ToString();
            string fullAddress = WeaponPrefabAddress + weaponName;

            // ✅ 이전 무기 제거
            if (battleAI.Weapon != null)
            {
                Destroy(battleAI.Weapon);
                Debug.Log($"[{battleAI.name}] 이전 무기 제거 완료");
            }

            // ✅ 참조 초기화
            battleAI.Weapon = null;
            battleAI.weaponTrigger = null;
            battleAI.arrowWeaponTrigger = null;
            battleAI.arrowWeaponTrigger = null;
            weaponCache.Remove(weaponName);

            // ✅ Addressable 로드
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

                    // ✅ 연결 및 캐시
                    battleAI.Weapon = instance;
                    weaponCache[weaponName] = instance.transform;

                    // ✅ 무기 타입에 따라 스크립트 연결
                    if (battleAI.weaponType is WeaponType.Bow or WeaponType.Magic)
                    {
                        battleAI.arrowWeaponTrigger = instance.GetComponent<ArrowWeapon>();
                        if (battleAI.arrowWeaponTrigger != null)
                        {
                            battleAI.arrowWeaponTrigger.Initialize(battleAI, instance);
                            Debug.Log($"[{battleAI.name}] 원거리 무기 초기화 완료");
                        }
                    }
                    else
                    {
                        battleAI.weaponTrigger = instance.GetComponent<WeaponTrigger>();
                        if (battleAI.weaponTrigger != null)
                        {
                            battleAI.weaponTrigger.Initialize(battleAI);
                            Debug.Log($"[{battleAI.name}] 근접 무기 초기화 완료");
                        }
                    }

                    Debug.Log($"[{battleAI.name}] 무기 연결 완료: {weaponName}");
                }
                else
                {
                    Debug.LogError($"[{battleAI.name}] 무기 프리팹 로드 실패: {fullAddress}");
                }
            };
        }
    }
}
