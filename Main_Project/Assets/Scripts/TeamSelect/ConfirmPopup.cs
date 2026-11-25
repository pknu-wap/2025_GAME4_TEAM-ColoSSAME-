using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ConfirmPopup : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TMP_Text familyNameText;
    [SerializeField] private RawImage crestImage;
    [SerializeField] private TMP_Text warningText;

    private Action onConfirm;
    private GameObject returnFocus;

    private void Awake()
    {
        confirmButton.onClick.AddListener(HandleConfirm);
        cancelButton.onClick.AddListener(Hide);
    }

    public void Show(string familyName, int crestSlot, Action confirm)
    {
        if (root.activeSelf) return;
        familyNameText.text = Loc.Get("command_greeting", familyName);
        messageText.text = Loc.Get("command_brief");
        warningText.text = Loc.Get("teamselect_confirm_warn");
        crestImage.enabled = crestSlot >= 0 && crestSlot < 10;
        if (crestImage.enabled)
            crestImage.uvRect = new Rect((crestSlot % 5) / 5f, 0.5f - (crestSlot / 5) * 0.5f, 0.2f, 0.5f);

        onConfirm = confirm;
        returnFocus = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        root.SetActive(true);
        confirmButton.Select();
    }

    public void Hide()
    {
        onConfirm = null;
        root.SetActive(false);
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(returnFocus);
    }

    private void HandleConfirm()
    {
        var action = onConfirm;
        if (action == null) return;
        onConfirm = null;
        cancelButton.interactable = false;
        confirmButton.interactable = false;
        action.Invoke();
    }
}
