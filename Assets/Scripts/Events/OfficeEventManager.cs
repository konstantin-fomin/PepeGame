using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class OfficeEventManager : MonoBehaviour
{
    public static OfficeEventManager Instance;

    [Header("Settings")]
    [SerializeField] private List<OfficeEventData> allEvents;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private MenuNavigationController menuNav;

    [Header("Floating KPI")]
    [SerializeField] private FloatingKpiSpawner floatingKpiSpawner;
    [SerializeField] private RectTransform kpiDisplayTransform;

    [Header("Timing")]
    [SerializeField] private float firstEventDelay   = 60f;
    [SerializeField] private float minInterval       = 60f;
    [SerializeField] private float maxInterval       = 150f;

    public OfficeEventData ActiveEvent   { get; private set; }
    public float ActiveTimeRemaining     { get; private set; }
    public bool  HasActiveEvent          => ActiveEvent != null;

    public class OfficeEventToastResult
    {
        public OfficeEventData eventData;
        public string message;
        public int kpiReward;
        public bool hasTemporaryBuff;
        public float displaySeconds;
    }

    public event System.Action<OfficeEventToastResult> OnEventToast;
    public event System.Action OnEventToastExpired;

    public event System.Action OnEventExpired;
    public event System.Action<OfficeEventData> OnEventActivated;
    public event System.Action OnEventEnded;

    private bool isRunning = false;
    private bool isToastVisible = false;
    private Coroutine toastVisibilityCoroutine;
    private Coroutine activeEventCoroutine;

    private void Awake() => Instance = this;

    public void StartEventSystem()
    {
        if (isRunning) return;
        isRunning = true;
        StartCoroutine(EventLoop());
    }

public void StopEventSystem()
    {
        isRunning = false;
        StopAllCoroutines();
        toastVisibilityCoroutine = null;
        activeEventCoroutine = null;
        ClearVisibleToast();
        ClearActiveEvent();
    }

private IEnumerator EventLoop()
    {
        yield return new WaitForSeconds(firstEventDelay);

        while (isRunning)
        {
            yield return WaitUntilCanShowRandomOfficeEvent();
            if (!isRunning) yield break;

            OfficeEventData ev = PickEvent();
            if (ev != null)
            {
                SpawnEventToast(ev);
            }

            float wait = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(wait);
        }
    }

private IEnumerator RunEvent(OfficeEventData ev)
    {
        ActiveEvent = ev;
        ActiveTimeRemaining = Mathf.Max(0f, ev.durationSeconds);
        OnEventActivated?.Invoke(ev);

        float elapsed = 0f;
        while (elapsed < ev.durationSeconds)
        {
            elapsed += Time.deltaTime;
            ActiveTimeRemaining = Mathf.Max(0f, ev.durationSeconds - elapsed);
            yield return null;
        }

        activeEventCoroutine = null;
        ClearActiveEvent();
    }

private void ApplyEffects(OfficeEventData ev, bool apply)
    {
        if (ev == null || ev.effects == null || gameManager == null) return;

        foreach (var effect in ev.effects)
        {
            switch (effect.type)
            {
                case OfficeEventType.ClickKpiMultiplier:
                    gameManager.clickKpiMultiplier = apply ? GetSafeMultiplier(effect.value) : 1f;
                    break;
                case OfficeEventType.PassiveKpiMultiplier:
                    gameManager.passiveKpiMultiplier = apply ? GetSafeMultiplier(effect.value) : 1f;
                    break;
                case OfficeEventType.InstantKpiReward:
                    if (apply)
                    {
                        int reward = CalculateInstantKpiReward(effect.value);
                        if (reward > 0)
                        {
                            gameManager.AddKpiPublic(reward);
                        }
                    }
                    break;
                case OfficeEventType.NoEffectFlavor:
                    break;
            }
        }
    }

private float GetSafeMultiplier(float value)
    {
        return Mathf.Max(0.1f, value);
    }

    private int CalculateInstantKpiReward(float effectValue)
    {
        float safeValue = Mathf.Max(0f, effectValue);
        float baseKpi = Mathf.Max(0, gameManager.KpiPerClick);
        return Mathf.Max(0, Mathf.RoundToInt(baseKpi * safeValue));
    }

    private void SpawnFloatingKpiAtDisplay(int amount)
    {
        if (floatingKpiSpawner == null || kpiDisplayTransform == null) return;
        floatingKpiSpawner.Spawn(kpiDisplayTransform.position, amount, false);
    }

private bool IsGameplayAvailableForEvent()
    {
        if (RankUpPopupView.IsShowing) return false;
        return menuNav == null || menuNav.IsGameplayAvailableForOfficeEvents;
    }

    private OfficeEventData PickEvent()
    {
        string currentRankId = gameManager.CurrentRank?.rankId ?? "intern";
        List<string> rankOrder = new List<string>
            { "intern","junior","middle","senior","lead","ceo" };
        int currentRankIdx = rankOrder.IndexOf(currentRankId.ToLower());

        var available = allEvents.Where(e => {
            int minIdx = rankOrder.IndexOf(e.minRankId.ToLower());
            if (minIdx < 0) minIdx = 0;
            if (currentRankIdx < minIdx) return false;
            if (!string.IsNullOrEmpty(e.maxRankId))
            {
                int maxIdx = rankOrder.IndexOf(e.maxRankId.ToLower());
                if (maxIdx >= 0 && currentRankIdx > maxIdx) return false;
            }
            return true;
        }).ToList();

        if (available.Count == 0) return null;

        int totalWeight = available.Sum(e => e.weight);
        int roll = Random.Range(0, totalWeight);
        int cumulative = 0;
        foreach (var e in available)
        {
            cumulative += e.weight;
            if (roll < cumulative) return e;
        }
        return available[available.Count - 1];
    }


private bool CanShowRandomOfficeEvent()
    {
        return isRunning &&
            !HasActiveEvent &&
            !isToastVisible &&
            IsGameplayAvailableForEvent();
    }

    private IEnumerator WaitUntilCanShowRandomOfficeEvent()
    {
        while (isRunning && !CanShowRandomOfficeEvent())
        {
            yield return null;
        }
    }

private IEnumerator WaitForToastLifetime(float displaySeconds)
    {
        float elapsed = 0f;
        float lifetime = Mathf.Max(0.1f, displaySeconds);

        while (isRunning && isToastVisible && elapsed < lifetime)
        {
            if (!IsGameplayAvailableForEvent())
            {
                ClearVisibleToast();
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        ClearVisibleToast();
    }

private void ClearVisibleToast()
    {
        if (!isToastVisible) return;
        isToastVisible = false;
        OnEventToastExpired?.Invoke();
        OnEventExpired?.Invoke();
    }

private void ClearActiveEvent()
    {
        if (ActiveEvent == null) return;
        ApplyEffects(ActiveEvent, apply: false);
        ActiveEvent = null;
        ActiveTimeRemaining = 0f;
        OnEventEnded?.Invoke();
    }


private void SpawnEventToast(OfficeEventData ev)
    {
        OfficeEventToastResult result = ApplyEventAndCreateToastResult(ev);

        isToastVisible = true;
        OnEventToast?.Invoke(result);

        if (toastVisibilityCoroutine != null)
        {
            StopCoroutine(toastVisibilityCoroutine);
        }
        toastVisibilityCoroutine = StartCoroutine(WaitForToastLifetime(result.displaySeconds));

        if (result.hasTemporaryBuff)
        {
            activeEventCoroutine = StartCoroutine(RunEvent(ev));
        }
    }

    private OfficeEventToastResult ApplyEventAndCreateToastResult(OfficeEventData ev)
    {
        OfficeEventToastResult result = new OfficeEventToastResult
        {
            eventData = ev,
            message = GetBaseToastMessage(ev),
            kpiReward = 0,
            hasTemporaryBuff = false,
            displaySeconds = GetDisplaySeconds(ev)
        };

        if (ev == null || ev.effects == null || gameManager == null)
        {
            return result;
        }

        List<string> effectMessages = new List<string>();

        foreach (var effect in ev.effects)
        {
            switch (effect.type)
            {
                case OfficeEventType.ClickKpiMultiplier:
                    float clickMultiplier = GetSafeMultiplier(effect.value);
                    gameManager.clickKpiMultiplier = clickMultiplier;
                    result.hasTemporaryBuff = true;
                    effectMessages.Add($"Клики x{FormatMultiplier(clickMultiplier)} · {Mathf.CeilToInt(ev.durationSeconds)} сек.");
                    break;
                case OfficeEventType.PassiveKpiMultiplier:
                    float passiveMultiplier = GetSafeMultiplier(effect.value);
                    gameManager.passiveKpiMultiplier = passiveMultiplier;
                    result.hasTemporaryBuff = true;
                    effectMessages.Add($"Пассивный KPI x{FormatMultiplier(passiveMultiplier)} · {Mathf.CeilToInt(ev.durationSeconds)} сек.");
                    break;
                case OfficeEventType.InstantKpiReward:
                    int reward = CalculateInstantKpiReward(effect.value);
                    if (reward > 0)
                    {
                        gameManager.AddKpiPublic(reward);
                        result.kpiReward += reward;
                        SpawnFloatingKpiAtDisplay(reward);
                    }
                    break;
                case OfficeEventType.NoEffectFlavor:
                    break;
            }
        }

        if (result.kpiReward > 0)
        {
            effectMessages.Add($"+{result.kpiReward} KPI");
        }

        if (effectMessages.Count > 0)
        {
            string baseMessage = string.IsNullOrWhiteSpace(result.message) ? ev.title : result.message;
            result.message = string.IsNullOrWhiteSpace(baseMessage)
                ? string.Join(" ", effectMessages)
                : $"{baseMessage} {string.Join(" ", effectMessages)}";
        }

        return result;
    }

    private string GetBaseToastMessage(OfficeEventData ev)
    {
        if (ev == null) return string.Empty;
        if (!string.IsNullOrWhiteSpace(ev.description)) return ev.description.Replace("\n", " ");
        if (!string.IsNullOrWhiteSpace(ev.title)) return ev.title;
        return string.Empty;
    }

    private float GetDisplaySeconds(OfficeEventData ev)
    {
        if (ev == null) return 4f;
        return Mathf.Max(0.1f, ev.popupLifetimeSeconds);
    }

    private string FormatMultiplier(float value)
    {
        return value.ToString("0.##", CultureInfo.InvariantCulture);
    }
}
