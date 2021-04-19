using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Accessibility_InspectorShared : Editor
{
	protected GUIStyle myLabelStyle;
	protected GUIStyle labelRightAligned;
	protected GUIStyle myTextInputStyle;
	protected GUIStyle myHeadingStyle;
	protected GUIStyle myHeader2Style;
	protected GUIStyle myTopStyle;
	protected GUIStyle myTextAreaInputStyle;
	protected GUIStyle myEnumStyle;
	bool stylesInitialized = false;


	// Naming
	static bool showNaming = true;
	static bool showGroupNaming = false;
	private SerializedProperty m_NameLabel;
	private SerializedProperty m_AdditionalNameLabels;
	private SerializedProperty m_Text;
	private SerializedProperty m_Prefix;
	private SerializedProperty m_ContainerName;
	private SerializedProperty m_IsLocalizationKey;
	private SerializedProperty m_PrefixIsLocalizationKey;
	private SerializedProperty m_PrefixIsPostFix;

	// Target
	static bool showTarget = false;
	private SerializedProperty m_ReferenceElement;

	// Positioning
	static bool showTraversalOrder = false;
	//private SerializedProperty m_ForceStartHere;
	private SerializedProperty m_ManualPositionParent;
	private SerializedProperty m_ManualPositionOrder;
	private SerializedProperty m_UseTargetForOutline;

	// Speech Output
	static bool showSpeechOutput = false;
	private SerializedProperty m_AllowVoiceOver;
	private SerializedProperty m_ReadType;
	private SerializedProperty m_CustomHint;
	private SerializedProperty m_Hint;
	private SerializedProperty m_HintIsLocalizationKey;

	// Default Inspector
	static bool allowDefaultInspector = false;
	static protected bool drawDefaultInspector = false;

	// Slider Setup
	static bool showSliderSetup = false;
	private SerializedProperty m_Increments;
	private SerializedProperty m_IncrementInPercent;
	private SerializedProperty m_ReadPercentages;
	private SerializedProperty m_WholeNumbersOnly;

	// Container Settings
	static bool showContainerSettings = true;
	private SerializedProperty m_PopUp;
	private SerializedProperty m_AutoRead;
	private SerializedProperty m_Priority;

	// Container Navigation
	static bool showContainerNavigation = false;
	private SerializedProperty m_AllowTouchExplore;
	private SerializedProperty m_2DNavigation;
	private SerializedProperty m_ConstrainToContainerUp;
	private SerializedProperty m_ConstrainToContainerDown;
	private SerializedProperty m_ConstrainToContainerLeft;
	private SerializedProperty m_ConstrainToContainerRight;

	// Callbacks
	static bool showCallbacks = false;
	private SerializedProperty m_CallbackOnHighlight;
	private SerializedProperty m_OnInteractionStart;
	private SerializedProperty m_OnInteractionEnd;
	private SerializedProperty m_OnInteractionAbort;

	//////////////////////////////////////////////////////////////////////////

	public void DrawContainerNavigationSettings()
	{
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		m_AllowTouchExplore = serializedObject.FindProperty("m_AllowTouchExplore");
		m_2DNavigation = serializedObject.FindProperty("m_2DNavigation");
		m_ConstrainToContainerUp = serializedObject.FindProperty("m_ConstrainToContainerUp");
		m_ConstrainToContainerDown = serializedObject.FindProperty("m_ConstrainToContainerDown");
		m_ConstrainToContainerLeft = serializedObject.FindProperty("m_ConstrainToContainerLeft");
		m_ConstrainToContainerRight = serializedObject.FindProperty("m_ConstrainToContainerRight");

		showContainerNavigation = DrawSectionHeader("Navigation", showContainerNavigation);

		if (showContainerNavigation)
		{
			EditorGUILayout.PropertyField(m_AllowTouchExplore, new GUIContent("Allow Touch Explore", "You can optionally turn Touch Explore off if you want to restrict the user to navigate the elements in this container using swipes only. Default is true."));
			EditorGUILayout.PropertyField(m_2DNavigation, new GUIContent("2D Navigation", "If enabled, swiping up and down will select the UI element above or below. Useful for grid based puzzles."));
			if (m_2DNavigation.boolValue)
			{
				++EditorGUI.indentLevel;
				EditorGUILayout.PropertyField(m_ConstrainToContainerUp, new GUIContent("Constrain Up", "If checked, player cannot leave the container by swiping up once he reached the top most element."));
				EditorGUILayout.PropertyField(m_ConstrainToContainerDown, new GUIContent("Constrain Down", "If checked, player cannot leave the container by swiping down once he reached the bottom most element."));
				EditorGUILayout.PropertyField(m_ConstrainToContainerLeft, new GUIContent("Constrain Left", "If checked, player cannot leave the container by swiping left once he reached the left most element."));
				EditorGUILayout.PropertyField(m_ConstrainToContainerRight, new GUIContent("Constrain Right", "If checked, player cannot leave the container by swiping right once he reached the right most element."));
				--EditorGUI.indentLevel;
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////

	public void DrawContainerSettings()
	{
		m_PopUp = serializedObject.FindProperty("m_PopUp");
		m_AutoRead = serializedObject.FindProperty("m_AutoRead");
		m_Priority = serializedObject.FindProperty("m_Priority");

		showContainerSettings = DrawSectionHeader("Settings", showContainerSettings);

		if (showContainerSettings)
		{
			EditorGUILayout.PropertyField(m_PopUp, new GUIContent("Is Pop-Up", "Set this to true if this is a dialog window that overlays other UI. That way the accessibility plugin knows that the rest of the UI is not available right now."));
			EditorGUILayout.PropertyField(m_AutoRead, new GUIContent("Auto Read", "Automatically read the screen from top when this container is activated (and has focus). Useful for popups and cut-scenes."));
			EditorGUILayout.PropertyField(m_Priority, new GUIContent("Priority (optional)", "If more than one UI Group is on screen at the same time, they are traversed by order of priority (highest to lowest). If left unchanged, the order is determined by the order of creation, which usually matches their position in the hierarchy window."));
		}
	}

	//////////////////////////////////////////////////////////////////////////

	public void DrawSliderSetupSection()
	{
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		m_Increments = serializedObject.FindProperty("m_Increments");
		m_IncrementInPercent = serializedObject.FindProperty("m_IncrementInPercent");
		m_ReadPercentages = serializedObject.FindProperty("m_ReadPercentages");
		m_WholeNumbersOnly = serializedObject.FindProperty("m_WholeNumbersOnly");

		showSliderSetup = DrawSectionHeader("Slider Setup", showSliderSetup);

		if (showSliderSetup)
		{
			EditorGUILayout.PropertyField(m_Increments, new GUIContent("Increment Size", "By how much shall one swipe/one keystroke modify the slider value? Default is 5%"));
			EditorGUILayout.PropertyField(m_IncrementInPercent, new GUIContent("Increment in %", "The increment size is to be interpreted as % of the total slider range."));
			EditorGUILayout.Separator();
			EditorGUILayout.PropertyField(m_ReadPercentages, new GUIContent("Read Percentages", "If true, speech will not read the actual slider value, but the percentage of where the handle stands. E.g. a volume slider from 1 to 20 standing at 10 will be read as 50%"));
			EditorGUILayout.PropertyField(m_WholeNumbersOnly, new GUIContent("Read Whole Numbers Only", "Ignore decimal places and only read out whole numbers. E.g. '15.23' will be read as '15'"));
		}
	}

	//////////////////////////////////////////////////////////////////////////

	public bool DrawSpeechOutputSection()
	{
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		m_AllowVoiceOver = serializedObject.FindProperty("m_AllowVoiceOver");
		m_ReadType = serializedObject.FindProperty("m_ReadType");
		m_CustomHint = serializedObject.FindProperty("m_CustomHint");
		m_Hint = serializedObject.FindProperty("m_Hint");
		m_HintIsLocalizationKey = serializedObject.FindProperty("m_HintIsLocalizationKey");

		showSpeechOutput = DrawSectionHeader("Speech Output", showSpeechOutput);

		if (showSpeechOutput)
		{
			EditorGUILayout.PropertyField(m_AllowVoiceOver, new GUIContent("Allow VoiceOver (iOS)", "On iOS you can prevent this element from being read aloud with VoiceOver and read it with the regular speech synthesizer instead. See the documentation for examples of when this can be useful."));
			EditorGUILayout.PropertyField(m_ReadType, new GUIContent("Read Type", "In some special circumstances you might want to prevent the type from being read. See the documentation for examples."));
			EditorGUILayout.PropertyField(m_CustomHint, new GUIContent("Custom Hint Text", "Short description of what this control does. By default the plugin will read out a text fitting to the type of the element."));
			if (m_CustomHint.boolValue)
			{
				++EditorGUI.indentLevel;
				EditorGUILayout.PropertyField(m_Hint, new GUIContent("Hint Text", "Specify a custom hint text to be read if the user stays on this element for a few seconds."));
				EditorGUILayout.PropertyField(m_HintIsLocalizationKey, new GUIContent("Is Localization Key", "If checked, the plugin will treat the hint text as a localization key and request a translation from the localization system at runtime."));
				// Display localized text if needed
				if (m_HintIsLocalizationKey.boolValue)
				{
					EditorGUI.BeginDisabledGroup(true);
					EditorGUILayout.TextArea(UAP_AccessibilityManager.Localize(m_Hint.stringValue), myTextAreaInputStyle);
					EditorGUI.EndDisabledGroup();
				}
				--EditorGUI.indentLevel;
			}
		}
		return showSpeechOutput;
	}

	//////////////////////////////////////////////////////////////////////////

	public void DrawTargetSection(string typeName)
	{
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		m_ReferenceElement = serializedObject.FindProperty("m_ReferenceElement");
		m_UseTargetForOutline = serializedObject.FindProperty("m_UseTargetForOutline");

		showTarget = DrawSectionHeader("Target", showTarget);

		if (showTarget)
		{
			EditorGUILayout.PropertyField(m_ReferenceElement, new GUIContent("Target " + typeName + " (optional)", "If the actual UI " + typeName + " isn't on this GameObject, specify the GameObject it is on here."));
			// TODO: Error checking - is there a Button or UI button component on that GameObject?
			EditorGUI.BeginDisabledGroup((m_ReferenceElement.objectReferenceValue == null));
			EditorGUILayout.PropertyField(m_UseTargetForOutline, new GUIContent("Use For Outline", "Use that GameObject for Element Frame drawing and for Touch Explore.\nDefault is false."));
			EditorGUI.EndDisabledGroup();
		}
	}

	//////////////////////////////////////////////////////////////////////////

	public void DrawPositionOrderSection(bool showTargetSection = true)
	{
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		//m_ForceStartHere = serializedObject.FindProperty("m_ForceStartHere");
		m_ManualPositionOrder = serializedObject.FindProperty("m_ManualPositionOrder");
		m_ManualPositionParent = serializedObject.FindProperty("m_ManualPositionParent");

		showTraversalOrder = DrawSectionHeader("Traversal Order", showTraversalOrder);

		if (showTraversalOrder)
		{
			//EditorGUILayout.PropertyField(m_ForceStartHere, new GUIContent("Force Start Here", "Make this the initially selected element in this UI container."));
			//EditorGUILayout.Separator();

			EditorGUILayout.PropertyField(m_ManualPositionOrder, new GUIContent("Manual Order Index", "Instead of ordering UI elements by their position, you can specify a custom order. Leave at -1 for automatic placement."));
			if (showTargetSection)
				EditorGUILayout.PropertyField(m_ManualPositionParent, new GUIContent("Manual Order Parent", "This is a parent transform that the manual order position is relative to. Use the same transform on multiple elements to order them relative to one another."));

			// Disabled the error, because version v1.0.3 no longer requires setting a parent (it's still recommended for safety in the hierarchy)
			//if (m_ManualPositionOrder.intValue >= 0 && m_ManualPositionParent.objectReferenceValue == null)
			//  DrawErrorBox("You must specify a parent transform if you want to use manual ordering");

			EditorGUILayout.Separator();
		}
	}

	//////////////////////////////////////////////////////////////////////////

	public void DrawNameSection(bool showNameLabel = true)
	{
		m_NameLabel = serializedObject.FindProperty("m_NameLabel");
		m_AdditionalNameLabels = serializedObject.FindProperty("m_AdditionalNameLabels");
		m_Text = serializedObject.FindProperty("m_Text");
		m_Prefix = serializedObject.FindProperty("m_Prefix");
		m_IsLocalizationKey = serializedObject.FindProperty("m_IsLocalizationKey");
		m_PrefixIsLocalizationKey = serializedObject.FindProperty("m_PrefixIsLocalizationKey");
		m_PrefixIsPostFix = serializedObject.FindProperty("m_PrefixIsPostFix");

		showNaming = DrawSectionHeader("Name", showNaming);

		if (showNaming)
		{
			// Name editing doesn't work in multi-select
			if (targets.Length > 1)
			{
				EditorGUILayout.LabelField("-- Multiple Values --", myLabelStyle);
				EditorGUILayout.PropertyField(m_Prefix, new GUIContent("Prefix", "Will automatically be added in front of the name if a label is set.\nYou can use the wildcard {0} to insert the label into your text instead."));
			}
			else
			{
				UAP_BaseElement baseItem = (UAP_BaseElement)targets[0];
				if (!baseItem.m_IsInitialized)
					baseItem.Initialize();

				if (showNameLabel)
				{
					EditorGUILayout.PropertyField(m_NameLabel, new GUIContent("Name Label", "If assigned, the plugin will read out the content of the label when this UI element receives focus.\nUse {0} in the prefix to place this text."));
					// Only if at least 1 name label is set, allow the user to set more
					if (m_NameLabel.objectReferenceValue != null)
						EditorGUILayout.PropertyField(m_AdditionalNameLabels, new GUIContent("Combine Labels", "Additional label out of which the resulting text can be combined via {1}, {2}, etc..."), true);
				}
				else
					baseItem.m_NameLabel = null;

				if (baseItem.m_NameLabel != null)
				{
					// Read the element's name from a reference label
					EditorGUILayout.PropertyField(m_Prefix, new GUIContent("Combine String", "Will automatically be added in front of the name if a label is set.\nYou can use the wildcard {0} to insert the label into your text instead.\nUse {1}, {2}, ... to access additional labels."));
					EditorGUILayout.PropertyField(m_PrefixIsLocalizationKey, new GUIContent("Is Localization Key", "If set to true, the plugin will treat the name as a localization key and request the localization at runtime."));
					if (m_AdditionalNameLabels.arraySize == 0)
						EditorGUILayout.PropertyField(m_PrefixIsPostFix, new GUIContent("Is Postfix", "If set to true, the combination string will be add behind the label's content, not in front of it."));
					string nameText = baseItem.GetTextToRead();
					EditorGUI.BeginDisabledGroup(baseItem.m_NameLabel != null);
					EditorGUILayout.TextArea(nameText, myTextAreaInputStyle);
					EditorGUI.EndDisabledGroup();
					baseItem.m_TryToReadLabel = true;
				}
				else
				{
					// Manual setup of the name
					m_Text.stringValue = EditorGUILayout.TextArea(m_Text.stringValue, myTextAreaInputStyle);
					EditorGUILayout.PropertyField(m_IsLocalizationKey, new GUIContent("Is Localization Key", "If set to true, the plugin will treat the name as a localization key and request the localization at runtime."));
					// Display localized text if needed
					if (m_IsLocalizationKey.boolValue)
					{
						++EditorGUI.indentLevel;
						EditorGUI.BeginDisabledGroup(true);
						EditorGUILayout.TextArea(baseItem.GetTextToRead(), myTextAreaInputStyle);
						EditorGUI.EndDisabledGroup();
						--EditorGUI.indentLevel;
					}
					baseItem.m_TryToReadLabel = false;
				}
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////

	public void DrawContainerNameSection()
	{
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		m_ContainerName = serializedObject.FindProperty("m_ContainerName");
		m_IsLocalizationKey = serializedObject.FindProperty("m_IsLocalizationKey");

		showGroupNaming = DrawSectionHeader("Group Name", showGroupNaming);

		if (showGroupNaming)
		{
			// Name editing doesn't work in multi-select
			if (targets.Length > 1)
			{
				EditorGUILayout.LabelField("-- Multiple Values --", myLabelStyle);
			}
			else
			{
				AccessibleUIGroupRoot container = (AccessibleUIGroupRoot)targets[0];
				// Manual setup of the name
				EditorGUILayout.HelpBox("(Optional) The plugin can read out the name of this UI group if one of it's elements receives focus. The name is not repeated while the focus stays within the container.", MessageType.Info);
				m_ContainerName.stringValue = EditorGUILayout.TextArea(m_ContainerName.stringValue, myTextAreaInputStyle);
				EditorGUILayout.PropertyField(m_IsLocalizationKey, new GUIContent("Is Localization Key", "If set to true, the plugin will treat the name as a localization key and request the localization at runtime."));
				// Display localized text if needed
				if (m_IsLocalizationKey.boolValue)
				{
					++EditorGUI.indentLevel;
					EditorGUI.BeginDisabledGroup(true);
					EditorGUILayout.TextArea(container.GetContainerName(), myTextAreaInputStyle);
					EditorGUI.EndDisabledGroup();
					--EditorGUI.indentLevel;
				}
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////

	public void DrawCallbackSection(bool isInteractive)
	{
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		m_CallbackOnHighlight = serializedObject.FindProperty("m_CallbackOnHighlight");
		m_OnInteractionStart = serializedObject.FindProperty("m_OnInteractionStart");
		m_OnInteractionEnd = serializedObject.FindProperty("m_OnInteractionEnd");
		m_OnInteractionAbort = serializedObject.FindProperty("m_OnInteractionAbort");

		showCallbacks = DrawSectionHeader("Callbacks", showCallbacks);

		if (showCallbacks)
		{
			EditorGUILayout.PropertyField(m_CallbackOnHighlight, new GUIContent("Highlight", "UAP will call any function assigned here when the focus is moved onto or off of this element."));

			if (isInteractive)
			{
				EditorGUILayout.PropertyField(m_OnInteractionStart, new GUIContent("Interact Start", "UAP will call any function assigned here when the user interacts with this element (double tap or return key)."));
				EditorGUILayout.PropertyField(m_OnInteractionEnd, new GUIContent("Interact Finish", "UAP will call any function assigned here when the user finishes the interaction."));
				EditorGUILayout.PropertyField(m_OnInteractionAbort, new GUIContent("Interact Cancel", "UAP will call any function assigned here when the user cancels the interaction."));
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////

	protected bool DrawSectionHeader(string headerText, bool initialValue = false)
	{
		GUI.contentColor = EditorGUIUtility.isProSkin ? new Color(0.8f, 0.8f, 0.8f, 0.8f) : new Color(1f, 1f, 1f, 0.8f);
		//		initialValue = EditorGUILayout.ToggleLeft(headerText, initialValue, myHeader2Style);
		initialValue = GUILayout.Toggle(initialValue, headerText, myHeader2Style, GUILayout.MinWidth(20f));
		GUI.contentColor = Color.white;
		EditorGUILayout.Separator();
		return initialValue;
	}

	//////////////////////////////////////////////////////////////////////////

	protected void DrawDefaultInspectorSection()
	{
		if (!allowDefaultInspector)
			return;

		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		drawDefaultInspector = EditorGUILayout.Foldout(drawDefaultInspector, "Default Inspector");
		if (drawDefaultInspector)
			DrawDefaultInspector();
	}

	//////////////////////////////////////////////////////////////////////////

	protected void SetupGUIStyles()
	{
		if (stylesInitialized)
			return;

		stylesInitialized = true;

		Color textColor = EditorGUIUtility.isProSkin ? Color.yellow : Color.blue;

		myLabelStyle = new GUIStyle(EditorStyles.label);
		myLabelStyle.fontSize = 12;
		myLabelStyle.fixedHeight = 30;

		labelRightAligned = new GUIStyle(EditorStyles.label);
		labelRightAligned.alignment = TextAnchor.MiddleRight;

		myTextInputStyle = new GUIStyle(EditorStyles.textField);
		myTextInputStyle.fontSize = 12;
		myTextInputStyle.normal.textColor = textColor;
		myTextInputStyle.alignment = TextAnchor.LowerLeft;
		myTextInputStyle.fixedHeight = 20;

		myTextAreaInputStyle = new GUIStyle(EditorStyles.textArea);
		myTextAreaInputStyle.fontSize = 12;
		myTextAreaInputStyle.normal.textColor = textColor;

		myEnumStyle = new GUIStyle(EditorStyles.toolbarDropDown);
		myEnumStyle.fontSize = 12;
		myEnumStyle.fixedHeight = 20;

		myHeadingStyle = new GUIStyle(myLabelStyle);
		myHeadingStyle.fontSize = 14;
		myHeadingStyle.fontStyle = FontStyle.Bold;

		myHeader2Style = new GUIStyle(GUI.skin.GetStyle("PreToolbar"));
		myHeader2Style.fontSize = 14;
		myHeader2Style.fontStyle = FontStyle.Bold;
		myHeader2Style.fixedHeight = 20;

		myTopStyle = new GUIStyle(myHeadingStyle);
		myTopStyle.alignment = TextAnchor.MiddleCenter;
		myTopStyle.normal.textColor = Color.blue;
		myTopStyle.normal.background = EditorStyles.numberField.normal.background;// myTextAreaInputStyle.normal.background;
																																							//myHeader2Style.fixedHeight = 35;
	}

	//////////////////////////////////////////////////////////////////////////

	protected void DrawErrorBox(string errorMessage)
	{
		//GUI.color = Color.red;
		EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
		//GUI.color = Color.white;
	}

	//////////////////////////////////////////////////////////////////////////

	protected void DrawWarningBox(string errorMessage)
	{
		//GUI.color = Color.red;
		EditorGUILayout.HelpBox(errorMessage, MessageType.Warning);
		//GUI.color = Color.white;
	}

	//////////////////////////////////////////////////////////////////////////

}
