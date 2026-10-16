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
    private CharacterData incoming;

    private GameObject Root => root != null ? root : gameObject;

    private void Awake()
    {
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        cancelButton.onClick.AddListener(OnCancelButtonClicked);
        Loc.Changed += RefreshMessage;
    }

    private void OnDestroy()
    {
        confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
        cancelButton.onClick.RemoveListener(OnCancelButtonClicked);
        characterList.OnCharacterSelected -= HandleCharacterSelected;
        Loc.Changed -= RefreshMessage;
    }

    public void Show(CharacterData incoming, Action<string> onConfirm, Action onCancel)
    {
        this.onConfirm = onConfirm;
        this.onCancel = onCancel;
        this.incoming = incoming;
        selectedUnitId = null;

        Root.SetActive(true);

        characterList.OnCharacterSelected -= HandleCharacterSelected;
        characterList.OnCharacterSelected += HandleCharacterSelected;
        characterList.CoroutineHost = this;
        characterList.ClearSelection();
        characterList.Refresh();

        confirmButton.interactable = false;

        RefreshMessage();
    }

    private void RefreshMessage()
    {
        if (messageText == null) return;
        string unitName = incoming != null
            ? Loc.UnitName(incoming.Unit_ID, incoming.Unit_Name)
            : Loc.Get("recruitreplace_default_name");
        messageText.SetText(Loc.Get("recruitreplace_message", unitName));
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
