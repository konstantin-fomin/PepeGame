using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ConfettiTestLoop : MonoBehaviour
{
    [SerializeField] private CorporateConfettiController confetti;
    [SerializeField] private Button smallButton;
    [SerializeField] private Button mediumButton;
    [SerializeField] private Button finalButton;
    [SerializeField] private float loopDelay = 1.0f;

    private Coroutine loopCoroutine;
    private int activePreset = -1;

    private void Start()
    {
        if (smallButton != null)
            smallButton.onClick.AddListener(() => ToggleLoop(0));
        if (mediumButton != null)
            mediumButton.onClick.AddListener(() => ToggleLoop(1));
        if (finalButton != null)
            finalButton.onClick.AddListener(() => ToggleLoop(2));
    }

    private void ToggleLoop(int preset)
    {
        if (activePreset == preset)
        {
            StopLoop();
            return;
        }

        StopLoop();
        activePreset = preset;
        loopCoroutine = StartCoroutine(LoopCoroutine(preset));
    }

    private void StopLoop()
    {
        if (loopCoroutine != null)
        {
            StopCoroutine(loopCoroutine);
            loopCoroutine = null;
        }
        activePreset = -1;
        if (confetti != null)
            confetti.Cleanup();
    }

    private IEnumerator LoopCoroutine(int preset)
    {
        while (true)
        {
            if (confetti != null)
            {
                switch (preset)
                {
                    case 0: confetti.PlayPromotionSmall(); break;
                    case 1: confetti.PlayPromotionMedium(); break;
                    case 2: confetti.PlayFinalCareer(); break;
                }
            }

            float waitTime = preset == 2 ? 4.5f : 3.0f;
            yield return new WaitForSeconds(waitTime + loopDelay);
        }
    }
}
