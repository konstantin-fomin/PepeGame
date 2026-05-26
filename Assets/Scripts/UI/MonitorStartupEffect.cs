using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MonitorStartupEffect : MonoBehaviour
{
    public static MonitorStartupEffect Instance;

    [Header("References")]
    [SerializeField] private ScreenFader screenFader;
    [SerializeField] private RawImage glowLine;
    [SerializeField] private RawImage staticNoise;
    [SerializeField] private Image vignetteOverlay;

    [Header("Audio")]
    [SerializeField] private AudioManager audioManager;

    [Header("Timing")]
    [SerializeField] private float lineDuration    = 0.5f;
    [SerializeField] private float expandDuration  = 0.4f;
    [SerializeField] private float staticDuration  = 0.35f;
    [SerializeField] private float settleDuration  = 0.5f;

    private Texture2D glowTex;
    private Texture2D noiseTex;
    private int noiseWidth  = 128;
    private int noiseHeight = 128;

    private void Awake() => Instance = this;

    private void Start()
    {
        BuildGlowTexture();
        BuildNoiseTexture();
        BuildVignetteTexture();

        glowLine.gameObject.SetActive(false);
        staticNoise.gameObject.SetActive(false);
        vignetteOverlay.gameObject.SetActive(false);
    }

    public void PlayStartSound()
    {
        audioManager?.PlayStartScene();
    }

    public IEnumerator PlayLinePhase()
    {
        yield return StartCoroutine(LinePhase());
    }

    public IEnumerator PlayWaitLoop(System.Func<bool> isReady)
    {
        glowLine.gameObject.SetActive(true);
        RectTransform rt = glowLine.rectTransform;
        rt.sizeDelta = new Vector2(2000f, 80f);
        rt.anchoredPosition = Vector2.zero;

        float t = 0f;
        while (!isReady())
        {
            t += Time.deltaTime * 2f;
            float pulse = 0.4f + Mathf.Sin(t) * 0.25f;
            glowLine.color = new Color(1f, 1f, 1f, pulse);
            float h = 60f + Mathf.Sin(t * 0.7f) * 20f;
            rt.sizeDelta = new Vector2(2000f, h);
            yield return null;
        }

    }

    public IEnumerator PlayRevealPhase()
    {
        yield return StartCoroutine(ExpandPhase());
        yield return StartCoroutine(StaticPhase());
        yield return StartCoroutine(SettlePhase());
    }

    public IEnumerator PlayStartup()
    {
        yield return StartCoroutine(PlayLinePhase());
        yield return StartCoroutine(PlayRevealPhase());
    }

    // Фаза 1: тонкая светящаяся линия появляется по центру
    private IEnumerator LinePhase()
    {
        glowLine.gameObject.SetActive(true);
        RectTransform rt = glowLine.rectTransform;
        rt.sizeDelta = new Vector2(2000f, 80f);
        rt.anchoredPosition = Vector2.zero;

        Color c = Color.white;
        c.a = 0f;
        glowLine.color = c;

        yield return null;
        audioManager?.PlayStartScene();

        float elapsed = 0f;
        while (elapsed < lineDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / lineDuration);
            c.a = t;
            glowLine.color = c;
            yield return null;
        }

        yield return new WaitForSeconds(0.08f);
    }

    // Фаза 2: линия взрывается — заполняет экран, потом ScreenFader открывается
    private IEnumerator ExpandPhase()
    {
        RectTransform rt = glowLine.rectTransform;
        RectTransform parent = rt.parent as RectTransform;
        float fullH = parent.rect.height + 200f;

        float elapsed = 0f;
        while (elapsed < expandDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / expandDuration);

            float h = Mathf.Lerp(80f, fullH, t);
            rt.sizeDelta = new Vector2(2000f, h);

            if (t > 0.4f)
            {
                float openT = (t - 0.4f) / 0.6f;
                screenFader.SetAlpha(Mathf.Lerp(1f, 0f, openT));
            }

            Color c = glowLine.color;
            c.a = Mathf.Lerp(1f, 0f, Mathf.Clamp01((t - 0.5f) * 2f));
            glowLine.color = c;

            yield return null;
        }

        screenFader.SetAlpha(0f);
        glowLine.gameObject.SetActive(false);
    }

    // Фаза 3: статика поверх изображения
    private IEnumerator StaticPhase()
    {
        staticNoise.gameObject.SetActive(true);
        vignetteOverlay.gameObject.SetActive(true);
        vignetteOverlay.color = new Color(0, 0, 0, 0.7f);

        float elapsed = 0f;
        while (elapsed < staticDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / staticDuration;

            if (Time.frameCount % 2 == 0)
                UpdateNoise(1f - t);

            Color c = staticNoise.color;
            c.a = Mathf.Lerp(0.85f, 0f, t);
            staticNoise.color = c;

            yield return null;
        }

        staticNoise.gameObject.SetActive(false);
    }

    // Фаза 4: стабилизация — виньетка спадает
    private IEnumerator SettlePhase()
    {
        float elapsed = 0f;
        Color c = vignetteOverlay.color;
        while (elapsed < settleDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(0.7f, 0f, elapsed / settleDuration);
            vignetteOverlay.color = c;
            yield return null;
        }
        vignetteOverlay.gameObject.SetActive(false);
    }

    // --- Генерация текстур ---

    private void BuildGlowTexture()
    {
        int w = 4, h = 128;
        glowTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        glowTex.filterMode = FilterMode.Bilinear;
        glowTex.wrapMode   = TextureWrapMode.Clamp;

        float center = h / 2f;
        for (int y = 0; y < h; y++)
        {
            float dist  = Mathf.Abs(y - center) / center;
            float alpha = Mathf.Pow(1f - dist, 2.5f);
            Color col = Color.Lerp(
                new Color(0.6f, 0.85f, 1f, alpha),
                new Color(1f,   1f,    1f, alpha),
                1f - dist);
            for (int x = 0; x < w; x++)
                glowTex.SetPixel(x, y, col);
        }
        glowTex.Apply();
        glowLine.texture = glowTex;
    }

    private void BuildNoiseTexture()
    {
        noiseTex = new Texture2D(noiseWidth, noiseHeight, TextureFormat.RGBA32, false);
        noiseTex.filterMode = FilterMode.Point;
        noiseTex.wrapMode   = TextureWrapMode.Repeat;
        UpdateNoise(1f);
        staticNoise.texture = noiseTex;
        staticNoise.color   = new Color(1, 1, 1, 0.85f);
    }

    private void UpdateNoise(float intensity)
    {
        Color[] pixels = new Color[noiseWidth * noiseHeight];
        for (int i = 0; i < pixels.Length; i++)
        {
            int row     = i / noiseWidth;
            float scanM = (row % 2 == 0) ? 1f : 0.3f;
            float v     = Random.value * intensity * scanM;
            pixels[i]   = new Color(v, v, v, intensity);
        }
        noiseTex.SetPixels(pixels);
        noiseTex.Apply();
    }

    private void BuildVignetteTexture()
    {
        int size = 256;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Vector2 center = new Vector2(size / 2f, size / 2f);
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx    = (x - center.x) / (size / 2f);
                float dy    = (y - center.y) / (size / 2f);
                float dist  = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Pow(Mathf.Clamp01((dist - 0.35f) * 2f), 2f);
                tex.SetPixel(x, y, new Color(0, 0, 0, alpha));
            }
        tex.Apply();
        vignetteOverlay.sprite = Sprite.Create(tex,
            new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        vignetteOverlay.type = Image.Type.Simple;
    }
}
