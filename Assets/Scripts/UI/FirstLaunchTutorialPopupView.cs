using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class FirstLaunchTutorialPopupView : MonoBehaviour
{
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private Image dimOverlay;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private Button startButton;

    public static bool IsShowing { get; private set; }

    public event System.Action OnPopupClosed;

    private void Start()
    {
        if (startButton != null)
            startButton.onClick.AddListener(Hide);
    }

    public void Show()
    {
        titleText.text = "ПЕРВАЯ СМЕНА";
        bodyText.text =
            "Кликайте по рабочей зоне,\nчтобы выжимать KPI.\n\n" +
            "Покупайте карточки, чтобы работать\nэффективнее и страдать продуктивнее.\n\n" +
            "XP двигает вас к следующему рангу.";

        IsShowing = true;
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
        OnPopupClosed?.Invoke();
    }
}
