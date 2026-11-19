using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmPopup : MonoBehaviour
{
    [SerializeField] private GameObject root;        
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [Header("가문 확정 창 (비어 있으면 문구만 표시)")]
    [SerializeField] private TMP_Text familyNameText;
    [SerializeField] private Image bannerImage;
    [SerializeField] private TMP_Text warningText;

    private Action onConfirm;
    private bool wired;

    public void Show(string message, Action onConfirm)
    {
        if (!wired)
        {
            confirmButton.onClick.AddListener(HandleConfirm);
            cancelButton.onClick.AddListener(Hide);
            wired = true;
        }

        messageText.text = message;
        this.onConfirm = onConfirm;
        root.SetActive(true);
    }

    // 가문명·깃발(지금 가운데 깃발의 그림·머티리얼)과 함께 표시
    public void Show(string familyName, Image banner, string message, Action onConfirm)
    {
        if (familyNameText != null) familyNameText.text = familyName;
        if (bannerImage != null && banner != null)
        {
            bannerImage.sprite = banner.sprite;
            bannerImage.material = banner.material;
            bannerImage.preserveAspect = true;
        }
        if (warningText != null) warningText.text = Loc.Get("teamselect_confirm_warn");
        Show(message, onConfirm);
    }

    public void Hide()
    {
        root.SetActive(false);
        onConfirm = null;
    }

    private void HandleConfirm()
    {
        var action = onConfirm;
        Hide();             
        action?.Invoke();
    }
}
