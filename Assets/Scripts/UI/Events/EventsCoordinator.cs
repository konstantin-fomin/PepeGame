using UnityEngine;

public class EventsCoordinator : MonoBehaviour
{
    [SerializeField] private OfficeEventPopupView popupView;
    [SerializeField] private OfficeEventManager eventManager;
private void OnEnable()
    {
        if (eventManager == null || popupView == null) return;
        eventManager.OnEventToast += popupView.Show;
        eventManager.OnEventToastExpired += popupView.Hide;
    }

private void OnDisable()
    {
        if (eventManager == null || popupView == null) return;
        eventManager.OnEventToast -= popupView.Show;
        eventManager.OnEventToastExpired -= popupView.Hide;
    }
}
