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
    [SerializeField] private Image bannerImage;
    [SerializeField] private Image strategyKey;
    [SerializeField] private TMP_Text warningText;

    [SerializeField] private Image armoryAccents;
    [SerializeField] private Image armoryPreview;

    private Action onConfirm;
    private GameObject returnFocus;

    private void Awake()
    {
        confirmButton.onClick.AddListener(HandleConfirm);
        cancelButton.onClick.AddListener(Hide);
    }

    public void Show(string familyName, string familyId, Image banner, Color houseColor, Action confirm)
    {
        if (root.activeSelf) return;
        familyNameText.text = Loc.Get("command_greeting", familyName);
        messageText.text = Loc.Get("command_brief");
        warningText.text = Loc.Get("teamselect_confirm_warn");
        armoryPreview.sprite = armoryAccents.sprite;
        armoryPreview.material = armoryAccents.material;
        armoryPreview.color = houseColor;
        bannerImage.sprite = banner.overrideSprite;
        bannerImage.material = banner.material;
        StrategyRoomKey.Display(strategyKey, familyId);

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
