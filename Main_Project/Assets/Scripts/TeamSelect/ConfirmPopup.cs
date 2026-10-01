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
