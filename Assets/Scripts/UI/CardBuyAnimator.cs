using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CardBuyAnimator : MonoBehaviour
{
    [Header("Card References")]
    [SerializeField] private RectTransform cardRect;
    [SerializeField] private Image cardBackground;
    [SerializeField] private Image priceButtonImage;
    [SerializeField] private TextMeshProUGUI priceText;

    [Header("Flyout")]
    [SerializeField] private GameObject flyoutPrefab; // создадим через код
    [SerializeField] private RectTransform canvasRoot;

    [Header("Colors")]
    [SerializeField] private Color flashColorSuccess = new Color(1f, 0.85f, 0.2f, 0.6f);
    [SerializeField] private Color flashColorError   = new Color(1f, 0.2f, 0.2f, 0.7f);
    [SerializeField] private Color flyoutColor       = new Color(0.9f, 0.6f, 0.1f, 1f);

    private Color originalCardColor;
    private Color originalPriceColor;
    private bool isAnimating = false;

    private void Start()
    {
        if (cardBackground != null)
            originalCardColor = cardBackground.color;
        if (priceButtonImage != null)
            originalPriceColor = priceButtonImage.color;
    }

    // === SUCCESS ANIMATION ===
    public void PlayBuyAnimation(int cost)
    {
        if (isAnimating) return;
        StartCoroutine(BuySequence(cost));
    }

    private IEnumerator BuySequence(int cost)
    {
        isAnimating = true;

        // Press
        yield return StartCoroutine(ScaleTo(0.96f, 0.06f));

        // Запускаем флеш и flyout параллельно с bounce
        StartCoroutine(FlashCard(flashColorSuccess));
        SpawnFlyout($"-{NumberFormatter.Format(cost)} KPI");

        // Bounce (пока флеш уже идёт)
        yield return StartCoroutine(ScaleTo(1.05f, 0.10f));
        yield return StartCoroutine(ScaleTo(1.00f, 0.08f));

        isAnimating = false;
    }

    // === ERROR ANIMATION ===
    public void PlayErrorAnimation()
    {
        StartCoroutine(ErrorSequence());
    }

    private IEnumerator ErrorSequence()
    {
        StartCoroutine(FlashImage(cardBackground, flashColorError, originalCardColor, 0.3f));
        yield return StartCoroutine(ShakeCard());
    }

    // === SCALE ===
    private IEnumerator ScaleTo(float targetScale, float duration)
    {
        Vector3 startScale = cardRect.localScale;
        Vector3 endScale   = Vector3.one * targetScale;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            cardRect.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }
        cardRect.localScale = endScale;
    }

    // === SHAKE ===
    private IEnumerator ShakeCard()
    {
        Vector3 originalPos = cardRect.anchoredPosition3D;
        float duration  = 0.18f;
        float magnitude = 8f;
        float elapsed   = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float x = Mathf.Sin(t * Mathf.PI * 6f) * magnitude * (1f - t);
            cardRect.anchoredPosition3D = originalPos + new Vector3(x, 0, 0);
            yield return null;
        }
        cardRect.anchoredPosition3D = originalPos;
    }

    // === FLASH CARD BORDER ===
    private IEnumerator FlashCard(Color flashColor)
    {
        if (cardBackground == null) yield break;
        yield return StartCoroutine(
            FlashImage(cardBackground, flashColor, originalCardColor, 0.3f));
    }

    private IEnumerator FlashImage(Image img, Color to, Color from, float duration)
    {
        img.color = to;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            img.color = Color.Lerp(to, from, elapsed / duration);
            yield return null;
        }
        img.color = from;
    }

    // === FLYOUT TEXT ===
    private void SpawnFlyout(string text)
    {
        RectTransform parent = canvasRoot != null ? canvasRoot : cardRect;

        GameObject go = new GameObject("KpiFlyout");
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300f, 50f);

        // Переводим центр карточки в координаты Canvas
        Vector2 cardCenter = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent,
            RectTransformUtility.WorldToScreenPoint(null, cardRect.position),
            null,
            out cardCenter);

        rt.anchoredPosition = cardCenter + new Vector2(0f, cardRect.rect.height * 0.5f);

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = 22;
        tmp.color     = flyoutColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;

        StartCoroutine(AnimateFlyout(rt, tmp));
    }

    private IEnumerator AnimateFlyout(RectTransform rt, TextMeshProUGUI tmp)
    {
        float duration = 0.8f;
        float elapsed  = 0f;
        Vector2 startPos = rt.anchoredPosition;
        Vector2 endPos   = startPos + new Vector2(0f, 60f);
        Color startColor = tmp.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            Color c = startColor;
            c.a = Mathf.Lerp(1f, 0f, t);
            tmp.color = c;
            yield return null;
        }
        Destroy(rt.gameObject);
    }

}
