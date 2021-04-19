using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[AddComponentMenu("Accessibility/UI/Accessible Button")]
public class AccessibleButton : UAP_BaseElement
{

	//////////////////////////////////////////////////////////////////////////

	AccessibleButton()
	{
		m_Type = AccessibleUIGroupRoot.EUIElement.EButton;
	}

	//////////////////////////////////////////////////////////////////////////

	protected override void OnInteract()
	{
		// Press button (works for UGUI and TMP)
		Button button = GetButton();
		if (button != null)
		{
			var pointer = new PointerEventData(EventSystem.current); // pointer event for Execute
			button.OnPointerClick(pointer);
			return;
		}

#if ACCESS_NGUI
		UIButton nGUIButton = GetNGUIButton();
		if (nGUIButton != null)
		{
			nGUIButton.SendMessage("OnPress", true, SendMessageOptions.DontRequireReceiver);
			nGUIButton.SendMessage("OnPress", false, SendMessageOptions.DontRequireReceiver);
			nGUIButton.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
			return;
		}
		else
		{
			UIEventTrigger nGUITrigger = GetNGUIEventTrigger();
			if (nGUITrigger != null)
			{
				nGUITrigger.SendMessage("OnPress", true, SendMessageOptions.DontRequireReceiver);
				nGUITrigger.SendMessage("OnPress", false, SendMessageOptions.DontRequireReceiver);
				nGUITrigger.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
				return;
			}
		}
#endif
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

	private Button GetButton()
	{
		Button refButton = null;
		if (m_ReferenceElement != null && m_ReferenceElement.activeInHierarchy)
			refButton = m_ReferenceElement.GetComponent<Button>();
		if (refButton == null && gameObject.activeInHierarchy)
			refButton = gameObject.GetComponent<Button>();

		return refButton;
	}

	//////////////////////////////////////////////////////////////////////////

#if ACCESS_NGUI
	private UIButton GetNGUIButton()
	{
		UIButton refButton = null;
		if (m_ReferenceElement != null)
			refButton = m_ReferenceElement.GetComponent<UIButton>();
		if (refButton == null)
			refButton = GetComponent<UIButton>();

		return refButton;
	}

	private UIEventTrigger GetNGUIEventTrigger()
	{
		UIEventTrigger refButton = null;
		if (m_ReferenceElement != null)
			refButton = m_ReferenceElement.GetComponent<UIEventTrigger>();
		if (refButton == null)
			refButton = GetComponent<UIEventTrigger>();

		return refButton;
	}
#endif

	//////////////////////////////////////////////////////////////////////////

	public override bool IsInteractable()
	{
		Button buttonComponent = GetButton();
		if (buttonComponent != null)
		{
			if (buttonComponent.enabled == false || buttonComponent.IsInteractable() == false)
				return false;
			else
				return true;
		}

		// NGUI
#if ACCESS_NGUI
		UIButton nGUIButtonComponent = GetNGUIButton();
		if (nGUIButtonComponent != null)
		{
			if (nGUIButtonComponent.enabled == false || nGUIButtonComponent.isEnabled == false)
				return false;
			else
				return true;
		}
		else
		{
			// There might be an event trigger on this instead of a regular UI button
			UIEventTrigger nGUIEventTrigger = GetNGUIEventTrigger();
			if (nGUIEventTrigger != null)
			{
				//Debug.Log("Found Event Trigger");
				if (nGUIEventTrigger.enabled && nGUIEventTrigger.isActiveAndEnabled)
					return true;
				else
					return false;
			}
		}

#endif

		// We couldn't find any buttons...
		return false;
	}

	//////////////////////////////////////////////////////////////////////////

	public override bool AutoFillTextLabel()
	{
		if (base.AutoFillTextLabel())
			return true;

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
		}

		// TextMesh Pro
		//////////////////////////////////////////////////////////////////////////
		if (!found)
		{
			var TMP_Label = GetTextMeshProLabelInChildren();
			if (TMP_Label != null)
			{
				m_Text = GetTextFromTextMeshPro(TMP_Label);
				found = true;
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
					m_Text = label.text;
					found = true;
				}
			}

			if (!found)
			{
				UILabel label = gameObject.GetComponentInChildren<UILabel>();
				if (label != null)
				{
					m_Text = label.text;
					found = true;
				}
			}
		}
#endif

		// if nothing, use the GameObject name
		if (!found)
			m_Text = gameObject.name;

		return found;
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

			// TextMesh Pro
			//////////////////////////////////////////////////////////////////////////
			if (!found)
			{
				var TMP_Label = GetTextMeshProLabelInChildren();
				if (TMP_Label != null)
				{
					m_NameLabel = TMP_Label.gameObject;
					found = true;
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

	protected override void OnHoverHighlight(bool enable)
	{
#if ACCESS_NGUI
		UIButton nGUIButton = GetNGUIButton();
		if (nGUIButton != null)
		{
			if (enable)
				nGUIButton.SetState(IsInteractable() ? UIButtonColor.State.Hover : UIButtonColor.State.Disabled, false);
			else
				nGUIButton.SetState(IsInteractable() ? UIButtonColor.State.Normal : UIButtonColor.State.Disabled, false);
		}
#else
		Button button = GetButton();
		if (button != null)
		{
			var pointer = new PointerEventData(EventSystem.current); // pointer event for Execute
			if (enable)
				button.OnPointerEnter(pointer);
			else
				button.OnPointerExit(pointer);
		}
#endif
	}

	//////////////////////////////////////////////////////////////////////////

}


