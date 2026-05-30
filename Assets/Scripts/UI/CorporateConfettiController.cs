using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CorporateConfettiController : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite[] confettiSprites;
    [SerializeField] private int[] spriteWeights;

    [Header("Container")]
    [SerializeField] private RectTransform container;

    private readonly List<ConfettiParticle> particles = new();
    private bool isPlaying;

    private struct ConfettiParticle
    {
        public RectTransform rt;
        public Image image;
        public float lifetime;
        public float maxLifetime;
        public float fallSpeed;
        public float xDrift;
        public float rotationSpeed;
        public float fadeOutStart;
        public Vector2 startPos;
    }

    [System.Serializable]
    public struct BurstPreset
    {
        public int spawnCount;
        public float duration;
        public float lifetimeMin;
        public float lifetimeMax;
        public float fallSpeedMin;
        public float fallSpeedMax;
        public float xDriftRange;
        public float rotationSpeedRange;
        public float scaleMin;
        public float scaleMax;
        public float fadeOutStart;
    }

    private static readonly BurstPreset SmallBurst = new BurstPreset
    {
        spawnCount = 42,
        duration = 1.8f,
        lifetimeMin = 1.8f,
        lifetimeMax = 2.6f,
        fallSpeedMin = 180f,
        fallSpeedMax = 360f,
        xDriftRange = 80f,
        rotationSpeedRange = 180f,
        scaleMin = 0.35f,
        scaleMax = 0.75f,
        fadeOutStart = 0.7f
    };

    private static readonly BurstPreset MediumBurst = new BurstPreset
    {
        spawnCount = 65,
        duration = 2.3f,
        lifetimeMin = 2.2f,
        lifetimeMax = 3.0f,
        fallSpeedMin = 200f,
        fallSpeedMax = 420f,
        xDriftRange = 100f,
        rotationSpeedRange = 220f,
        scaleMin = 0.4f,
        scaleMax = 0.85f,
        fadeOutStart = 0.7f
    };

    private static readonly BurstPreset FinalBurst = new BurstPreset
    {
        spawnCount = 105,
        duration = 3.2f,
        lifetimeMin = 2.8f,
        lifetimeMax = 3.8f,
        fallSpeedMin = 250f,
        fallSpeedMax = 550f,
        xDriftRange = 140f,
        rotationSpeedRange = 260f,
        scaleMin = 0.45f,
        scaleMax = 0.95f,
        fadeOutStart = 0.7f
    };

    public void PlayPromotionSmall()
    {
        Play(SmallBurst);
    }

    public void PlayPromotionMedium()
    {
        Play(MediumBurst);
    }

    public void PlayFinalCareer()
    {
        Play(FinalBurst);
    }

    public void Play(BurstPreset preset)
    {
        if (isPlaying) Cleanup();
        StartCoroutine(BurstCoroutine(preset));
    }

    private IEnumerator BurstCoroutine(BurstPreset preset)
    {
        isPlaying = true;

        float spawnInterval = preset.duration / preset.spawnCount;
        Rect containerRect = container.rect;
        float halfW = containerRect.width * 0.5f;

        int[] weightTable = BuildWeightTable();

        for (int i = 0; i < preset.spawnCount; i++)
        {
            SpawnParticle(preset, halfW, weightTable);

            float wait = spawnInterval * Random.Range(0.5f, 1.5f);
            float elapsed = 0f;
            while (elapsed < wait)
            {
                elapsed += Time.unscaledDeltaTime;
                UpdateParticles();
                yield return null;
            }
        }

        while (particles.Count > 0)
        {
            UpdateParticles();
            yield return null;
        }

        isPlaying = false;
    }

    private void SpawnParticle(BurstPreset preset, float halfW, int[] weightTable)
    {
        if (confettiSprites == null || confettiSprites.Length == 0) return;

        GameObject go = new GameObject("Confetti", typeof(RectTransform));
        go.layer = 5;
        go.transform.SetParent(container, false);

        RectTransform rt = (RectTransform)go.transform;
        float x = Random.Range(-halfW * 0.9f, halfW * 0.9f);
        float y = container.rect.height * 0.5f + 30f;
        rt.anchoredPosition = new Vector2(x, y);

        float scale = Random.Range(preset.scaleMin, preset.scaleMax);
        rt.localScale = new Vector3(scale, scale, 1f);
        rt.sizeDelta = new Vector2(64f, 64f);
        rt.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

        Image img = go.AddComponent<Image>();
        int spriteIdx = weightTable[Random.Range(0, weightTable.Length)];
        img.sprite = confettiSprites[spriteIdx];
        img.raycastTarget = false;
        img.preserveAspect = true;

        ConfettiParticle p = new ConfettiParticle
        {
            rt = rt,
            image = img,
            lifetime = 0f,
            maxLifetime = Random.Range(preset.lifetimeMin, preset.lifetimeMax),
            fallSpeed = Random.Range(preset.fallSpeedMin, preset.fallSpeedMax),
            xDrift = Random.Range(-preset.xDriftRange, preset.xDriftRange),
            rotationSpeed = Random.Range(-preset.rotationSpeedRange, preset.rotationSpeedRange),
            fadeOutStart = preset.fadeOutStart,
            startPos = new Vector2(x, y)
        };

        particles.Add(p);
    }

    private void UpdateParticles()
    {
        float dt = Time.unscaledDeltaTime;

        for (int i = particles.Count - 1; i >= 0; i--)
        {
            ConfettiParticle p = particles[i];
            p.lifetime += dt;

            if (p.lifetime >= p.maxLifetime || p.rt == null)
            {
                if (p.rt != null) Destroy(p.rt.gameObject);
                particles.RemoveAt(i);
                continue;
            }

            float t = p.lifetime / p.maxLifetime;

            Vector2 pos = p.rt.anchoredPosition;
            pos.y -= p.fallSpeed * dt;
            pos.x += p.xDrift * dt * Mathf.Sin(p.lifetime * 2f);
            p.rt.anchoredPosition = pos;

            float angle = p.rt.localEulerAngles.z + p.rotationSpeed * dt;
            p.rt.localEulerAngles = new Vector3(0f, 0f, angle);

            if (t > p.fadeOutStart)
            {
                float fadeT = (t - p.fadeOutStart) / (1f - p.fadeOutStart);
                Color c = p.image.color;
                c.a = 1f - fadeT;
                p.image.color = c;
            }

            particles[i] = p;
        }
    }

    private int[] BuildWeightTable()
    {
        if (spriteWeights == null || spriteWeights.Length == 0 ||
            confettiSprites == null || confettiSprites.Length == 0)
        {
            int[] simple = new int[confettiSprites != null ? confettiSprites.Length : 0];
            for (int i = 0; i < simple.Length; i++) simple[i] = i;
            return simple;
        }

        List<int> table = new List<int>();
        int count = Mathf.Min(spriteWeights.Length, confettiSprites.Length);
        for (int i = 0; i < count; i++)
        {
            int w = Mathf.Max(1, spriteWeights[i]);
            for (int j = 0; j < w; j++)
                table.Add(i);
        }

        for (int i = count; i < confettiSprites.Length; i++)
            table.Add(i);

        return table.ToArray();
    }

    public void Cleanup()
    {
        StopAllCoroutines();
        for (int i = particles.Count - 1; i >= 0; i--)
        {
            if (particles[i].rt != null)
                Destroy(particles[i].rt.gameObject);
        }
        particles.Clear();
        isPlaying = false;
    }

    private void OnDisable()
    {
        Cleanup();
    }
}
