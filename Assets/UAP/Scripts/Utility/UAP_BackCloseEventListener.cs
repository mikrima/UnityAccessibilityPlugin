using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This component listens for the back or close gesture and triggers an event.
/// It is meant to be placed on the 'close' or 'X' button of popup dialogs etc, to 
/// trigger the regular close event when the back/close accessibility gesture is
/// performed. See MagicGestures in the documentation (on iOS, this is a 2-finger scrub)
/// </summary>
public class UAP_BackCloseEventListener : MonoBehaviour
{
	public UnityEvent m_OnBackEvent = new UnityEvent();

	private void OnEnable()
	{
		UAP_AccessibilityManager.RegisterOnBackCallback(OnScrubGesturePerformed);
	}

	private void OnDisable()
	{
		UAP_AccessibilityManager.UnregisterOnBackCallback(OnScrubGesturePerformed);
	}

	private void OnScrubGesturePerformed()
	{
		m_OnBackEvent.Invoke();
	}
}
