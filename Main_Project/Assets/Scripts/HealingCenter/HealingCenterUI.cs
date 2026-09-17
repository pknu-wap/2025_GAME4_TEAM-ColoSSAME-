using System.Collections;
using BattleK.Scripts.Data.Stat;
using UnityEngine;

namespace Colosseum.HealingCenter
{
    public class HealingCenterUI : MonoBehaviour
    {
        [SerializeField] private HealingCharacterList characterList;
        [SerializeField] private HealingCharacterDetail characterDetail;
        [SerializeField] private TextToastUI toastUI;

        private void Awake()
        {
            characterList.CoroutineHost = this;
            characterDetail.CoroutineHost = this;

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
            StartCoroutine(RefreshNextFrame());
        }

        private IEnumerator RefreshNextFrame()
        {
            yield return null;

            characterList.Refresh();

            Unit first = characterList.GetFirstUnit();
            if (first != null)
            {
                characterList.Select(first);
            }
            else
            {
                characterDetail.Clear();
            }
        }

        private void HandleHealRequested(Unit unit)
        {
            HealingResult result = HealingService.Instance.TryHeal(unit);

            if (!result.IsSuccess)
            {
                Debug.Log($"[HealingCenterUI] {result.GetMessage()}");
                if (toastUI != null)
                {
                    toastUI.Show(result.GetMessage(), 2f);
                }
                return;
            }
            characterList.RefreshSelectedSlotStatus();
            characterDetail.Refresh();
        }
    }
}
