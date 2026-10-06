using System.Collections;
using BattleK.Scripts.Data.Stat;
using Colosseum.Character;
using UnityEngine;

namespace TeamManage
{
    public class UnitListPage : MonoBehaviour
    {
        [SerializeField] private CharacterList characterList;

        private TeamManageController controller;

        private TeamManageController Controller
        {
            get
            {
                if (controller == null) controller = GetComponentInParent<TeamManageController>(true);
                return controller;
            }
        }

        private void Awake()
        {
            characterList.CoroutineHost = this;
            characterList.OnCharacterSelected += HandleCharacterSelected;
        }

        private void OnDestroy()
        {
            if (characterList != null) characterList.OnCharacterSelected -= HandleCharacterSelected;
        }

        private void OnEnable()
        {
            StartCoroutine(RefreshNextFrame());
        }

        private IEnumerator RefreshNextFrame()
        {
            yield return null;

            characterList.Refresh();

            Unit target = Controller != null ? Controller.SelectedUnit : null;
            if (target == null) target = characterList.GetFirstUnit();

            if (target != null) characterList.Select(target);
            else if (Controller != null) Controller.ClearSelection();
        }

        private void HandleCharacterSelected(Unit unit)
        {
            if (unit != null && Controller != null) Controller.SelectUnit(unit.Id);
        }
    }
}
