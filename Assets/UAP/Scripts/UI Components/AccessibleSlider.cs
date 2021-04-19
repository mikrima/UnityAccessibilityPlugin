using UnityEngine;
using System.Collections;

using UnityEngine.UI;
using UnityEngine.EventSystems;

[AddComponentMenu("Accessibility/UI/Accessible Slider")]
public class AccessibleSlider : UAP_BaseElement
{
	//! Will read out percentage of the slider instead of actual value
	public bool m_ReadPercentages = true;

	//! Increment by which the sliders is moved per swipe (Default is 5%)
	public float m_Increments = 5.0f;
	public bool m_IncrementInPercent = true;
	public bool m_WholeNumbersOnly = true;

	//////////////////////////////////////////////////////////////////////////

	AccessibleSlider()
	{
		m_Type = AccessibleUIGroupRoot.EUIElement.ESlider;
	}

	//////////////////////////////////////////////////////////////////////////

	public override bool IsElementActive()
	{
		// Return whether this button is usable, and visible
		if (!base.IsElementActive())
			return false;

		if (m_ReferenceElement != null)
			if (!m_ReferenceElement.gameObject.activeInHierarchy)
				return false;

		if (!UAP_AccessibilityManager.GetSpeakDisabledInteractables())
			if (!IsInteractable())
				return false;

		return true;
	}

	//////////////////////////////////////////////////////////////////////////

	public override bool IsInteractable()
	{
		// Check if the UI Element is enabled and allows interaction
		Slider slider = GetSlider();
		if (slider != null)
		{
			if (slider.enabled == false || slider.IsInteractable() == false)
				return false;

			return true;
		}

#if ACCESS_NGUI
		UISlider nGUIComponent = GetNGUISlider();
		if (nGUIComponent != null)
		{
			if (nGUIComponent.enabled == false)
				return false;
			else
				return true;
		}
#endif

		return true;
	}

	//////////////////////////////////////////////////////////////////////////

	Slider GetSlider()
	{
		Slider refElement = null;
		if (m_ReferenceElement != null)
			refElement = m_ReferenceElement.GetComponent<Slider>();
		if (refElement == null)
			refElement = GetComponent<Slider>();

		return refElement;
	}

	//////////////////////////////////////////////////////////////////////////

#if ACCESS_NGUI
	private UISlider GetNGUISlider()
	{
		UISlider refElement = null;
		if (m_ReferenceElement != null)
			refElement = m_ReferenceElement.GetComponent<UISlider>();
		if (refElement == null)
			refElement = GetComponent<UISlider>();

		return refElement;
	}
#endif

	//////////////////////////////////////////////////////////////////////////

	public override string GetCurrentValueAsText()
	{
		bool foundValue = false;
		float value = -1.0f;

		Slider slider = GetSlider();
		if (slider != null)
		{
			foundValue = true;
			value = slider.value;

			if (m_ReadPercentages)
			{
				// Convert the slider value between min and max to percentage
				value = (value - slider.minValue) / (slider.maxValue - slider.minValue);
				value *= 100.0f;
			}
		}

#if ACCESS_NGUI
		UISlider nGUISlider = GetNGUISlider();
		if (nGUISlider != null)
		{
			foundValue = true;
			value = nGUISlider.value;

			if (m_ReadPercentages)
			{
				// NGUI sliders are always in the range of 0..1
				value *= 100.0f;
			}
		}
#endif

		if (foundValue)
		{

			string valueAsText = value.ToString("0.##");

			if (m_WholeNumbersOnly || (slider != null && slider.wholeNumbers))
				valueAsText = value.ToString("0");

			if (m_ReadPercentages)
				valueAsText += "%";

			return valueAsText;
		}

		return "";
	}

	//////////////////////////////////////////////////////////////////////////

	public override bool Increment()
	{
		Slider slider = GetSlider();
		if (slider != null && slider.value == slider.maxValue)
			return false;

#if ACCESS_NGUI
		UISlider nGUISlider = GetNGUISlider();
		if (nGUISlider != null && nGUISlider.value == 1.0f)
			return false;
#endif

		ModifySliderValue(m_Increments);
		return true;
	}

	//////////////////////////////////////////////////////////////////////////

	public override bool Decrement()
	{
		Slider slider = GetSlider();
		if (slider != null && slider.value == slider.minValue)
			return false;

#if ACCESS_NGUI
		UISlider nGUISlider = GetNGUISlider();
		if (nGUISlider != null && nGUISlider.value == 0.0f)
			return false;
#endif

		ModifySliderValue(-m_Increments);
		return true;
	}

	//////////////////////////////////////////////////////////////////////////
	
	private void ModifySliderValue(float change)
	{
		Slider slider = GetSlider();
		if (slider != null)
		{
			// Modify value
			float incrementValue = change;
			if (m_IncrementInPercent)
			{
				incrementValue = (slider.maxValue - slider.minValue) * (incrementValue / 100.0f);
			}
			slider.value += incrementValue;
		}
		
#if ACCESS_NGUI
		UISlider nGUISlider = GetNGUISlider();
		if (nGUISlider != null)
		{
			// Modify value
			float incrementValue = change;
			if (m_IncrementInPercent)
			{
				// NGUI Sliders are always in the range 0..1
				incrementValue = (incrementValue / 100.0f);
			}
			nGUISlider.value += incrementValue;
		}
#endif

	}

	//////////////////////////////////////////////////////////////////////////

	protected override void OnHoverHighlight(bool enable)
	{
		Slider slider = GetSlider();
		if (slider != null)
		{
			var pointer = new PointerEventData(EventSystem.current); // pointer event for Execute
			if (enable)
				slider.OnPointerEnter(pointer);
			else
				slider.OnPointerExit(pointer);
		}

#if ACCESS_NGUI
		UISlider nGUISlider = GetNGUISlider();
		if (nGUISlider != null)
		{
			UIButton button = nGUISlider.gameObject.GetComponentInChildren<UIButton>();
			if (button != null)
			{
				if (enable)
					button.SetState(IsInteractable() ? UIButtonColor.State.Hover : UIButtonColor.State.Disabled, false);
				else
					button.SetState(IsInteractable() ? UIButtonColor.State.Normal : UIButtonColor.State.Disabled, false);
			}
		}
#endif
	}

	//////////////////////////////////////////////////////////////////////////

	public override bool AutoFillTextLabel()
	{
		// If there is no name label set, don't set anything as name
		if (!base.AutoFillTextLabel())
			m_Text = "";

		return false;
	}

	//////////////////////////////////////////////////////////////////////////


}
