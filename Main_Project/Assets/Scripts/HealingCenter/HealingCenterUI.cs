using UnityEngine;

namespace Colosseum.HealingCenter
{
    public class HealingCenterUI : MonoBehaviour
    {
        [SerializeField] private HealingCharacterList characterList;
        [SerializeField] private HealingCharacterDetail characterDetail;

        private void Awake()
        {
            characterList.OnCharacterSelected += characterDetail.ShowCharacter;
            characterDetail.OnHealRequested += HandleHealRequested;
        }

        private void OnDestroy()
        {
            characterList.OnCharacterSelected -= characterDetail.ShowCharacter;
            characterDetail.OnHealRequested -= HandleHealRequested;
        }

        private void OnEnable()
        {
            characterList.Refresh();
            characterDetail.Clear();
        }

        private void HandleHealRequested(string unitId)
        {
            HealingResult result = HealingService.Instance.TryHeal(unitId);

            if (!result.IsSuccess)
            {
                Debug.Log($"[HealingCenterUI] {result.GetMessage()}");
                // TODO: 실제 토스트/팝업 매니저가 있다면 연동
                // 예: ToastManager.Instance.Show(result.GetMessage());
                return;
            }

            // 치유 완료: 좌측(HP 갱신) + 우측(상태 갱신) 모두 다시 그린다.
            characterList.Refresh();
            characterDetail.Refresh();
        }
    }
}
