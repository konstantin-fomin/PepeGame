using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ConfirmDialogController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private CanvasGroup canvasGroup;

    private Action onConfirm;
    private Action onCancel;

    private void Start()
    {
        confirmButton.onClick.AddListener(OnConfirm);
        cancelButton.onClick.AddListener(OnCancel);
        gameObject.SetActive(false);
    }

    public void Show(string title, string message,
        Action onConfirm, Action onCancel = null)
    {
        titleText.text   = title;
        messageText.text = message;
        this.onConfirm   = onConfirm;
        this.onCancel    = onCancel;
        gameObject.SetActive(true);
    }

    private void OnConfirm()
    {
        gameObject.SetActive(false);
        onConfirm?.Invoke();
    }

    private void OnCancel()
    {
        gameObject.SetActive(false);
        onCancel?.Invoke();
    }
}
