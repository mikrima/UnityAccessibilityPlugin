using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Serialization;

/// <summary>This is the base for all accessibility UI components. Use this to set values directly.</summary>
//[AddComponentMenu("Accessibility/Core/UAP Base Element")]
public abstract class UAP_BaseElement : MonoBehaviour
{
	[System.Serializable]
	public class UAPBoolCallback : UnityEvent<bool>
	{
	}

	public enum EHighlightSource
	{
		Internal = 0, // auto select/deselect/item removed/pause/unpause/script
		UserInput, // Swipe, Keyboard 
		TouchExplore,
	}

	[System.Serializable]
	public class UAPHighlightCallback : UnityEvent<bool, EHighlightSource>
	{
	}

	public UnityEvent m_OnInteractionStart = new UnityEvent();
	public UnityEvent m_OnInteractionEnd = new UnityEvent();
	public UnityEvent m_OnInteractionAbort = new UnityEvent();

	public bool m_ForceStartHere = false;
	public int m_ManualPositionOrder = -1;
	public GameObject m_ManualPositionParent = null;
	public bool m_UseTargetForOutline = false;

	// #UAP: These should be readonly - they are only for debugging purposes
	public int m_PositionOrder = 0;
	public int m_SecondaryOrder = 0;
	public Vector2 m_Pos = new Vector2(0, 0);

	[Header("Element Name")]
	public AudioClip m_TextAsAudio = null;

	public string m_Prefix = "";
	public bool m_PrefixIsLocalizationKey = false;
	public bool m_PrefixIsPostFix = false; //! Only relevant if this is not a combination string
	public bool m_FilterText = true; //! This will remove formatting from text, for example <b> or [#FF3C3C] and [-]

	/// <summary>
	/// This variable contains the text that will be read aloud if this UI element receives focus.
	/// This variable will only be used if the plugin is not reading text directly from a text label.
	/// </summary>
	public string m_Text = "";

	/// <summary>
	/// Label that contains the name or description of the edit box.
	/// Use this to directly read localized content.<br><br>
	/// </summary>
	public GameObject m_NameLabel = null;
	public List<GameObject> m_AdditionalNameLabels = null;
	public GameObject[] m_TestList = null;

	/// <summary>
	/// Uses the text inside m_NameLabel and requests translation.
	/// </summary>
	[FormerlySerializedAs("m_IsNGUILocalizationKey")]
	public bool m_IsLocalizationKey = false;

	//! Only works if there is a text label component either on this GameObject or
	//! set as a reference.
	public bool m_TryToReadLabel = true;

	//! Only set this if the accessibility component doesn't sit on the same GameObject
	public GameObject m_ReferenceElement = null;
	//! In very specific circumstances it can make sense to deliberately prevent using VoiceOver from speaking this element.
	public bool m_AllowVoiceOver = true;
	//! In very specific circumstances you might want to suppress the reading of the type
	public bool m_ReadType = true;

	[HideInInspector]
	public bool m_WasJustAdded = true;

	[HideInInspector]
	public AccessibleUIGroupRoot.EUIElement m_Type = AccessibleUIGroupRoot.EUIElement.EUndefined;

	// My owner
	AccessibleUIGroupRoot AUIContainer = null;

	public bool m_CustomHint = false;
	public AudioClip m_HintAsAudio = null;
	public string m_Hint = "";
	public bool m_HintIsLocalizationKey = false;

	[HideInInspector]
	public bool m_IsInsideScrollView = false;

	//private bool m_ContainerRefreshNeeded = false;
	private bool m_HasStarted = false;

	[HideInInspector]
	public bool m_IsInitialized = false;

	public UAPHighlightCallback m_CallbackOnHighlight = new UAPHighlightCallback();

	//////////////////////////////////////////////////////////////////////////

	void Reset()
	{
		AutoFillTextLabel();
		m_IsInitialized = false;
		Initialize();
	}

	//////////////////////////////////////////////////////////////////////////

	public void Initialize()
	{
		if (m_IsInitialized)
			return;

		AutoInitialize();

		if (!m_IsLocalizationKey)
			m_Text = GetMainText();
		m_TryToReadLabel = (m_NameLabel != null);

		m_IsInitialized = true;
	}

	/// <summary>
	/// Do not call this function directly, use Initialize() instead.
	/// </summary>
	protected virtual void AutoInitialize()
	{
		if (m_IsInitialized)
			return;
	}

	//////////////////////////////////////////////////////////////////////////

	void OnEnable()
	{
		if (!m_HasStarted)
			return;

		RegisterWithContainer();
		// Tell container to earmark a refresh next frame
		//		m_ContainerRefreshNeeded = true;
		CancelInvoke("RefreshContainerNextFrame");
		Invoke("RefreshContainerNextFrame", 0.5f);
		//		RefreshContainerNextFrame();
	}

	//////////////////////////////////////////////////////////////////////////

	void Start()
	{
		m_HasStarted = true;
		RegisterWithContainer();
		// Tell container to earmark a refresh next frame
		//		m_ContainerRefreshNeeded = true;
		CancelInvoke("RefreshContainerNextFrame");
		Invoke("RefreshContainerNextFrame", 0.5f);
		//		RefreshContainerNextFrame();
	}

	//////////////////////////////////////////////////////////////////////////

	private void GetContainer()
	{
		Transform t = transform;
		while (t != null && AUIContainer == null)
		{
			AUIContainer = t.gameObject.GetComponent<AccessibleUIGroupRoot>();
			t = t.parent;
		}
	}

	//////////////////////////////////////////////////////////////////////////

	internal AccessibleUIGroupRoot GetUIGroupContainer()
	{
		if (AUIContainer == null)
			GetContainer();

		return AUIContainer;
	}

	//////////////////////////////////////////////////////////////////////////

	private void RegisterWithContainer()
	{
		GetContainer();

		if (AUIContainer == null)
		{
			LogErrorNoValidParent();
			return;
		}

		AUIContainer.CheckForRegister(this);

		UAP_SelectionGroup[] groups = GetComponentsInParent<UAP_SelectionGroup>();
		foreach (UAP_SelectionGroup group in groups)
			group.AddElement(this);
	}

	//////////////////////////////////////////////////////////////////////////

	public void SetAsStartItem()
	{
		m_ForceStartHere = true;

		GetContainer();

		if (AUIContainer == null)
		{
			LogErrorNoValidParent();
			return;
		}

		AUIContainer.SetAsStartItem(this);
	}

	//////////////////////////////////////////////////////////////////////////

	void RefreshContainerNextFrame()
	{
		GetContainer();

		if (AUIContainer == null)
		{
			LogErrorNoValidParent();
			return;
		}

		AUIContainer.RefreshNextUpdate();
	}

	private void LogErrorNoValidParent()
	{
		// Many game objects have the same name, e.g. 'Button'
		// To create a little more helpful debug output, list the full hierarchy path here
		string fullpath = gameObject.name;
		Transform p = gameObject.transform.parent;
		while (p != null)
		{
			fullpath = p.name + "/" + fullpath;
			p = p.parent;
		}
		Debug.LogError("[Accessibility] Could not find an Accessibility UI Container in any parent object of " + gameObject.name + "! This UI element will be unaccessible. Full Path: " + fullpath);
	}

	//////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Whether this is a 3D object (actual 3D, not on a UI canvas or NGUI root)
	/// </summary>
	/// <returns></returns>
	public virtual bool Is3DElement()
	{
		return false;
	}


	/*
	void Update()
	{
		if (m_ContainerRefreshNeeded)
		{
			m_ContainerRefreshNeeded = false;
			// Tell container to earmark a refresh next frame
			RefreshContainerNextFrame();
		}
	}
	*/

	//////////////////////////////////////////////////////////////////////////

	public virtual bool AutoFillTextLabel()
	{
		bool found = false;

		if (m_NameLabel != null)
		{
			// Unity UI
			Text label = m_NameLabel.GetComponent<Text>();
			if (label != null)
			{
				m_Text = label.text;
				found = true;
			}

			// TextMesh Pro
			if (!found)
			{
				var TMP_Label = m_NameLabel.GetComponent("TMP_Text");
				if (TMP_Label != null)
				{
					m_Text = GetTextFromTextMeshPro(TMP_Label);
					found = true;
				}
			}

#if ACCESS_NGUI
			// NGUI
			UILabel nGUILabel = m_NameLabel.GetComponent<UILabel>();
			if (nGUILabel != null)
			{
				m_Text = nGUILabel.text;
				found = true;
			}
#endif
		}

		if (!found)
			m_Text = gameObject.name;

		return found;
	}

	//////////////////////////////////////////////////////////////////////////

	void OnDestroy()
	{
		//	Debug.Log("Destroying Element" + gameObject.name);
		if (AUIContainer != null)
		{
			//Debug.Log("Unregstering " + gameObject.name);
			AUIContainer.UnRegister(this);
		}

		UAP_SelectionGroup[] groups = GetComponentsInParent<UAP_SelectionGroup>();
		foreach (UAP_SelectionGroup group in groups)
			group.RemoveElement(this);

	}

	//////////////////////////////////////////////////////////////////////////

	public virtual bool IsInteractable()
	{
		return false;
	}

	//////////////////////////////////////////////////////////////////////////

	public void Interact()
	{
		// Callback to any listeners
		m_OnInteractionStart.Invoke();

		OnInteract();
	}

	protected virtual void OnInteract()
	{
	}

	//////////////////////////////////////////////////////////////////////////

	public void InteractEnd()
	{
		// Callback to any listeners
		m_OnInteractionEnd.Invoke();

		OnInteractEnd();
	}

	protected virtual void OnInteractEnd()
	{
	}

	//////////////////////////////////////////////////////////////////////////

	public void InteractAbort()
	{
		// Callback to any listeners
		m_OnInteractionAbort.Invoke();

		OnInteractAbort();
	}

	//! End the interaction but - if it makes sense - restore previous value
	protected virtual void OnInteractAbort()
	{
		// This is the default behavior, for safety reasons.
		// Override this function if you need to restore a previous value
		OnInteractEnd();
	}

	//////////////////////////////////////////////////////////////////////////

	protected string CombinePrefix(string text)
	{
		string prefix = m_PrefixIsLocalizationKey ? UAP_AccessibilityManager.Localize(m_Prefix) : m_Prefix;
		if (prefix.Length == 0)
			return text;

		if (prefix.IndexOf("{0}") != -1)
		{
			string resultText = prefix;
			resultText = resultText.Replace("{0}", text);

			if (m_AdditionalNameLabels != null && m_AdditionalNameLabels.Count > 0)
			{
				for (int i = 0; i < m_AdditionalNameLabels.Count; i++)
				{
					if (resultText.IndexOf("{" + (i + 1).ToString("0") + "}") != -1)
						resultText = resultText.Replace("{" + (i + 1).ToString("0") + "}", GetLabelText(m_AdditionalNameLabels[i]));
				}
			}

			return resultText;
		}

		if (m_PrefixIsPostFix)
			return text + " " + prefix;
		return prefix + " " + text;
	}

	//////////////////////////////////////////////////////////////////////////

	static public string FilterText(string text)
	{
		// Filter formatting information from text to prevent it from being read out
		// Remove entire substrings
		RemoveSubsting(ref text, "[-]");
		RemoveSubsting(ref text, "<b>");
		RemoveSubsting(ref text, "</b>");
		RemoveSubsting(ref text, "<B>");
		RemoveSubsting(ref text, "</B>");
		RemoveSubsting(ref text, "<u>");
		RemoveSubsting(ref text, "</u>");
		RemoveSubsting(ref text, "<U>");
		RemoveSubsting(ref text, "</U>");
		RemoveSubsting(ref text, "<i>");
		RemoveSubsting(ref text, "</i>");
		RemoveSubsting(ref text, "<I>");
		RemoveSubsting(ref text, "</I>");

		// Remove color information
		// NGUI color format: [53e2fe] (= 6 chars inside square brackets)
		int testIndex = 0;
		testIndex = text.IndexOf('[', testIndex);
		while ((testIndex > -1) && (text.Length > (testIndex + 7)))
		{
			// Is this a color?
			if (text[testIndex + 7] == ']')
			{
				// It's a color, let's remove it
				text = text.Remove(testIndex, 8);
			}
			else
			{
				++testIndex;
			}
			testIndex = text.IndexOf('[', testIndex);
		}

		// #UAP Text filtering Unity color formatting
		// UGUI color format: <color=X>text</color>


		return text;
	}

	private static void RemoveSubsting(ref string text, string substring)
	{
		int index = text.LastIndexOf(substring);
		while (index >= 0)
		{
			text = text.Replace(substring, "");
			index = text.LastIndexOf(substring);
		}
	}

	//////////////////////////////////////////////////////////////////////////

	public string GetTextToRead()
	{
		string textToRead = GetMainText();
		if (m_FilterText)
			textToRead = FilterText(textToRead);
		return textToRead;
	}

	protected virtual string GetMainText()
	{
		if (m_TryToReadLabel)
			AutoFillTextLabel();

		if (IsNameLocalizationKey())
			return CombinePrefix(UAP_AccessibilityManager.Localize(m_Text));

		// Only use prefix if there is a text label associated
		if (m_TryToReadLabel)
			return CombinePrefix(m_Text);
		else
			return m_Text;
	}

	//////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Convenience function to set the text on this element manually and disable the automatic label reading at the same time
	/// Text is expected to be localized (if applicable). This function will disable the setting treating the text as a localization key
	/// </summary>
	/// <param name="itemText"></param>
	public void SetCustomText(string itemText)
	{
		m_TryToReadLabel = false;
		m_Text = itemText;
		m_IsLocalizationKey = false;
	}

	//////////////////////////////////////////////////////////////////////////

	public virtual string GetCurrentValueAsText()
	{
		return "";
	}

	//////////////////////////////////////////////////////////////////////////

	public virtual AudioClip GetCurrentValueAsAudio()
	{
		return null;
	}

	//////////////////////////////////////////////////////////////////////////

	public virtual bool IsElementActive()
	{
		if (!enabled)
			return false;

		if (!gameObject.activeInHierarchy)
			return false;

		return true;
	}

	//////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Call this function to force select this UI element.
	/// By default, if the element is already selected before this function was called,
	/// the element is not read out aloud again. If you want to force the repetition, 
	/// pass true as a parameter.
	/// </summary>
	/// <param name="forceRepeatItem">If true, the item will be read aloud again even if 
	/// it was already selected. Has no effect if the item wasn't selected prior to this function call.</param>
	public bool SelectItem(bool forceRepeatItem = false)
	{
		if (!IsElementActive())
		{
			if (UAP_AccessibilityManager.IsEnabled())
				Debug.LogWarning("[Accessibility] Trying to select element '" + GetMainText() + "' (" + gameObject.name + ") but the element is not active/interactable/visible.");
			return false;
		}

		if (!m_HasStarted)
		{
			// #UAP: delay the rest of this function until Start()
			//return;
		}
				
		return SelectItem_Internal(forceRepeatItem);
	}

	//////////////////////////////////////////////////////////////////////////

	private bool SelectItem_Internal(bool forceRepeatItem)
	{
		if (AUIContainer == null)
		{
			// #UAP: If this doesn't work, delay the rest of this function until Start()
			RegisterWithContainer();
			if (AUIContainer == null)
			{
				Debug.LogWarning("[Accessibility] SelectItem: " + gameObject.name + " is not placed within an Accessibility UI container. Can't be selected. Aborting.");
				return false;
			}
		}

		// Notify the parent container, so that it can notify the manager
		return AUIContainer.SelectItem(this, forceRepeatItem);
	}

	//////////////////////////////////////////////////////////////////////////

	public virtual bool Increment()
	{
		return false;
	}

	//////////////////////////////////////////////////////////////////////////

	public virtual bool Decrement()
	{
		return false;
	}

	//////////////////////////////////////////////////////////////////////////

	public virtual void HoverHighlight(bool enable, EHighlightSource selectionSource)
	{
		EventTrigger eventTrigger = null;
		if (m_ReferenceElement != null && m_ReferenceElement.activeInHierarchy)
			eventTrigger = m_ReferenceElement.GetComponent<EventTrigger>();
		if (eventTrigger == null && gameObject.activeInHierarchy)
			eventTrigger = gameObject.GetComponent<EventTrigger>();
		if (eventTrigger != null)
		{
			if (enable)
				eventTrigger.OnSelect(new BaseEventData(EventSystem.current) { selectedObject = eventTrigger.gameObject });
			else
				eventTrigger.OnDeselect(new BaseEventData(EventSystem.current) { selectedObject = eventTrigger.gameObject });
		}

		OnHoverHighlight(enable);

		// Callback to listeners, if any
		m_CallbackOnHighlight.Invoke(enable, selectionSource);
	}

	protected virtual void OnHoverHighlight(bool enable)
	{
		// Derived classes will send Hover notification to that UI element (uGUI and NGUI) if applicable (for example buttons)
	}

	//////////////////////////////////////////////////////////////////////////

	public GameObject GetTargetGameObject()
	{
		if (m_ReferenceElement != null)
			return m_ReferenceElement.gameObject;

		return gameObject;
	}

	//////////////////////////////////////////////////////////////////////////

	public bool IsNameLocalizationKey()
	{
		return m_IsLocalizationKey;
	}

	//////////////////////////////////////////////////////////////////////////

	public string GetCustomHint()
	{
		if (m_HintIsLocalizationKey)
			return UAP_AccessibilityManager.Localize(m_Hint);
		return m_Hint;
	}

	/// <summary>
	/// Sets a custom hint text for this UI element, overriding the default text.
	/// The purpose of this text is to tell the player how to use the UI element, and give
	/// additional information as to what the element does. Most of the time, changing this is NOT needed.
	/// A default hint for a button (on mobile) would for example be 'Double tap to activate'.
	/// It might makes sense to change this to 'Double tap to sell this inventory item'
	/// Only interactive UI elements support hint texts.
	/// </summary>
	/// <returns></returns>
	public void SetCustomHintText(string hintText, bool isLocalizationKey = false)
	{
		m_CustomHint = true;
		m_Hint = hintText;
		m_HintIsLocalizationKey = isLocalizationKey;
	}

	public void ResetHintText()
	{
		m_CustomHint = false;
	}

	//////////////////////////////////////////////////////////////////////////

	protected string GetTextFromTextMeshPro(Component textMeshProLabel)
	{
		if (textMeshProLabel == null)
			return null;
		string fromTMPLabel = null;
		var info = textMeshProLabel.GetType().GetProperty("text");
		if (info != null)
			fromTMPLabel = info.GetValue(textMeshProLabel, null) as string;
		return fromTMPLabel;
	}

	//////////////////////////////////////////////////////////////////////////

	protected virtual string GetLabelText(GameObject go)
	{
		if (go == null)
			return "";

		// Unity UI
		Text label = go.GetComponent<Text>();
		if (label != null)
			return label.text;

		// TextMesh Pro
		string fromTMPLabel = GetTextFromTextMeshPro(go.GetComponent("TMP_Text"));
		if (!string.IsNullOrEmpty(fromTMPLabel))
			return fromTMPLabel;

#if ACCESS_NGUI
		// NGUI
		UILabel nGUIlabel = null;
		nGUIlabel = go.GetComponent<UILabel>();
		if (nGUIlabel != null)
			return nGUIlabel.text;

#endif

		return "";
	}

	//////////////////////////////////////////////////////////////////////////

	protected Component GetTextMeshProLabelInChildren()
	{
		foreach (Transform child in transform)
		{
			var tmpLabel = child.gameObject.GetComponent("TMP_Text");
			if (tmpLabel != null)
			{
				return tmpLabel;
			}
		}
		return null;
	}

	//////////////////////////////////////////////////////////////////////////

}
