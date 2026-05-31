using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class OfflineProgressPopupView : MonoBehaviour
{
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private Image dimOverlay;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI kpiRewardText;
    [SerializeField] private TextMeshProUGUI xpRewardText;
    [SerializeField] private Button claimButton;

    public static bool IsShowing { get; private set; }

    private void Start()
    {
        if (claimButton != null)
            claimButton.onClick.AddListener(Hide);
    }

    public void Show(OfflineProgressResult result)
    {
        if (!result.shouldShowPopup) return;

        titleText.text = L("LOC_0049", "ПОКА ВАС НЕ БЫЛО");
        descriptionText.text = L("LOC_0050", "Офис сделал вид, что работал.");

        bool showKpi = result.earnedKpi > 0;
        bool showXp = result.earnedXp > 0;

        kpiRewardText.gameObject.SetActive(showKpi);
        if (showKpi)
            kpiRewardText.text = $"+{NumberFormatter.Format(result.earnedKpi)} KPI";

        xpRewardText.gameObject.SetActive(showXp);
        if (showXp)
            xpRewardText.text = $"+{NumberFormatter.Format(result.earnedXp)} XP";

        IsShowing = true;
        gameObject.SetActive(true);
        StartCoroutine(FadeIn());
    }

    private static string L(string key, string fallback)
    {
        return LocalizationManager.Instance != null ? LocalizationManager.Instance.Get(key) : fallback;
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
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / 0.3f);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        while (elapsed < 0.2f)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / 0.2f);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        IsShowing = false;
        gameObject.SetActive(false);
    }
}
