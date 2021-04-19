using UnityEngine;
using System.Collections;

using UnityEngine.UI;
using UnityEngine.EventSystems;

[AddComponentMenu("Accessibility/UI/Accessible Toggle")]
public class AccessibleToggle : UAP_BaseElement
{
	public bool m_UseCustomOnOff = false;
	public string m_CustomOn = "Checked";
	public string m_CustomOff = "Not Checked";
	public bool m_CustomHintsAreLocalizationKeys = false;
	public AudioClip m_CustomOnAudio = null;
	public AudioClip m_CustomOffAudio = null;

	//////////////////////////////////////////////////////////////////////////

	public AccessibleToggle()
	{
		m_Type = AccessibleUIGroupRoot.EUIElement.EToggle;
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
		Toggle toggle = GetToggle();
		if (toggle != null)
		{
			if (toggle.enabled == false || toggle.IsInteractable() == false)
				return false;
			else
				return true;
		}

#if ACCESS_NGUI
		UIToggle nGUIToggle = GetNGUIToggle();
		if (nGUIToggle != null)
		{
			if (nGUIToggle.enabled == false || nGUIToggle.isActiveAndEnabled == false)
				return false;
			else
				return true;
		}
#endif

		// No toggle to be found
		return false;
	}

	//////////////////////////////////////////////////////////////////////////

	protected override void OnInteract()
	{
		Toggle toggle = GetToggle();

		// Press button
		if (toggle != null)
		{
			var pointer = new PointerEventData(EventSystem.current); // pointer event for Execute
			toggle.OnPointerClick(pointer);
		}

#if ACCESS_NGUI
		UIToggle nGUIToggle = GetNGUIToggle();
		if (nGUIToggle != null)
		{
			nGUIToggle.gameObject.SendMessage("OnClick");
			return;
		}
#endif
	}

	//////////////////////////////////////////////////////////////////////////

	protected Toggle GetToggle()
	{
		Toggle toggle = null;
		if (m_ReferenceElement != null)
			toggle = m_ReferenceElement.GetComponent<Toggle>();
		if (toggle == null)
			toggle = GetComponent<Toggle>();

		return toggle;
	}

	//////////////////////////////////////////////////////////////////////////

#if ACCESS_NGUI
	protected UIToggle GetNGUIToggle()
	{
		UIToggle toggle = null;
		if (m_ReferenceElement != null)
			toggle = m_ReferenceElement.GetComponent<UIToggle>();
		if (toggle == null)
			toggle = GetComponent<UIToggle>();

		return toggle;
	}
#endif

	//////////////////////////////////////////////////////////////////////////

	public override string GetCurrentValueAsText()
	{
		if (IsChecked())
		{
			if (m_UseCustomOnOff)
			{
				if (m_CustomHintsAreLocalizationKeys)
					return UAP_AccessibilityManager.Localize(m_CustomOn);
				return m_CustomOn;
			}
			else
				return UAP_AccessibilityManager.Localize_Internal("Checkbox_Checked");
		}
		else
		{
			if (m_UseCustomOnOff)
			{
				if (m_CustomHintsAreLocalizationKeys)
					return UAP_AccessibilityManager.Localize(m_CustomOff);
				return m_CustomOff;
			}
			else
				return UAP_AccessibilityManager.Localize_Internal("Checkbox_NotChecked");
		}
	}

	//////////////////////////////////////////////////////////////////////////

	public bool IsChecked()
	{
		Toggle toggle = GetToggle();
		if (toggle != null)
			return toggle.isOn;

#if ACCESS_NGUI
		UIToggle nGUIToggle = GetNGUIToggle();
		if (nGUIToggle != null)
			return nGUIToggle.value;
#endif

		return false;
	}

	//////////////////////////////////////////////////////////////////////////

	public void SetToggleState(bool toggleState)
	{
		// Nothing to do?
		if (toggleState == IsChecked())
			return;

		Toggle toggle = GetToggle();
		if (toggle != null)
			toggle.isOn = toggleState;

#if ACCESS_NGUI
		UIToggle nGUIToggle = GetNGUIToggle();
		if (nGUIToggle != null)
			nGUIToggle.value = toggleState;
#endif
	}

	//////////////////////////////////////////////////////////////////////////

	public override AudioClip GetCurrentValueAsAudio()
	{
		Toggle toggle = GetToggle();
		if (toggle == null)
			return null;

		// Support custom on and off audio and text
		if (m_UseCustomOnOff)
		{
			if (toggle.isOn)
				return m_CustomOnAudio;
			else
				return m_CustomOffAudio;
		}

		return null;
	}

	//////////////////////////////////////////////////////////////////////////

	public override bool AutoFillTextLabel()
	{
		if (base.AutoFillTextLabel())
			return true;

		bool found = false;

		// Try to find a label in the button's children
		Transform textLabel = gameObject.transform.Find("Label");
		if (textLabel != null)
		{
			Text label = textLabel.gameObject.GetComponent<Text>();
			if (label != null)
			{
				m_Text = label.text;
				found = true;
			}
		}

		if (!found)
		{
			Text label = gameObject.GetComponentInChildren<Text>();
			if (label != null)
			{
				m_Text = label.text;
				found = true;
			}
		}

		// if nothing, use the GameObject's name
		if (!found)
			m_Text = gameObject.name;

		return found;
	}

	//////////////////////////////////////////////////////////////////////////

	protected override void OnHoverHighlight(bool enable)
	{
		Toggle toggle = GetToggle();
		if (toggle != null)
		{
			var pointer = new PointerEventData(EventSystem.current); // pointer event for Execute
			if (enable)
				toggle.OnPointerEnter(pointer);
			else
				toggle.OnPointerExit(pointer);
		}

#if ACCESS_NGUI
		// NGUI's toggles also have buttons attached, which takes care of the highlighting
		UIToggle nGUIToggle = GetNGUIToggle();
		UIButton nGUIButton = nGUIToggle != null ? nGUIToggle.gameObject.GetComponent<UIButton>() : null;
		if (nGUIButton != null)
		{
			if (enable)
				nGUIButton.SetState(IsInteractable() ? UIButtonColor.State.Hover : UIButtonColor.State.Disabled, false);
			else
				nGUIButton.SetState(IsInteractable() ? UIButtonColor.State.Normal : UIButtonColor.State.Disabled, false);
		}
#endif
	}

	//////////////////////////////////////////////////////////////////////////

	protected override void AutoInitialize()
	{
		if (m_TryToReadLabel)
		{
			bool found = false;

			// Unity UI
			//////////////////////////////////////////////////////////////////////////
			{
				// Try to find a label in the button's children
				Transform textLabel = gameObject.transform.Find("Text");
				if (textLabel != null)
				{
					Text label = textLabel.gameObject.GetComponent<Text>();
					if (label != null)
					{
						m_NameLabel = label.gameObject;
						found = true;
					}
				}

				if (!found)
				{
					Text label = gameObject.GetComponentInChildren<Text>();
					if (label != null)
					{
						m_NameLabel = label.gameObject;
						found = true;
					}
				}
			}

#if ACCESS_NGUI
			// NGUI
			//////////////////////////////////////////////////////////////////////////
			{
				Transform textLabel = gameObject.transform.Find("Label");
				if (textLabel != null)
				{
					UILabel label = textLabel.gameObject.GetComponent<UILabel>();
					if (label != null)
					{
						m_NameLabel = label.gameObject;
						found = true;
					}
				}

				if (!found)
				{
					UILabel label = gameObject.GetComponentInChildren<UILabel>();
					if (label != null)
					{
						m_NameLabel = label.gameObject;
						found = true;
					}
				}
			}
#endif
		}
		else
		{
			m_NameLabel = null;
		}
	}

	//////////////////////////////////////////////////////////////////////////


}
