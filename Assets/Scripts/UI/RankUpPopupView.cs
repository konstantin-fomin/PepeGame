using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class RankUpPopupView : MonoBehaviour
{
    [Header("Overlay")]
    [SerializeField] private CanvasGroup overlay;

    [Header("Popup")]
    [SerializeField] private CanvasGroup popupGroup;
    [SerializeField] private RectTransform popupPanel;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI rankTransitionText;
    [SerializeField] private TextMeshProUGUI sarcasticMessageText;

    [Header("Button")]
    [SerializeField] private CanvasGroup continueButtonGroup;
    [SerializeField] private Button continueButton;

    [Header("Timing")]
    [SerializeField] private float overlayFadeDuration = 0.2f;
    [SerializeField] private float popupFadeDuration = 0.25f;
    [SerializeField] private float buttonAppearDelay = 0.6f;

    [Header("Confetti")]
    [SerializeField] private CorporateConfettiController confetti;

    public event System.Action OnPopupClosed;

    public static bool IsShowing { get; private set; }

    private bool isShowing = false;

    private void Awake()
    {
        continueButton.onClick.AddListener(OnContinueClicked);
    }

    public void Show(RankData previousRank, RankData newRank)
    {
        if (isShowing) return;
        isShowing = true;
        IsShowing = true;

        rankTransitionText.text = $"{previousRank.rankName}  →  {newRank.rankName}";
        sarcasticMessageText.text = newRank.rankUpMessage;

        gameObject.SetActive(true);

        if (confetti != null)
        {
            string newId = newRank.rankId != null ? newRank.rankId.ToLower() : "";
            if (newId == "lead" || newId == "ceo")
                confetti.PlayPromotionMedium();
            else
                confetti.PlayPromotionSmall();
        }
    }

    private void OnEnable()
    {
        if (isShowing)
            StartCoroutine(ShowSequence());
    }

    private IEnumerator ShowSequence()
    {
        overlay.alpha = 0f;
        popupGroup.alpha = 0f;
        popupPanel.localScale = Vector3.one * 0.92f;
        continueButtonGroup.alpha = 0f;
        continueButtonGroup.interactable = false;
        continueButtonGroup.blocksRaycasts = false;

        yield return StartCoroutine(FadeCanvasGroup(overlay, 0f, 1f, overlayFadeDuration));

        float elapsed = 0f;
        while (elapsed < popupFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / popupFadeDuration;
            float smooth = 1f - Mathf.Pow(1f - t, 3f);
            popupGroup.alpha = smooth;
            popupPanel.localScale = Vector3.one * Mathf.Lerp(0.92f, 1f, smooth);
            yield return null;
        }
        popupGroup.alpha = 1f;
        popupPanel.localScale = Vector3.one;

        yield return new WaitForSeconds(buttonAppearDelay);
        yield return StartCoroutine(FadeCanvasGroup(continueButtonGroup, 0f, 1f, 0.2f));
        continueButtonGroup.interactable = true;
        continueButtonGroup.blocksRaycasts = true;
    }

    private void OnContinueClicked()
    {
        StartCoroutine(HideSequence());
    }

    private IEnumerator HideSequence()
    {
        continueButtonGroup.interactable = false;

        if (confetti != null)
            confetti.Cleanup();

        if (ScreenFader.Instance != null)
            yield return StartCoroutine(ScreenFader.Instance.FadeOut(0.35f));

        IsShowing = false;
        isShowing = false;
        gameObject.SetActive(false);
        OnPopupClosed?.Invoke();
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        float elapsed = 0f;
        cg.alpha = from;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        cg.alpha = to;
    }
}
