using System.Collections;
using BattleK.Scripts.Data.Stat;
using BattleK.UI.Book;
using Colosseum.Character;
using UnityEngine;

namespace Colosseum.HealingCenter
{
    public class HealingCenterUI : MonoBehaviour
    {
        [SerializeField] private CharacterList characterList;
        [SerializeField] private CharacterDetail characterDetail;
        [SerializeField] private TextToastUI toastUI;
        [SerializeField] private BookPage page;

        private void Awake()
        {
            if (page == null)
                page = GetComponent<BookPage>();
            if (page != null)
                page.Opened += HandlePageOpened;

            characterList.CoroutineHost = this;
            characterDetail.CoroutineHost = this;

            characterList.OnCharacterSelected += characterDetail.ShowCharacter;
            characterDetail.OnHealRequested += HandleHealRequested;
        }

        private void OnDestroy()
        {
            if (page != null)
                page.Opened -= HandlePageOpened;

            characterList.OnCharacterSelected -= characterDetail.ShowCharacter;
            characterDetail.OnHealRequested -= HandleHealRequested;
        }

        private void OnEnable()
        {
            StartCoroutine(RefreshNextFrame());
        }

        private void HandlePageOpened()
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
