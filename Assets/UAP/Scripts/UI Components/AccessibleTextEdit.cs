using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[AddComponentMenu("Accessibility/UI/Accessible Text Edit")]
public class AccessibleTextEdit : UAP_BaseElement
{
#if ACCESS_NGUI
	private EventDelegate m_Callback = null;
#endif

	string prevText = "";
	string deltaText = "";

	//////////////////////////////////////////////////////////////////////////

	AccessibleTextEdit()
	{
		m_Type = AccessibleUIGroupRoot.EUIElement.ETextEdit;
	}

	//////////////////////////////////////////////////////////////////////////

	public override bool IsElementActive()
	{
		// Return whether this element is visible (and maybe usable)
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

	InputField GetInputField()
	{
		InputField refElement = null;
		if (m_ReferenceElement != null)
			refElement = m_ReferenceElement.GetComponent<InputField>();
		if (refElement == null)
			refElement = GetComponent<InputField>();

		return refElement;
	}

	//////////////////////////////////////////////////////////////////////////

#if ACCESS_NGUI
	private UIInput GetNGUIInputField()
	{
		UIInput refElement = null;
		if (m_ReferenceElement != null)
			refElement = m_ReferenceElement.GetComponent<UIInput>();
		if (refElement == null)
			refElement = GetComponent<UIInput>();

		return refElement;
	}
#endif

	//////////////////////////////////////////////////////////////////////////

	private Component GetTMPInputField()
	{
		Component refElement = null;
		if (m_ReferenceElement != null)
			refElement = m_ReferenceElement.GetComponent("TMP_InputField");
		if (refElement == null)
			refElement = GetComponent("TMP_InputField");

		return refElement;
	}

	//////////////////////////////////////////////////////////////////////////

	public override string GetCurrentValueAsText()
	{
		// If this is a password input, do NOT deliver the actual text back
		if (IsPassword())
			return UAP_AccessibilityManager.Localize_Internal("Keyboard_PasswordHidden");
		return GetValueFromEditBox();
	}

	private string GetValueFromEditBox()
	{
		// Unity UI
		InputField inputField = GetInputField();
		if (inputField != null)
			return inputField.text;

		// TextMesh Pro
		var tmpInputField = GetTMPInputField();
		if (tmpInputField != null)
		{
			var info = tmpInputField.GetType().GetProperty("text");
			if (info != null)
				return info.GetValue(tmpInputField, null) as string;
		}

#if ACCESS_NGUI
		// NGUI
		UIInput element = GetNGUIInputField();
		if (element != null)
			return element.value;
#endif

		return "";
	}

	//////////////////////////////////////////////////////////////////////////

	private bool IsPassword()
	{
		// Unity UI
		InputField inputField = GetInputField();
		if (inputField != null)
		{
			return (inputField.contentType == InputField.ContentType.Password);
		}

		// TextMesh Pro
		var tmpInputField = GetTMPInputField();
		if (tmpInputField != null)
		{
			var info = tmpInputField.GetType().GetProperty("contentType");
			if (info != null)
			{
				var value = (int)info.GetValue(tmpInputField, null);
				return value == (int)InputField.ContentType.Password;
			}
		}

#if ACCESS_NGUI
		// NGUI
		UIInput element = GetNGUIInputField();
		if (element != null)
		{
			return (element.inputType == UIInput.InputType.Password);
		}
#endif

		return false;
	}

	//////////////////////////////////////////////////////////////////////////

	public override bool IsInteractable()
	{
		// Unity UI
		InputField inputField = GetInputField();
		if (inputField != null)
		{
			if (inputField.enabled == false || inputField.interactable == false)
				return false;
			else
				return true;
		}

		// TextMesh Pro
		Behaviour tmpInputField = (Behaviour)GetTMPInputField();
		if (tmpInputField != null)
		{
			if (tmpInputField.enabled == false)
				return false;
			var info = tmpInputField.GetType().GetProperty("interactable");
			if (info != null)
			{
				var isInteractable = (bool)info.GetValue(tmpInputField, null);
				return isInteractable;
			}
			return true;
		}

#if ACCESS_NGUI
		// NGUI
		UIInput element = GetNGUIInputField();
		if (element != null)
		{
			if (element.enabled == false || element.isActiveAndEnabled == false)
				return false;
			else
				return true;
		}

#endif

		// We couldn't find any buttons...
		return false;
	}

	//////////////////////////////////////////////////////////////////////////

	protected override void OnInteract()
	{
		// Determine whether to use custom virtual keyboard or not - use virtual keyboard only on supported platforms and languages, and respect global setting
		bool useBuiltinVirtualKeyboard = UAP_AccessibilityManager.ShouldUseBuiltInKeyboard();
		bool isPassword = IsPassword();
		bool isMultiline = false;

		// Unity UI
		InputField inputField = GetInputField();
		if (inputField != null)
		{
			prevText = inputField.text;
			deltaText = prevText;

			// Give the Text Field the focus (to bring up the keyboard entry)
			if (!useBuiltinVirtualKeyboard)
			{
				inputField.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
				EventSystem.current.SetSelectedGameObject(inputField.gameObject);
			}
			else
			{
				isMultiline = inputField.multiLine;
			}
		}

		// TextMesh Pro
		Behaviour tmpInputField = (Behaviour)GetTMPInputField();
		if (tmpInputField != null)
		{
			var textInfo = tmpInputField.GetType().GetProperty("text");
			prevText = textInfo.GetValue(tmpInputField, null) as string;
			deltaText = prevText;

			// Give the Text Field the focus (to bring up the keyboard entry)
			if (!useBuiltinVirtualKeyboard)
			{
				var info = tmpInputField.GetType().GetProperty("onValueChanged");
				if (info != null)
				{
					var onValueChanged = (UnityEvent<string>)info.GetValue(tmpInputField, null);
					onValueChanged.AddListener(delegate { ValueChangeCheck(); });
					EventSystem.current.SetSelectedGameObject(tmpInputField.gameObject);
				}
			}
			else
			{
				var info = tmpInputField.GetType().GetProperty("multiLine");
				if (info != null)
				{
					isMultiline = (bool)info.GetValue(tmpInputField, null);
				}
			}
		}

#if ACCESS_NGUI
		// NGUI
		UIInput element = GetNGUIInputField();
		if (element != null)
		{
			//Debug.Log("Enabling NGUI Input field");
			if (!useBuiltinVirtualKeyboard)
			{
				if (m_Callback == null)
					m_Callback = new EventDelegate(this, "ValueChangeCheck");
				element.onChange.Add(m_Callback);
				element.isSelected = true;
			}

			isMultiline = element.onReturnKey == UIInput.OnReturnKey.NewLine;

#pragma warning disable CS0618
			// Disabling warning for backwards compatibility. 
			// In newer NGUI versions, 'text' is being obsoleted
			// and replaced by 'value'. 
			// If the following code line does not compile, 
			// please replace it with the code one line below
			prevText = element.text;
			//prevText = element.value;
#pragma warning restore
			deltaText = prevText;
		}
#endif

		if (useBuiltinVirtualKeyboard)
		{
			UAP_VirtualKeyboard.ShowOnscreenKeyboard(prevText, isPassword ? UAP_VirtualKeyboard.EKeyboardMode.Password : UAP_VirtualKeyboard.EKeyboardMode.Default, !isPassword, isMultiline);
			UAP_VirtualKeyboard.SetOnFinishListener(OnInputFinished);
		}

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
		else 
			TouchScreenKeyboard.Open(prevText);
#endif
	}

	//////////////////////////////////////////////////////////////////////////

	private void OnInputFinished(string editedText, bool wasConfirmed)
	{
		// Set the text (if needed)
		if (wasConfirmed)
		{
			// Unity UI
			InputField inputField = GetInputField();
			if (inputField != null)
			{
				inputField.text = editedText;
				// Call the appropriate OnEndEdit function of Unity UI (developer might have set up callbacks) 
				inputField.onEndEdit.Invoke(editedText);
			}

			// TestMesh Pro
			var tmpInputField = GetTMPInputField();
			if (tmpInputField != null)
			{
				var textInfo = tmpInputField.GetType().GetProperty("text");
				if (textInfo != null)
				{
					textInfo.SetValue(tmpInputField, editedText, null);
					// Call the appropriate OnEndEdit function of Unity UI (developer might have set up callbacks) 
					var onEndEditInfo = tmpInputField.GetType().GetProperty("onEndEdit");
					if (onEndEditInfo != null)
					{
						var onEndEdit = (UnityEvent<string>)onEndEditInfo.GetValue(tmpInputField, null);
						onEndEdit.Invoke(editedText);
					}
				}
			}

#if ACCESS_NGUI
			// NGUI
		UIInput element = GetNGUIInputField();
		if (element != null)
		{
			element.value = editedText;
			// Call the appropriate onSubmit function of NGUI UIInput (developer might have set up callbacks) 
			element.Submit();
		}
#endif
			}

			// Conclude the interaction with the UAP Accessibility Manager
			UAP_AccessibilityManager.FinishCurrentInteraction();
	}

	//////////////////////////////////////////////////////////////////////////

	public void ValueChangeCheck()
	{
		string newText = "";
		string fullText = "";

		// Unity UI
		InputField inputField = GetInputField();
		if (inputField != null)
		{
			newText = inputField.text;
		}

		// TestMesh Pro
		var tmpInputField = GetTMPInputField();
		if (tmpInputField != null)
		{
			var info = tmpInputField.GetType().GetProperty("text");
			if (info != null)
				newText = info.GetValue(tmpInputField, null) as string;
		}

#if ACCESS_NGUI
		// NGUI
		UIInput element = GetNGUIInputField();
		if (element != null)
		{
			newText = element.value;
		}
#endif

		fullText = newText;
		// Remove the previous text from the string to get just the new bits
		if (newText.StartsWith(deltaText))
			newText = newText.Substring(deltaText.Length);
		if (newText.Length > 0)
			UAP_AccessibilityManager.Say(newText);
		deltaText = fullText;
	}

	//////////////////////////////////////////////////////////////////////////

	protected override void OnInteractAbort()
	{
		// Unity UI
		InputField inputField = GetInputField();
		if (inputField != null)
		{
			inputField.onValueChanged.RemoveListener(delegate { ValueChangeCheck(); });

			// Restore previous value
			inputField.text = prevText;
		}

		// TestMesh Pro
		var tmpInputField = GetTMPInputField();
		if (tmpInputField != null)
		{
			var info = tmpInputField.GetType().GetProperty("onValueChanged");
			if (info != null)
			{
				var onValueChanged = (UnityEvent<string>)info.GetValue(tmpInputField, null);
				onValueChanged.AddListener(delegate { ValueChangeCheck(); });

				// Restore previous value
				var textInfo = tmpInputField.GetType().GetProperty("text");
				if (textInfo != null)
					textInfo.SetValue(tmpInputField, prevText, null);
			}
		}

#if ACCESS_NGUI
		// NGUI
		UIInput element = GetNGUIInputField();
		if (element != null)
		{
			element.onChange.Remove(m_Callback);
			element.RemoveFocus();
			//element.isSelected = false;

			// Restore previous value
			element.value = prevText;
		}
#endif

		prevText = "";
	}

	//////////////////////////////////////////////////////////////////////////

	protected override void OnInteractEnd()
	{
		// Unity UI
		InputField inputField = GetInputField();
		if (inputField != null)
		{
			inputField.onValueChanged.RemoveListener(delegate { ValueChangeCheck(); });
		}

		// TestMesh Pro
		var tmpInputField = GetTMPInputField();
		if (tmpInputField != null)
		{
			var info = tmpInputField.GetType().GetProperty("onValueChanged");
			if (info != null)
			{
				var onValueChanged = (UnityEvent<string>)info.GetValue(tmpInputField, null);
				onValueChanged.RemoveListener(delegate { ValueChangeCheck(); });
			}
		}

#if ACCESS_NGUI
		// NGUI
		UIInput element = GetNGUIInputField();
		if (element != null)
		{
			element.onChange.Remove(m_Callback);
			element.RemoveFocus();
			//element.isSelected = false;
		}
#endif

	}

	//////////////////////////////////////////////////////////////////////////

	protected override void OnHoverHighlight(bool enable)
	{
		// Unity UI
		InputField inputField = GetInputField();
		if (inputField != null)
		{
			var pointer = new PointerEventData(EventSystem.current); // pointer event for Execute
			if (enable)
				inputField.OnPointerEnter(pointer);
			else
				inputField.OnPointerExit(pointer);
		}

#if ACCESS_NGUI
		// NGUI
		UIInput element = GetNGUIInputField();
		if (element != null)
		{
			// There is currently no hover effect on NGUI UIInputs
			//element.isSelected = true;
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
