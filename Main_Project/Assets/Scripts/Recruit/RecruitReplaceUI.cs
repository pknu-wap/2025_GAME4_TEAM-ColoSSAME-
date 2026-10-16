using System;
using BattleK.Scripts.Data.Stat;
using Colosseum.Character;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecruitReplaceUI : MonoBehaviour
{
    [Header("팝업 루트 (비워두면 이 오브젝트)")]
    [SerializeField] private GameObject root;

    [Header("참조")]
    [SerializeField] private CharacterList characterList;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    [Header("Optional - 안내 문구")]
    [SerializeField] private TMP_Text messageText;

    private Action<string> onConfirm;
    private Action onCancel;
    private string selectedUnitId;

    private GameObject Root => root != null ? root : gameObject;

    private void Awake()
    {
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        cancelButton.onClick.AddListener(OnCancelButtonClicked);
    }

    private void OnDestroy()
    {
        confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
        cancelButton.onClick.RemoveListener(OnCancelButtonClicked);
        characterList.OnCharacterSelected -= HandleCharacterSelected;
    }

    public void Show(CharacterData incoming, Action<string> onConfirm, Action onCancel)
    {
        this.onConfirm = onConfirm;
        this.onCancel = onCancel;
        selectedUnitId = null;

        Root.SetActive(true);

        characterList.OnCharacterSelected -= HandleCharacterSelected;
        characterList.OnCharacterSelected += HandleCharacterSelected;
        characterList.CoroutineHost = this;
        characterList.ClearSelection();
        characterList.Refresh();

        confirmButton.interactable = false;

        if (messageText != null)
        {
            string unitName = incoming != null ? incoming.Unit_Name : "검투사";
            messageText.SetText($"{unitName} 영입: 교체할 검투사를 선택하세요. 교체된 검투사의 레벨·스킬·등급은 사라집니다");
        }
    }

    public void Hide()
    {
        Release();
        Root.SetActive(false);
    }

    public void Release()
    {
        onConfirm = null;
        onCancel = null;
        selectedUnitId = null;

        confirmButton.interactable = false;
        characterList.OnCharacterSelected -= HandleCharacterSelected;
    }

    private void HandleCharacterSelected(Unit unit)
    {
        selectedUnitId = unit?.Id;

        confirmButton.interactable = !string.IsNullOrEmpty(selectedUnitId);
    }

    private void OnConfirmButtonClicked()
    {
        if (string.IsNullOrEmpty(selectedUnitId))
        {
            return;
        }

        Action<string> callback = onConfirm;
        string unitId = selectedUnitId;

        Hide();
        callback?.Invoke(unitId);
    }

    private void OnCancelButtonClicked()
    {
        Action callback = onCancel;

        Hide();
        callback?.Invoke();
    }
}
