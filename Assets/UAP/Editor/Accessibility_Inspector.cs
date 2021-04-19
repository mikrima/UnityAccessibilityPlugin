using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(UAP_AccessibilityManager))]
public class Accessibility_Inspector : Accessibility_InspectorShared
{
	static bool showSettings = false;
	static Texture m_LogoTexture = null;

	// Enabling
	private SerializedProperty m_DefaultState;
	private SerializedProperty m_AutoTurnOnIfScreenReaderDetected;
	private SerializedProperty m_SaveEnabledState;

	// Settings
	private SerializedProperty m_HandleUI;
	private SerializedProperty m_HandleMagicGestures;
	private SerializedProperty m_ExploreByTouch;
	private SerializedProperty m_MouseSwipes;
	private SerializedProperty m_ReadDisabledInteractables;
	private SerializedProperty m_CyclicMenus;
	private SerializedProperty m_DetectVoiceOverAtRuntime;
	private SerializedProperty m_AllowVoiceOverGlobal;
	private SerializedProperty m_AllowBuiltInVirtualKeyboard;

	// Debug Testing
	static bool showDebugging = false;
	public SerializedProperty m_EditorOverride;
	public SerializedProperty m_EditorEnabledState;
	public SerializedProperty m_DebugOutput;

	// Sounds
	static bool showSounds = false;
	private SerializedProperty m_GoogleTTSAPIKey;
	private SerializedProperty m_UINavigationClick;
	private SerializedProperty m_UIInteract;
	private SerializedProperty m_UIFocusEnter;
	private SerializedProperty m_UIFocusLeave;
	private SerializedProperty m_UIBoundsReached;
	private SerializedProperty m_UIPopUpOpen;
	private SerializedProperty m_UIPopUpClose;

	// NGUI
	static bool showNGUI = false;

	//////////////////////////////////////////////////////////////////////////

	void OnEnable()
	{
		m_DefaultState = serializedObject.FindProperty("m_DefaultState");
		m_AutoTurnOnIfScreenReaderDetected = serializedObject.FindProperty("m_AutoTurnOnIfScreenReaderDetected");
		m_SaveEnabledState = serializedObject.FindProperty("m_SaveEnabledState");
		m_DetectVoiceOverAtRuntime = serializedObject.FindProperty("m_DetectVoiceOverAtRuntime");

		m_EditorOverride = serializedObject.FindProperty("m_EditorOverride");
		m_EditorEnabledState = serializedObject.FindProperty("m_EditorEnabledState");
		m_DebugOutput = serializedObject.FindProperty("m_DebugOutput");

		m_HandleUI = serializedObject.FindProperty("m_HandleUI");
		m_HandleMagicGestures = serializedObject.FindProperty("m_HandleMagicGestures");
		m_ExploreByTouch = serializedObject.FindProperty("m_ExploreByTouch");
		m_MouseSwipes = serializedObject.FindProperty("m_WindowsUseMouseSwipes");
		m_ReadDisabledInteractables = serializedObject.FindProperty("m_ReadDisabledInteractables");
		m_CyclicMenus = serializedObject.FindProperty("m_CyclicMenus");
		m_AllowVoiceOverGlobal = serializedObject.FindProperty("m_AllowVoiceOverGlobal");
		m_AllowBuiltInVirtualKeyboard = serializedObject.FindProperty("m_AllowBuiltInVirtualKeyboard");

		m_GoogleTTSAPIKey = serializedObject.FindProperty("m_GoogleTTSAPIKey");
		m_UINavigationClick = serializedObject.FindProperty("m_UINavigationClick");
		m_UIInteract = serializedObject.FindProperty("m_UIInteract");
		m_UIFocusEnter = serializedObject.FindProperty("m_UIFocusEnter");
		m_UIFocusLeave = serializedObject.FindProperty("m_UIFocusLeave");
		m_UIBoundsReached = serializedObject.FindProperty("m_UIBoundsReached");
		m_UIPopUpOpen = serializedObject.FindProperty("m_UIPopUpOpen");
		m_UIPopUpClose = serializedObject.FindProperty("m_UIPopUpClose");

		LoadLogoTexture();
	}

	//////////////////////////////////////////////////////////////////////////

	private void LoadLogoTexture()
	{
		string path = Accessibility_EditorFunctions.PluginFolder + "/Editor/img/Logo_Inspector_Bright.png";
		if (!EditorGUIUtility.isProSkin)
			path = Accessibility_EditorFunctions.PluginFolder + "/Editor/img/Logo_Inspector_Dark.png";
		m_LogoTexture = AssetDatabase.LoadAssetAtPath<Texture>(path);
	}

	//////////////////////////////////////////////////////////////////////////

	public override void OnInspectorGUI()
	{

		SetupGUIStyles();
		serializedObject.Update();

		EditorGUILayout.Separator();
		EditorGUILayout.Separator();

		if (m_LogoTexture == null)
			LoadLogoTexture();

		if (m_LogoTexture == null)
		{
			EditorGUILayout.LabelField("UAP - UI Accessibility Plugin", myTopStyle);
			EditorGUILayout.Separator();
			EditorGUILayout.Separator();
		}
		else
		{
			//GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			var headerRect = GUILayoutUtility.GetRect(0.0f, 5.0f);
			headerRect.width = m_LogoTexture.width;
			headerRect.height = m_LogoTexture.height;
			GUI.DrawTexture(headerRect, m_LogoTexture);

			GUILayout.FlexibleSpace();
			//GUILayout.FlexibleSpace();
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.Space(60);
			//GUILayout.FlexibleSpace();
		}

		if (GUILayout.Button("Open Documentation", GUILayout.Height(25)))
		{
			Application.OpenURL("http://www.metalpopgames.com/assetstore/accessibility/doc");
		}


		// ENABLING
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		DrawSectionHeader("Turn On/Off Accessibility", true);
		//EditorGUILayout.PropertyField(m_DefaultState, new GUIContent("Enabled on App Install", "Default: Off\nDetermines whether accessibility should be on or off when the user installs the app for the first time. If Auto Enable is on, this can usually be left off."), GUILayout.ExpandWidth(true));
		//EditorGUILayout.PropertyField(m_AutoTurnOnIfScreenReaderDetected, new GUIContent("Auto-Enable if screen reader detected", "Default: On\nThe plugin can detect whether screen reader software is running on the target platform (TalkBack, VoiceOver, NVDA, etc) and turn on automatically if that is the case."));
		m_DefaultState.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Enabled after app install", "Determines whether accessibility should be on or off when the user installs the app for the first time. If Auto Enable is on, this can usually be left off."), m_DefaultState.boolValue);
		if (m_DefaultState.boolValue)
			DrawWarningBox("Are you sure?\nIf this is on then all users, including sighted ones, will start the app with accessibility mode enabled.");
		m_AutoTurnOnIfScreenReaderDetected.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Auto-Enable if screen reader detected", "The plugin can detect whether screen reader software is running on the target platform (TalkBack, VoiceOver, NVDA, etc) and turn on automatically if that is the case."), m_AutoTurnOnIfScreenReaderDetected.boolValue);
		if (!m_AutoTurnOnIfScreenReaderDetected.boolValue)
			DrawWarningBox("Are you sure?\nIf this is off then the only option for blind users to activate accessibility is with a Magic Gesture or asking a sighted friend for help.");
		EditorGUI.BeginDisabledGroup(!m_AutoTurnOnIfScreenReaderDetected.boolValue);
		m_DetectVoiceOverAtRuntime.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Enable/Disable with VoiceOver (iOS only)", "This will turn the plugin on/off depending on whether VoiceOver is on or off. This will feel the most natural to most players.\nDefault is true."), m_DetectVoiceOverAtRuntime.boolValue);
		EditorGUI.EndDisabledGroup();
		m_SaveEnabledState.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Save Enabled State", "Turn accessibility mode on or off, depending on what it was when the app was last closed."), m_SaveEnabledState.boolValue);

		if (Application.isPlaying)
		{
			EditorGUILayout.LabelField("Accessibility is currently " + (UAP_AccessibilityManager.IsEnabled() ? "ON" : "OFF"));
			//EditorGUILayout.LabelField("Accessibility GUID " + Accessibility_Manager.GetInstanceID());
		}


		// TESTING
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		showDebugging = DrawSectionHeader("Testing and Debugging", showDebugging);
		if (showDebugging || Application.isPlaying)
		{
			m_DebugOutput.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Debug Log Output", "If turned on, the UAP will log messages to the console and log file. This can help track down bugs and issues."), m_DebugOutput.boolValue);
			EditorGUILayout.Separator();

			EditorGUILayout.LabelField(new GUIContent("Editor Only - Force Accessibility Mode", "You can manually force to start the application with accessibility enabled or disabled. This overrides any other setting. These settings are only applicable while inside the Editor and will be ignored when the application is built."));
			//EditorGUILayout.LabelField("Force Accessibility to be:");
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			//EditorGUILayout.Space();
			bool forceOn = EditorGUILayout.Toggle(m_EditorOverride.boolValue && m_EditorEnabledState.boolValue, EditorStyles.radioButton, GUILayout.MaxWidth(15));
			EditorGUILayout.LabelField("On", GUILayout.MaxWidth(60));
			bool forceOff = EditorGUILayout.Toggle(m_EditorOverride.boolValue && !m_EditorEnabledState.boolValue, EditorStyles.radioButton, GUILayout.MaxWidth(15));
			EditorGUILayout.LabelField("Off", GUILayout.MaxWidth(60));
			bool useDefault = EditorGUILayout.Toggle(!m_EditorOverride.boolValue, EditorStyles.radioButton, GUILayout.MaxWidth(15));
			EditorGUILayout.LabelField(new GUIContent("App Controlled", "Controlled by the app. Use whatever has been saved in the user preferences."), GUILayout.MaxWidth(90));
			//GUILayout.FlexibleSpace();
			GUILayout.FlexibleSpace();
			//EditorGUILayout.Space();
			if (useDefault && m_EditorOverride.boolValue)
			{
				m_EditorOverride.boolValue = false;
				m_EditorEnabledState.boolValue = false;
			}
			else if (forceOn && (!m_EditorEnabledState.boolValue || !m_EditorOverride.boolValue))
			{
				m_EditorOverride.boolValue = true;
				m_EditorEnabledState.boolValue = true;
			}
			else if (forceOff && (m_EditorEnabledState.boolValue || !m_EditorOverride.boolValue))
			{
				m_EditorOverride.boolValue = true;
				m_EditorEnabledState.boolValue = false;
			}
			EditorGUILayout.EndHorizontal();

			// If the app is playing/running, offer a button to toggle accessibility manually
			if (Application.isPlaying)
			{
				EditorGUILayout.Separator();
				if (GUILayout.Button("Toggle Accessibility", GUILayout.Height(25)))
				{
					UAP_AccessibilityManager.ToggleAccessibility();
				}
			}

		}


		// SETTINGS
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		showSettings = DrawSectionHeader("Settings", showSettings);
		if (showSettings)
		{
			EditorGUILayout.HelpBox("Please consult the documentation before changing these values.\nThe default settings conform to the way VoiceOver and other screen readers work. Only change these if you are certain your application needs it.", MessageType.Info);

			m_HandleUI.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Handle UI", "If true (Default), the plugin will block direct touch to the screen."), m_HandleUI.boolValue);
			if (!m_HandleUI.boolValue)
				DrawErrorBox("Attention:\nIf you just want the plugin to stop handling swipes, use PauseAccessibility() instead.");
			m_ExploreByTouch.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Explore By Touch", "If true (Default) the plugin reads out the UI element that is under the user's finger. If 'Handle UI' is not active, this setting will be ignored."), m_ExploreByTouch.boolValue);
			m_MouseSwipes.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Mouse Swiping", "Default is false.. If true, the plugin reads mouse swipes as finger swipes. Using the ALT key emulates two finger swipes.\nThis is very useful if developing a mobile application, but probably undesirable in a Desktop app."), m_MouseSwipes.boolValue);
			EditorGUILayout.Separator();
			m_HandleMagicGestures.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Magic Gestures", "If true (Default), the plugin will recognize magic gestures for functions like Back, Exit and Pause, and call the appropriate callbacks to your game."), m_HandleMagicGestures.boolValue);
			if (!m_HandleMagicGestures.boolValue)
				DrawErrorBox("Are you sure?\nYou don't need to subscribe to any events.\nBut disabling this also takes away the player's ability to enable/disable accessibility with a finger gesture.");
			m_ReadDisabledInteractables.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Read Disabled", "If true (Default), the plugin reads out disabled but visible interactive UI elements. The plugin will read out 'Disabled' after the name."), m_ReadDisabledInteractables.boolValue);
			if (!m_ReadDisabledInteractables.boolValue)
				DrawErrorBox("Careful!\nIt's important that blind users know about buttons, sliders etc even if they are disabled. If they are not read while exploring the screen, they are invisible to them. They might not realize a button has become active later if they never knew it was there in the first place.");
			m_CyclicMenus.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Cyclic Menus", "Default is false. If true, when reaching the last element on the screen, the focus will jump back around to the first element."), m_CyclicMenus.boolValue);
			if (m_CyclicMenus.boolValue)
				DrawErrorBox("Attention:\nThis can be very irritating to blind players. Even though it makes sense to sighted people, this was regularly reported as a negative feature during beta tests.");
			m_AllowVoiceOverGlobal.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Use VoiceOver Speech (iOS only)", "Default is true. On iOS, Text-To-Speech will use VoiceOver. If this is off, or if VoiceOver is not running, the plugin will use the system's voice.\nYou can disable the use of VoiceOver altogether and enforce using the system's voice here."), m_AllowVoiceOverGlobal.boolValue);
			if (!m_AllowVoiceOverGlobal.boolValue)
				DrawWarningBox("Attention:\nBlind users have their voice and speech rate selected carefully to their personal taste. Only use the system voice and a default speech rate if you have a good reason to ignore their settings.\nHint:\nYou can disable VoiceOver for individual UI elements instead.");
			m_AllowBuiltInVirtualKeyboard.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Allow built-in virtual keyboard", "This will use the built-in virtual keyboard for edit boxes instead of the native OS on-screen keyboard. This is only done if the system's language is supported, or the platform is iOS."), m_AllowBuiltInVirtualKeyboard.boolValue);
			if (!m_AllowBuiltInVirtualKeyboard.boolValue)
				DrawWarningBox("Attention:\nOn Android players will need to re-enable TalkBack when they want to use an edit box. Afterwards TalkBack needs to be paused again.\nOn iOS, newer Unity versions often grab the focus away from the native on-screen keybaord, making input impossible. Make sure that this is really what you want.");

			if (GUILayout.Button("Reset Settings to Default"))
			{
				m_HandleUI.boolValue = true;
				m_ExploreByTouch.boolValue = true;
				m_MouseSwipes.boolValue = false;
				m_HandleMagicGestures.boolValue = true;
				m_ReadDisabledInteractables.boolValue = true;
				m_CyclicMenus.boolValue = false;
				m_AllowVoiceOverGlobal.boolValue = true;
				m_AllowBuiltInVirtualKeyboard.boolValue = true;
			}
		}


		// SOUND EFFECTS
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		showSounds = DrawSectionHeader("Sounds", showSounds);
		if (showSounds)
		{
			EditorGUILayout.PropertyField(m_GoogleTTSAPIKey, new GUIContent("Google TTS API key", "You can provide your Google Cloud API key to activate " +
				"Google TTS for WebGL. The UAP documentation contains step-by-step instructions on how to get an API key.\nThis is optional - if no key is provided, " +
				"UAP will use the browser Web Speech API to generate speech (most browsers support this)"));

			// If there is nothing, or only a very short key in the text field (invalid) show some help on how to get the key
			// The key should be 39 characters long - but it's not a hard requirement
			if (m_GoogleTTSAPIKey.stringValue == null || m_GoogleTTSAPIKey.stringValue.Length < 39) 
			{
				if (GUILayout.Button("Get API Key", GUILayout.MaxWidth(120)))
				{
					Application.OpenURL("http://www.metalpopgames.com/assetstore/accessibility/doc/WebGL.html");
				}
			}
			EditorGUILayout.PropertyField(m_UINavigationClick, new GUIContent("Navigation", "SFX when the UI element is changed"));
			EditorGUILayout.PropertyField(m_UIInteract, new GUIContent("Interact", "SFX when a button or toggle is pressed"));
			EditorGUILayout.PropertyField(m_UIFocusEnter, new GUIContent("Focus Enter", "SFX when an element receives exclusive focus, such as a slider"));
			EditorGUILayout.PropertyField(m_UIFocusLeave, new GUIContent("Focus Leave", "SFX when an element loses exclusive focus, such as a slider"));
			EditorGUILayout.PropertyField(m_UIBoundsReached, new GUIContent("Bounds Reached", "SFX when navigation reaches the end of the screen"));
			EditorGUILayout.PropertyField(m_UIPopUpOpen, new GUIContent("Popup Open", "SFX when popup opens"));
			EditorGUILayout.PropertyField(m_UIPopUpClose, new GUIContent("Popup Close", "SFX when popup closes"));
		}

		// SOUND EFFECTS
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		showNGUI = DrawSectionHeader("NGUI", showNGUI);
		if (showNGUI)
		{
			EditorGUILayout.HelpBox("UAP supports NGUI.\nNGUI has to be added to your project before enabling NGUI support.", MessageType.None);
			bool nguiDetected = UAP_WelcomeWindow.IsNGUIDetected();
			if (nguiDetected)
			{
				myLabelStyle.normal.textColor = Color.green;
				EditorGUILayout.LabelField("NGUI detected.", myLabelStyle);
			}
			else
			{
				myLabelStyle.normal.textColor = Color.red;
				EditorGUILayout.LabelField("NGUI not detected.", myLabelStyle);
			}
			myLabelStyle.normal.textColor = myHeadingStyle.normal.textColor;
			EditorGUILayout.Separator();

			bool nguiSupportEnabled = false;
#if !ACCESS_NGUI
			nguiSupportEnabled = false;
#else
			nguiSupportEnabled = true;
#endif
			EditorGUI.BeginDisabledGroup(!nguiDetected);
			bool desiredState = EditorGUILayout.ToggleLeft(new GUIContent("Enable NGUI Support", "Sets a precompiler flag that will include NGUI specific code in the plugin's compilation."), nguiSupportEnabled);
			EditorGUI.EndDisabledGroup();
			if (desiredState != nguiSupportEnabled)
			{
				if (nguiSupportEnabled)
				{
#if ACCESS_NGUI
					Accessibility_EditorFunctions.DisableNGUISupport();
#endif
				}
				else
				{
#if !ACCESS_NGUI
					// Make sure the user understands what he is doing
					Accessibility_EditorFunctions.EnableNGUISupport();
#endif
				}
			}
			EditorGUILayout.Separator();
		}

		/*
		
#if !ACCESS_NGUI
		// NGUI Check
		if (!Application.isPlaying)
		{
			if (UAP_WelcomeWindow.IsNGUIDetected())
			{
				// NGUI was detected in this project
				myHeadingStyle.normal.textColor = Color.red;
				EditorGUILayout.LabelField("NGUI detected but support not enabled.", myHeadingStyle);
				myHeadingStyle.normal.textColor = myLabelStyle.normal.textColor;
				EditorGUILayout.LabelField("Enable NGUI support?");
				if (GUILayout.Button("Enable NGUI support"))
				{
					UAP_WelcomeWindow.EnableNGUISupport();
				}
				EditorGUILayout.Separator();
				EditorGUILayout.Separator();
			}
		}
#endif		 
		 
		 */


		serializedObject.ApplyModifiedProperties();
		DrawDefaultInspectorSection();
	}

	//////////////////////////////////////////////////////////////////////////
}
