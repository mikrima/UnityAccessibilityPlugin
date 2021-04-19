using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Use this if the toggle is used in the Settings to indicate whether the Accessibility plugin is on/off.
/// You don't have to use this necessarily, but it will set the correct toggle state automatically, even 
/// when the gesture is used to change the state while the toggle is visible.
/// This will set or unset the toggle - so you OnChange listeners need to check whether there is anything 
/// to do before acting (whether plugin enabled state matches the toggle state).
/// </summary>
[AddComponentMenu("Accessibility/UI/Accessible Plugin Toggle")]
public class AccessiblePluginToggle : AccessibleToggle
{
	/// <summary>
	/// If true, this component will not just set the toggle state, but also react
	/// when the toggle is changed by the user - and then it will enable/disable the plugin.
	/// The reason that this is optional, is that some games prefer to show an additional confirmation 
	/// window before enabling the plugin and drastically changing the game's input.
	/// </summary>
	public bool m_HandleActivation = true;

#if ACCESS_NGUI
	private EventDelegate m_Callback = null;
#endif

	//////////////////////////////////////////////////////////////////////////

	void OnEnable()
	{
		// Register as state change listener with the plugin
		UAP_AccessibilityManager.RegisterAccessibilityModeChangeCallback(AccessibilitiyPlugin_StateChanged);

		// Set the correct toggle state if needed
		UpdateToggleState();
	}

	//////////////////////////////////////////////////////////////////////////

	void OnDisable()
	{
		// Unregister state change listener from the plugin
		UAP_AccessibilityManager.UnregisterAccessibilityModeChangeCallback(AccessibilitiyPlugin_StateChanged);
	}

	//////////////////////////////////////////////////////////////////////////

	void Start()
	{
		// Register as listener to the toggle
		Toggle toggle = GetToggle();
		if (toggle != null)
		{
			toggle.onValueChanged.AddListener(OnToggleStateChanged);
		}

#if ACCESS_NGUI
		UIToggle nGUIToggle = GetNGUIToggle();
		if (nGUIToggle != null)
		{
			if (m_Callback == null)
				m_Callback = new EventDelegate(this, "OnNGUIToggleStateChanged");
			nGUIToggle.onChange.Add(m_Callback);
		}
#endif
	}

	//////////////////////////////////////////////////////////////////////////

	public void OnToggleStateChanged(bool newState)
	{
		if (!m_HandleActivation)
			return;

		if (newState == UAP_AccessibilityManager.IsEnabled())
			return;

		UAP_AccessibilityManager.EnableAccessibility(newState);
	}

#if ACCESS_NGUI
	public void OnNGUIToggleStateChanged()
	{
		UIToggle nGUIToggle = GetNGUIToggle();
		if (nGUIToggle != null)
			OnToggleStateChanged(nGUIToggle.value);
	}
#endif

	//////////////////////////////////////////////////////////////////////////

	public void AccessibilitiyPlugin_StateChanged(bool newEnabledState)
	{
		// Set the correct toggle state if needed
		UpdateToggleState();
	}

	//////////////////////////////////////////////////////////////////////////

	private void UpdateToggleState()
	{
		if (UAP_AccessibilityManager.IsEnabled() != IsChecked())
			SetToggleState(UAP_AccessibilityManager.IsEnabled());
	}

	//////////////////////////////////////////////////////////////////////////
}
