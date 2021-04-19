using UnityEngine;
using System.Collections.Generic;

using UnityEngine.UI;
using UnityEngine.EventSystems;

[AddComponentMenu("Accessibility/UI/Accessible Dropdown")]
public class AccessibleDropdown : UAP_BaseElement
{
	[Header("Other")]
	//! List of audio files that correspond to the dropdown list entries
	public List<AudioClip> m_ValuesAsAudio = new List<AudioClip>();

	//! Used if the interaction is aborted
	private int prevSelectedIndex = -1;
#if ACCESS_NGUI
	private int activeSelectionIndex = 0;
#endif

	//////////////////////////////////////////////////////////////////////////

	AccessibleDropdown()
	{
		m_Type = AccessibleUIGroupRoot.EUIElement.EDropDown;
	}

	//////////////////////////////////////////////////////////////////////////

	void Awake()
	{
#if ACCESS_NGUI
		UIPopupList nGUIElement = GetNGUIDropdown();
		if (nGUIElement != null)
		{
			if (nGUIElement.value != null)
				activeSelectionIndex = nGUIElement.items.IndexOf(nGUIElement.value);
		}
#endif
	}

	//////////////////////////////////////////////////////////////////////////

	Dropdown GetDropdown()
	{
		Dropdown refElement = null;
		if (m_ReferenceElement != null)
			refElement = m_ReferenceElement.GetComponent<Dropdown>();
		if (refElement == null)
			refElement = GetComponent<Dropdown>();

		return refElement;
	}

	//////////////////////////////////////////////////////////////////////////

#if ACCESS_NGUI
	UIPopupList GetNGUIDropdown()
	{
		UIPopupList refElement = null;
		if (m_ReferenceElement != null)
			refElement = m_ReferenceElement.GetComponent<UIPopupList>();
		if (refElement == null)
			refElement = GetComponent<UIPopupList>();

		return refElement;
	}
#endif

	//////////////////////////////////////////////////////////////////////////

	Component GetTMPDropDown()
	{
		Component refElement = null;
		if (m_ReferenceElement != null)
			refElement = m_ReferenceElement.GetComponent("TMP_Dropdown");
		if (refElement == null)
			refElement = GetComponent("TMP_Dropdown");

		return refElement;
	}

	//////////////////////////////////////////////////////////////////////////

	public override bool IsInteractable()
	{
		return IsElementActive();
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

	public override string GetCurrentValueAsText()
	{
		// Unity UI
		Dropdown dropDown = GetDropdown();
		if (dropDown != null)
			return dropDown.captionText.text;

		// TextMesh Pro
		var tmpDropDown = GetTMPDropDown();
		if (tmpDropDown != null)
		{
			var textMeshProLabel = tmpDropDown.GetType().GetProperty("captionText");
			if (textMeshProLabel != null)
			{
				var tmpProLabel = (textMeshProLabel.GetValue(tmpDropDown, null) as Component);
				return GetTextFromTextMeshPro(tmpProLabel);
			}
		}

#if ACCESS_NGUI
		// NGUI
		UIPopupList nGUIElement = GetNGUIDropdown();
		if (nGUIElement != null)
		{
			if ((activeSelectionIndex < 0) || nGUIElement.items[activeSelectionIndex] == null)
				return "Nothing selected";
			return nGUIElement.items[activeSelectionIndex];
		}
#endif

		return "";
	}

	//////////////////////////////////////////////////////////////////////////

	public override AudioClip GetCurrentValueAsAudio()
	{
		Dropdown dropDown = GetDropdown();
		if (dropDown != null)
		{
			if (m_ValuesAsAudio.Count > dropDown.value)
				return m_ValuesAsAudio[dropDown.value];
		}

#if ACCESS_NGUI
		UIPopupList nGUIElement = GetNGUIDropdown();
		if (nGUIElement != null)
		{
			if (activeSelectionIndex >= 0 && m_ValuesAsAudio.Count > activeSelectionIndex)
				return m_ValuesAsAudio[activeSelectionIndex];
		}
#endif

		return null;
	}

	//////////////////////////////////////////////////////////////////////////

	protected override void OnInteract()
	{
		// Unity UI
		Dropdown dropDown = GetDropdown();
		if (dropDown != null)
		{
			prevSelectedIndex = dropDown.value;
			dropDown.Show();
		}

		// TextMesh Pro
		var tmpDropDown = GetTMPDropDown();
		if (tmpDropDown != null)
		{
			var pointerClickHandler = tmpDropDown.gameObject.GetComponent<IPointerClickHandler>();
			if (pointerClickHandler != null)
			{
				var pointer = new PointerEventData(EventSystem.current);
				pointerClickHandler.OnPointerClick(pointer);
			}
		}

#if ACCESS_NGUI
		// NGUI
		UIPopupList nGUIElement = GetNGUIDropdown();
		if (nGUIElement != null)
		{
			prevSelectedIndex = activeSelectionIndex;
			nGUIElement.Show();
		}
#endif
	}

	//////////////////////////////////////////////////////////////////////////

	protected override void OnInteractEnd()
	{
		prevSelectedIndex = -1;

		// Unity UI
		Dropdown dropDown = GetDropdown();
		if (dropDown != null)
			dropDown.Hide();

		// TextMesh Pro
		var tmpDropDown = GetTMPDropDown();
		if (tmpDropDown != null)
		{
			var cancelHandler = tmpDropDown.gameObject.GetComponent<ICancelHandler>();
			if (cancelHandler != null)
			{
				var eventData = new BaseEventData(EventSystem.current);
				cancelHandler.OnCancel(eventData);
			}
		}

#if ACCESS_NGUI
		// NGUI
		UIPopupList nGUIElement = GetNGUIDropdown();
		if (nGUIElement != null)
			nGUIElement.CloseSelf();
#endif
	}

	//////////////////////////////////////////////////////////////////////////

	protected override void OnInteractAbort()
	{
		// Restore previous value
		if (prevSelectedIndex != -1)
		{
			Dropdown dropDown = GetDropdown();
			if (dropDown != null)
				dropDown.value = prevSelectedIndex;

			// TextMesh Pro
			var tmpDropDown = GetTMPDropDown();
			if (tmpDropDown != null)
			{
				var info = tmpDropDown.GetType().GetProperty("value");
				if (info != null)
				{
					info.SetValue(tmpDropDown, prevSelectedIndex, null);
				}
			}

#if ACCESS_NGUI
			// NGUI
			UIPopupList nGUIElement = GetNGUIDropdown();
			if (nGUIElement != null)
			{
				nGUIElement.Set(nGUIElement.items[prevSelectedIndex]);
				activeSelectionIndex = prevSelectedIndex;
			}
#endif

		}

		OnInteractEnd();
	}

	//////////////////////////////////////////////////////////////////////////

	public override bool Increment()
	{
		// Unity UI
		Dropdown dropDown = GetDropdown();
		if (dropDown != null)
		{
			if (dropDown.value == dropDown.options.Count - 1)
				return false;

			++dropDown.value;
			return true;
		}

		// TextMesh Pro
		var tmpDropDown = GetTMPDropDown();
		if (tmpDropDown != null)
		{
			var valueInfo = tmpDropDown.GetType().GetProperty("value");
			if (valueInfo != null)
			{
				int valueIndex = (int)valueInfo.GetValue(tmpDropDown, null);
				int optionsCount = GetItemCount();

				if (valueIndex == optionsCount - 1)
					return false;

				valueInfo.SetValue(tmpDropDown, valueIndex + 1, null);
				return true;
			}
		}

#if ACCESS_NGUI
		// NGUI
		UIPopupList nGUIElement = GetNGUIDropdown();
		if (nGUIElement != null)
		{
			if (activeSelectionIndex == nGUIElement.items.Count - 1)
				return false;

			nGUIElement.gameObject.SendMessage("OnNavigate", KeyCode.DownArrow);
			++activeSelectionIndex;
			nGUIElement.Set(nGUIElement.items[activeSelectionIndex]);
			nGUIElement.CloseSelf();
			nGUIElement.Show();
			return true;
		}
#endif

		return false;
	}

	//////////////////////////////////////////////////////////////////////////

	public override bool Decrement()
	{
		// Unity UI
		Dropdown dropDown = GetDropdown();
		if (dropDown != null)
		{
			if (dropDown.value == 0)
				return false;

			--dropDown.value;
			return true;
		}

		// TextMesh Pro
		var tmpDropDown = GetTMPDropDown();
		if (tmpDropDown != null)
		{
			var optionsInfo = tmpDropDown.GetType().GetProperty("options");
			var valueInfo = tmpDropDown.GetType().GetProperty("value");
			if (optionsInfo != null && valueInfo != null)
			{
				int valueIndex = (int)valueInfo.GetValue(tmpDropDown, null);
				if (valueIndex == 0)
					return false;

				valueInfo.SetValue(tmpDropDown, valueIndex - 1, null);
				return true;
			}
		}

#if ACCESS_NGUI
		// NGUI
		UIPopupList nGUIElement = GetNGUIDropdown();
		if (nGUIElement != null)
		{
			if (activeSelectionIndex <= 0)
				return false;

			nGUIElement.gameObject.SendMessage("OnNavigate", KeyCode.UpArrow);
			--activeSelectionIndex;
			nGUIElement.Set(nGUIElement.items[activeSelectionIndex]);
			nGUIElement.CloseSelf();
			nGUIElement.Show();
			return true;
		}
#endif

		return false;
	}

	//////////////////////////////////////////////////////////////////////////

	public int GetItemCount()
	{
		// Unity UI
		Dropdown dropDown = GetDropdown();
		if (dropDown != null)
			return dropDown.options.Count;

		// TextMesh Pro
		var tmpDropDown = GetTMPDropDown();
		if (tmpDropDown != null)
		{
			var optionsInfo = tmpDropDown.GetType().GetProperty("options");
			if (optionsInfo != null)
			{
				var optionsList = optionsInfo.GetValue(tmpDropDown, null);
				var countInfo = optionsList.GetType().GetProperty("Count");
				if (countInfo != null)
					return (int)countInfo.GetValue(optionsList, null);
			}
		}

#if ACCESS_NGUI
		// NGUI
		UIPopupList nGUIElement = GetNGUIDropdown();
		if (nGUIElement != null)
			return nGUIElement.items.Count;
#endif

		return 0;
	}

	//////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Returns the 1-based item index of the current selection.
	/// The first index will be 1, not 0.
	/// </summary>
	/// <returns></returns>
	public int GetSelectedItemIndex()
	{
		// Unity UI
		Dropdown dropDown = GetDropdown();
		if (dropDown != null)
			return dropDown.value + 1;

		// TextMesh Pro
		var tmpDropDown = GetTMPDropDown();
		if (tmpDropDown != null)
		{
			var info = tmpDropDown.GetType().GetProperty("value");
			if (info != null)
			{
				return ((int)info.GetValue(tmpDropDown, null)) + 1;
			}
		}

#if ACCESS_NGUI
		// NGUI
		UIPopupList nGUIElement = GetNGUIDropdown();
		if (nGUIElement != null)
			return activeSelectionIndex + 1;
#endif

		return 0;
	}

	//////////////////////////////////////////////////////////////////////////

	protected override void OnHoverHighlight(bool enable)
	{
		// Unity UI
		Dropdown dropDown = GetDropdown();
		if (dropDown != null)
		{
			var pointer = new PointerEventData(EventSystem.current); // pointer event for Execute
			if (enable)
				dropDown.OnPointerEnter(pointer);
			else
				dropDown.OnPointerExit(pointer);
		}

#if ACCESS_NGUI
		// NGUI
		UIPopupList nGUIElement = GetNGUIDropdown();
		if (nGUIElement != null)
		{
			UIButton nGUIButton = nGUIElement.gameObject.GetComponent<UIButton>();
			if (nGUIButton != null)
			{
				if (enable)
					nGUIButton.SetState(IsInteractable() ? UIButtonColor.State.Hover : UIButtonColor.State.Disabled, false);
				else
					nGUIButton.SetState(IsInteractable() ? UIButtonColor.State.Normal : UIButtonColor.State.Disabled, false);
			}
		}
#endif
	}

	//////////////////////////////////////////////////////////////////////////

}
