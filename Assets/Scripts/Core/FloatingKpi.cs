// FloatingKpi.cs
// Version: 2026-01-12_v1

using UnityEngine;
using TMPro;

public class FloatingKpi : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float lifetime = 0.8f;
    [SerializeField, Min(1f)] private float normalTextWidth = 260f;

    [Header("Normal")]
    [SerializeField] private Color normalColor = new Color(1f, 0.68f, 0.22f, 1f);
    [SerializeField, Min(0f)] private float normalFloatDistance = 42f;
    [SerializeField, Range(0f, 1f)] private float normalFadeStart = 0.45f;

    [Header("Critical")]
    [SerializeField] private Color criticalColor = new Color(1f, 0.9f, 0.28f, 1f);
    [SerializeField, Min(1f)] private float criticalTextWidth = 420f;
    [SerializeField, Min(1f)] private float criticalFontSizeMultiplier = 1.5f;
    [SerializeField, Min(0f)] private float criticalLifetime = 0.85f;
    [SerializeField, Min(0f)] private float criticalFloatDistance = 58f;
    [SerializeField, Min(0f)] private float criticalPopDuration = 0.16f;
    [SerializeField, Min(0f)] private float criticalPopStartScale = 0.8f;
    [SerializeField, Min(1f)] private float criticalPopPeakScale = 1.25f;
    [SerializeField, Range(0f, 1f)] private float criticalFadeStart = 0.55f;
    [SerializeField] private Color criticalOutlineColor = new Color(0.12f, 0.07f, 0f, 1f);
    [SerializeField, Range(0f, 1f)] private float criticalOutlineWidth = 0.18f;

    private RectTransform rectTransform;
    private float timer;
    private float duration;
    private float elapsed;
    private bool isCritical;
    private Color activeColor;
    private Color normalOutlineColor;
    private float normalOutlineWidth;
    private float normalFontSize;
    private FontStyles normalFontStyle;
    private RectTransform textRectTransform;
    private Vector2 normalTextSize;
    private Vector3 normalScale;
    private Vector2 startPosition;
    private Vector2 targetPosition;

    private void Awake()
    {
        rectTransform = (RectTransform)transform;
        normalScale = transform.localScale;

        if (text != null)
        {
            textRectTransform = text.rectTransform;
            normalTextSize = textRectTransform.sizeDelta;
            normalFontSize = text.fontSize;
            normalFontStyle = text.fontStyle;
            normalOutlineColor = text.outlineColor;
            normalOutlineWidth = text.outlineWidth;
        }
    }

    public void Init(int amount)
    {
        Init(amount, false);
    }

    public void Init(int amount, bool isCritical)
    {
        this.isCritical = isCritical;

        duration = isCritical ? criticalLifetime : lifetime;
        timer = duration;
        elapsed = 0f;

        float floatDistance = isCritical ? criticalFloatDistance : normalFloatDistance;
        startPosition = rectTransform.anchoredPosition;
        targetPosition = startPosition + new Vector2(0f, floatDistance);

        transform.localScale = isCritical && criticalPopDuration > 0f
            ? normalScale * criticalPopStartScale
            : normalScale;

        if (text == null)
            return;

        text.text = $"+{NumberFormatter.Format(amount)} KPI";
        ApplyTextStyle(isCritical);
        ApplySingleLineLayout(isCritical);
        SetTextAlpha(1f);
    }

    private void ApplyTextStyle(bool critical)
    {
        if (text == null)
            return;

        activeColor = critical ? criticalColor : normalColor;
        text.color = activeColor;
        text.fontSize = critical ? normalFontSize * criticalFontSizeMultiplier : normalFontSize;
        text.fontStyle = critical ? normalFontStyle | FontStyles.Bold : normalFontStyle;

        text.outlineWidth = critical ? criticalOutlineWidth : normalOutlineWidth;
        text.outlineColor = critical ? criticalOutlineColor : normalOutlineColor;
    }

    private void ApplySingleLineLayout(bool critical)
    {
        if (text == null)
            return;

        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Overflow;

        if (textRectTransform == null)
            return;

        Vector2 size = normalTextSize;
        size.x = critical ? criticalTextWidth : normalTextWidth;
        textRectTransform.sizeDelta = size;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        UpdatePosition();
        UpdateScale();
        UpdateFade();

        timer -= Time.deltaTime;
        if (timer <= 0f)
            gameObject.SetActive(false);
    }

    private void UpdatePosition()
    {
        if (duration <= 0f)
            return;

        float progress = Mathf.Clamp01(elapsed / duration);
        float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
        rectTransform.anchoredPosition = Vector2.Lerp(
            startPosition,
            targetPosition,
            easedProgress
        );
    }

    private void UpdateScale()
    {
        if (!isCritical || criticalPopDuration <= 0f)
        {
            transform.localScale = normalScale;
            return;
        }

        if (elapsed >= criticalPopDuration)
        {
            transform.localScale = normalScale;
            return;
        }

        float progress = Mathf.Clamp01(elapsed / criticalPopDuration);
        float scale = progress < 0.5f
            ? Mathf.Lerp(criticalPopStartScale, criticalPopPeakScale, progress / 0.5f)
            : Mathf.Lerp(criticalPopPeakScale, 1f, (progress - 0.5f) / 0.5f);

        transform.localScale = normalScale * scale;
    }

    private void UpdateFade()
    {
        float fadeStart = isCritical ? criticalFadeStart : normalFadeStart;
        float progress = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;

        if (progress <= fadeStart)
        {
            SetTextAlpha(1f);
            return;
        }

        float fadeProgress = Mathf.InverseLerp(fadeStart, 1f, progress);
        SetTextAlpha(1f - fadeProgress);
    }

    private void SetTextAlpha(float alpha)
    {
        if (text == null)
            return;

        Color textColor = activeColor;
        textColor.a = alpha;
        text.color = textColor;

        Color outlineColor = isCritical ? criticalOutlineColor : normalOutlineColor;
        outlineColor.a *= alpha;
        text.outlineColor = outlineColor;
    }
}
