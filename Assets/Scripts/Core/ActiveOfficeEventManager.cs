using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ActiveOfficeEventManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private List<ActiveOfficeEventData> events;
    [SerializeField] private OfficeEventPopupView popupView;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private MenuNavigationController menuNav;

    [Header("Timing")]
    [SerializeField] private float minInterval = 180f;
    [SerializeField] private float maxInterval = 420f;

    private bool isPendingPopup = false;
    private bool isBoostActive = false;
    private Coroutine eventLoopCoroutine;
    private Coroutine boostCoroutine;

    public event Action<string, float> OnActiveEventStarted;
    public event Action OnActiveEventEnded;

    public void StartSystem()
    {
        if (eventLoopCoroutine != null) return;
        eventLoopCoroutine = StartCoroutine(EventLoop());
    }

    public void StopSystem()
    {
        if (eventLoopCoroutine != null)
        {
            StopCoroutine(eventLoopCoroutine);
            eventLoopCoroutine = null;
        }

        if (boostCoroutine != null)
        {
            StopCoroutine(boostCoroutine);
            boostCoroutine = null;
            ClearBoost();
        }

        if (isPendingPopup)
        {
            isPendingPopup = false;
            if (popupView != null) popupView.Hide();
        }
    }

    private IEnumerator EventLoop()
    {
        while (true)
        {
            float wait = UnityEngine.Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(wait);

            if (!CanShow()) continue;

            ActiveOfficeEventData picked = PickEvent();
            if (picked == null) continue;

            isPendingPopup = true;
            ActiveOfficeEventData capturedEvent = picked;
            popupView.Show(capturedEvent, () => OnAccepted(capturedEvent));

            float timer = 0f;
            float lifetime = Mathf.Max(0.1f, capturedEvent.popupLifetimeSeconds);
            while (timer < lifetime && isPendingPopup)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            if (isPendingPopup)
            {
                isPendingPopup = false;
                if (popupView != null) popupView.Hide();
            }
        }
    }

    private bool CanShow()
    {
        return !isPendingPopup
            && !isBoostActive
            && (menuNav == null || menuNav.IsGameplayAvailableForOfficeEvents);
    }

    private void OnAccepted(ActiveOfficeEventData data)
    {
        isPendingPopup = false;
        if (popupView != null) popupView.Hide();

        if (data.effectType == OfficeEventType.ClickKpiMultiplier)
        {
            gameManager.clickKpiMultiplier *= data.effectValue;
        }
        else if (data.effectType == OfficeEventType.PassiveKpiMultiplier)
        {
            gameManager.passiveKpiMultiplier *= data.effectValue;
        }

        isBoostActive = true;
        OnActiveEventStarted?.Invoke(data.title, data.durationSeconds);
        boostCoroutine = StartCoroutine(BoostTimer(data));
    }

    private IEnumerator BoostTimer(ActiveOfficeEventData data)
    {
        yield return new WaitForSeconds(data.durationSeconds);

        if (data.effectType == OfficeEventType.ClickKpiMultiplier && data.effectValue != 0f)
        {
            gameManager.clickKpiMultiplier /= data.effectValue;
        }
        else if (data.effectType == OfficeEventType.PassiveKpiMultiplier && data.effectValue != 0f)
        {
            gameManager.passiveKpiMultiplier /= data.effectValue;
        }

        isBoostActive = false;
        boostCoroutine = null;
        OnActiveEventEnded?.Invoke();
    }

    private void ClearBoost()
    {
        if (isBoostActive)
        {
            gameManager.clickKpiMultiplier = 1f;
            gameManager.passiveKpiMultiplier = 1f;
            isBoostActive = false;
            OnActiveEventEnded?.Invoke();
        }
    }

    private ActiveOfficeEventData PickEvent()
    {
        if (events == null || events.Count == 0) return null;

        string currentRankId = gameManager.CurrentRank != null
            ? gameManager.CurrentRank.rankId ?? "intern"
            : "intern";

        List<string> rankOrder = new List<string>
            { "intern", "junior", "middle", "senior", "lead", "ceo" };
        int currentRankIdx = rankOrder.IndexOf(currentRankId.ToLower());

        List<ActiveOfficeEventData> available = events.Where(e =>
        {
            if (e == null) return false;
            int minIdx = rankOrder.IndexOf(
                string.IsNullOrEmpty(e.minRankId) ? "intern" : e.minRankId.ToLower());
            if (minIdx < 0) minIdx = 0;
            return currentRankIdx >= minIdx;
        }).ToList();

        if (available.Count == 0) return null;

        int totalWeight = available.Sum(e => e.weight);
        int roll = UnityEngine.Random.Range(0, totalWeight);
        int cumulative = 0;
        foreach (ActiveOfficeEventData e in available)
        {
            cumulative += e.weight;
            if (roll < cumulative) return e;
        }
        return available[available.Count - 1];
    }
}
