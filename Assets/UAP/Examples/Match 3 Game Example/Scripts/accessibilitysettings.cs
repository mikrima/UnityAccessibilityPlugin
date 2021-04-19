using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class accessibilitysettings : MonoBehaviour
{
	public Toggle m_EnableAccessibility = null;
	public Slider m_SpeechRateSlider = null;

	public GameObject m_AccessibilityConfirmation = null;

	//////////////////////////////////////////////////////////////////////////

	public void OnEnable()
	{
		m_EnableAccessibility.isOn = UAP_AccessibilityManager.IsEnabled();
		m_SpeechRateSlider.value = UAP_AccessibilityManager.GetSpeechRate();
	}

	//////////////////////////////////////////////////////////////////////////

	public void OnAccessibilityEnabledToggleChanged(bool newValue)
	{
		if (UAP_AccessibilityManager.IsEnabled() == newValue)
			return;

		if (newValue == true)
			m_AccessibilityConfirmation.SetActive(true);
		else
			UAP_AccessibilityManager.EnableAccessibility(false, true);
	}

	//////////////////////////////////////////////////////////////////////////

	public void OnSpeechRateSliderChanged()
	{
		UAP_AccessibilityManager.SetSpeechRate(Mathf.RoundToInt(m_SpeechRateSlider.value));
	}

	//////////////////////////////////////////////////////////////////////////

	public void OnEnableCancel()
	{
		m_EnableAccessibility.isOn = false;
		m_AccessibilityConfirmation.SetActive(false);
	}

	//////////////////////////////////////////////////////////////////////////

	public void OnEnableConfirm()
	{
		UAP_AccessibilityManager.EnableAccessibility(true, true);
		m_AccessibilityConfirmation.SetActive(false);
	}

	//////////////////////////////////////////////////////////////////////////

	public void OnCloseSettings()
	{
		DestroyImmediate(gameObject);
	}
}
