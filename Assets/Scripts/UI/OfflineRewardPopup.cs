using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class OfflineRewardPopup : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button okButton;

    private void Start()
    {
        okButton.onClick.AddListener(Hide);
    }

    public void TryShow()
    {
        int offline = gameManager.PendingOfflineKpi;
        if (offline <= 0) return;

        messageText.text = $"Пока вас не было,\nПепе страдал без вас.\n\nНо всё же заработал:\n+{NumberFormatter.Format(offline)} KPI";

        gameObject.SetActive(true);
        StartCoroutine(FadeIn());
    }

    private void Hide()
    {
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeIn()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = true;
        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = elapsed / 0.3f;
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        while (elapsed < 0.2f)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = 1f - elapsed / 0.2f;
            yield return null;
        }
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

}
