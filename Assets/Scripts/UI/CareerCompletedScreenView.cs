using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CareerCompletedScreenView : MonoBehaviour
{
    [Header("Main View")]
    [SerializeField] private GameObject mainContent;
    [SerializeField] private Image dimOverlay;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI bodyText;

    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI kpiStatText;
    [SerializeField] private TextMeshProUGUI timeStatText;
    [SerializeField] private TextMeshProUGUI clicksStatText;

    [Header("Buttons")]
    [SerializeField] private Button stayCeoButton;
    [SerializeField] private Button newCareerButton;

    [Header("Confetti")]
    [SerializeField] private CorporateConfettiController confetti;

    [Header("Confirm View")]
    [SerializeField] private GameObject confirmContent;
    [SerializeField] private TextMeshProUGUI confirmTitleText;
    [SerializeField] private TextMeshProUGUI confirmMessageText;
    [SerializeField] private Button confirmCancelButton;
    [SerializeField] private Button confirmStartButton;

    public static bool IsShowing { get; private set; }

    public event System.Action OnStayCeo;
    public event System.Action OnNewCareerConfirmed;

    private const string VALUE_COLOR = "#4A382A";

    private void Start()
    {
        if (stayCeoButton != null)
            stayCeoButton.onClick.AddListener(HandleStayCeo);
        if (newCareerButton != null)
            newCareerButton.onClick.AddListener(ShowConfirm);
        if (confirmCancelButton != null)
            confirmCancelButton.onClick.AddListener(HideConfirm);
        if (confirmStartButton != null)
            confirmStartButton.onClick.AddListener(HandleNewCareerConfirmed);
    }

    public void Show(long totalKpiEarned, float totalPlaytimeSeconds, int totalClicks)
    {
        titleText.text = "КАРЬЕРА ЗАВЕРШЕНА";
        bodyText.text =
            "Вы достигли CEO.\n" +
            "Теперь можно ничего не понимать,\n" +
            "но уверенно согласовывать процессы.";

        kpiStatText.text = $"Итоговый KPI: <color={VALUE_COLOR}>{NumberFormatter.Format(totalKpiEarned)}</color>";
        timeStatText.text = $"Время в офисе: <color={VALUE_COLOR}>{FormatTime(totalPlaytimeSeconds)}</color>";
        clicksStatText.text = $"Кликов совершено: <color={VALUE_COLOR}>{NumberFormatter.Format(totalClicks)}</color>";

        confirmTitleText.text = "НАЧАТЬ НОВУЮ КАРЬЕРУ?";
        confirmMessageText.text =
            "Текущий прогресс будет сброшен.\n" +
            "HR обещает, что в этот раз будет иначе.";

        mainContent.SetActive(true);
        confirmContent.SetActive(false);

        IsShowing = true;
        gameObject.SetActive(true);
        StartCoroutine(FadeIn());

        if (confetti != null)
            confetti.PlayFinalCareer();
    }

    private void HandleStayCeo()
    {
        if (confetti != null) confetti.Cleanup();
        StartCoroutine(FadeOutThen(() =>
        {
            OnStayCeo?.Invoke();
        }));
    }

    private void ShowConfirm()
    {
        mainContent.SetActive(false);
        confirmContent.SetActive(true);
    }

    private void HideConfirm()
    {
        confirmContent.SetActive(false);
        mainContent.SetActive(true);
    }

    private void HandleNewCareerConfirmed()
    {
        if (confetti != null) confetti.Cleanup();
        IsShowing = false;
        gameObject.SetActive(false);
        OnNewCareerConfirmed?.Invoke();
    }

    private IEnumerator FadeIn()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = true;

        float elapsed = 0f;
        while (elapsed < 0.4f)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / 0.4f);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOutThen(System.Action callback)
    {
        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / 0.3f);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        IsShowing = false;
        gameObject.SetActive(false);
        callback?.Invoke();
    }

    private string FormatTime(float totalSeconds)
    {
        int h = (int)(totalSeconds / 3600);
        int m = (int)(totalSeconds % 3600) / 60;
        int s = (int)(totalSeconds % 60);
        if (h > 0) return $"{h}ч {m:00}м";
        return $"{m}м {s:00}с";
    }
}
