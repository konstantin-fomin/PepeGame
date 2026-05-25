using UnityEngine;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance;
    [SerializeField] private CanvasGroup canvasGroup;

    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator FadeOut(float duration = 0.3f)
    {
        canvasGroup.blocksRaycasts = true;
        yield return Fade(0f, 1f, duration);
    }

    public IEnumerator FadeIn(float duration = 0.3f)
    {
        yield return Fade(1f, 0f, duration);
        canvasGroup.blocksRaycasts = false;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        canvasGroup.alpha = from;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}
