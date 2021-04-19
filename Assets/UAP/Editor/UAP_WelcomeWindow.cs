using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using System.Reflection;

public class UAP_WelcomeWindow : EditorWindow
{
	static Texture m_LogoTexture = null;

	protected GUIStyle myLabelStyle;
	protected GUIStyle labelRightAligned;
	protected GUIStyle myTextInputStyle;
	protected GUIStyle myHeadingStyle;
	protected GUIStyle myHeader2Style;
	protected GUIStyle myTopStyle;
	protected GUIStyle myTextAreaInputStyle;
	protected GUIStyle myEnumStyle;

	bool stylesInitialized = false;
	Vector2 scrollPosition = new Vector2(0, 0);

	//////////////////////////////////////////////////////////////////////////

	private void LoadLogoTexture()
	{
		string path = Accessibility_EditorFunctions.PluginFolder + "/Editor/img/Logo_Inspector_Bright.png";
		if (!EditorGUIUtility.isProSkin)
			path = Accessibility_EditorFunctions.PluginFolder + "/Editor/img/Logo_Inspector_Dark.png";
		m_LogoTexture = AssetDatabase.LoadAssetAtPath<Texture>(path);
	}

	//////////////////////////////////////////////////////////////////////////

	static public void Init()
	{
		// Get existing open window or if none, make a new one:
		UAP_WelcomeWindow window = (UAP_WelcomeWindow)EditorWindow.GetWindow(typeof(UAP_WelcomeWindow));
		window.titleContent.text = "About UAP";
		window.position = new Rect(50 + 0, 50 + 0, 400, 500);
		window.maxSize = new Vector2(405f, 505f);
		//window.minSize = window.maxSize;
		window.Show();
	}

	//////////////////////////////////////////////////////////////////////////

	void OnGUI()
	{
		SetupGUIStyles();

		// LOGO
		if (m_LogoTexture == null)
			LoadLogoTexture();

		if (m_LogoTexture == null)
		{
			EditorGUILayout.LabelField("UI Accessibility Plugin", myTopStyle);
			EditorGUILayout.Separator();
			EditorGUILayout.Separator();
		}
		else
		{
			EditorGUILayout.Separator();
			//GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			var headerRect = GUILayoutUtility.GetRect(0.0f, 5.0f);
			headerRect.width = m_LogoTexture.width;
			headerRect.height = m_LogoTexture.height;
			GUI.DrawTexture(headerRect, m_LogoTexture);

			GUILayout.FlexibleSpace();
			GUILayout.FlexibleSpace();
			//GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.Space(60);
			//GUILayout.FlexibleSpace();
		}


		// WELCOME TEXT
		EditorGUILayout.LabelField("Welcome to the UAP!", myHeadingStyle);
		EditorGUILayout.LabelField("Thank you for purchasing the UI Accessibility Plugin!", myLabelStyle);
		//EditorGUILayout.LabelField("Please check out the following links to help you get started.", myLabelStyle);
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();


		// DOCUMENTATION
		EditorGUILayout.LabelField("Documentation", myHeadingStyle);
		if (GUILayout.Button("Quick Start Guide"))
			Application.OpenURL("http://www.metalpopgames.com/assetstore/accessibility/doc/QuickStart.html");
		if (GUILayout.Button("Documentation"))
			Application.OpenURL("http://www.metalpopgames.com/assetstore/accessibility/doc");
		//if (GUILayout.Button("Support"))
		//  Application.OpenURL("http://www.metalpopgames.com/assetstore/accessibility/doc/SupportAndRoadmap.html");
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();

		// NGUI
		if (!Application.isPlaying)
		{
/*
			if (IsNGUIDetected())
			{
				// NGUI was detected in this project
#if !ACCESS_NGUI
				myHeadingStyle.normal.textColor = Color.red;
				EditorGUILayout.LabelField("NGUI detected but support not enabled.", myHeadingStyle);
				myHeadingStyle.normal.textColor = myLabelStyle.normal.textColor;
				EditorGUILayout.LabelField("Enable NGUI support?");
				//EditorGUILayout.LabelField("The plugin will not recognize NGUI UI elements otherwise.");
				if (GUILayout.Button("Enable NGUI support"))
				{
					EnableNGUISupport();
				}
#endif
			}

#if ACCESS_NGUI
			EditorGUILayout.Separator();
			EditorGUILayout.Separator();
			EditorGUILayout.LabelField("NGUI Support activated.");
#endif
*/
		}


		// CHANGELIST
		EditorGUILayout.LabelField("What's New", myHeadingStyle);
		TextAsset changelist = AssetDatabase.LoadAssetAtPath<TextAsset>(Accessibility_EditorFunctions.PluginFolder + "/Editor/UAP_Version.txt");
		if (changelist != null)
		{
			GUIStyle backup = GUI.skin.box;

			scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(395), GUILayout.Height(250));
			GUI.skin.box.wordWrap = true;   
			GUI.skin.box.alignment = TextAnchor.UpperLeft;
			GUI.skin.box.normal.textColor = myLabelStyle.normal.textColor;
			GUILayout.Box(changelist.text); 
			GUILayout.EndScrollView();

			GUI.skin.box = backup;
		}
	}

	//////////////////////////////////////////////////////////////////////////

	static public void EnableNGUISupport()
	{
		string preprocessorDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
		if (!preprocessorDefines.Contains("ACCESS_NGUI"))
		{
			preprocessorDefines = "ACCESS_NGUI; " + preprocessorDefines;
			PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, preprocessorDefines);
		}
		preprocessorDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS);
		if (!preprocessorDefines.Contains("ACCESS_NGUI"))
		{
			preprocessorDefines = "ACCESS_NGUI; " + preprocessorDefines;
			PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, preprocessorDefines);
		}
		preprocessorDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
		if (!preprocessorDefines.Contains("ACCESS_NGUI"))
		{
			preprocessorDefines = "ACCESS_NGUI; " + preprocessorDefines;
			PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, preprocessorDefines);
		}
	}

	static public void DisableNGUISupport()
	{
		string preprocessorDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
		if (preprocessorDefines.Contains("ACCESS_NGUI"))
		{
			int length = "ACCESS_NGUI;".Length;
			int index = preprocessorDefines.IndexOf("ACCESS_NGUI;");
			if (index < 0)
			{
				--length;
				index = preprocessorDefines.IndexOf("ACCESS_NGUI");
			}
			preprocessorDefines = preprocessorDefines.Remove(index, length);
			PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, preprocessorDefines);
		}
		preprocessorDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS);
		if (preprocessorDefines.Contains("ACCESS_NGUI"))
		{
			int length = "ACCESS_NGUI;".Length;
			int index = preprocessorDefines.IndexOf("ACCESS_NGUI;");
			if (index < 0)
			{
				--length;
				index = preprocessorDefines.IndexOf("ACCESS_NGUI");
			}
			preprocessorDefines = preprocessorDefines.Remove(index, length);
			PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, preprocessorDefines);
		}
		preprocessorDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
		if (preprocessorDefines.Contains("ACCESS_NGUI"))
		{
			int length = "ACCESS_NGUI;".Length;
			int index = preprocessorDefines.IndexOf("ACCESS_NGUI;");
			if (index < 0)
			{
				--length;
				index = preprocessorDefines.IndexOf("ACCESS_NGUI");
			}
			preprocessorDefines = preprocessorDefines.Remove(index, length);
			PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, preprocessorDefines);
		}
	}

	//////////////////////////////////////////////////////////////////////////

	static public bool IsNGUIDetected()
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly ass in assemblies)
		{
			if (ass.GetType("UIRoot") != null)
			{
				return true;
			}
		}
		return false;
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

}