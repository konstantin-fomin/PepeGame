using UnityEngine;

public class EventsCoordinator : MonoBehaviour
{
    [SerializeField] private OfficeEventPopupView popupView;
    [SerializeField] private OfficeEventToastView toastView;
    [SerializeField] private OfficeEventManager eventManager;

    private void OnEnable()
    {
        if (eventManager == null || toastView == null) return;
        eventManager.OnEventToast += toastView.Show;
        eventManager.OnEventToastExpired += toastView.Hide;
    }

    private void OnDisable()
    {
        if (eventManager == null || toastView == null) return;
        eventManager.OnEventToast -= toastView.Show;
        eventManager.OnEventToastExpired -= toastView.Hide;
    }
}
