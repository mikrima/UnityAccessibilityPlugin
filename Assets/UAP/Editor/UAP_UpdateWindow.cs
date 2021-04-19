using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using System.Reflection;

public class UAP_UpdateWindow : EditorWindow
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

	static bool m_WaitingForVersionCheck = false;
	static bool m_NoInternet = false;
	static bool m_NewVersionAvailable = false;
	static string m_LatestVersion = "";
	static string m_Changelog = "";

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
		UAP_UpdateWindow window = (UAP_UpdateWindow)EditorWindow.GetWindow(typeof(UAP_UpdateWindow));
		window.titleContent.text = "UAP Update";
		window.position = new Rect(50 + 0, 50 + 0, 400, 285);
		window.Show();
	}

	//////////////////////////////////////////////////////////////////////////

	static public void StartVersionCheck()
	{
		m_WaitingForVersionCheck = true;
		m_NoInternet = false;
		m_NewVersionAvailable = false;
		m_Changelog = "";
	}

	//////////////////////////////////////////////////////////////////////////

	static public void NoInternet()
	{
		//Could not check for updates, because the online version check URL could not be reached.
		m_NoInternet = true;
		m_WaitingForVersionCheck = false;
		m_NewVersionAvailable = false;
		EditorWindow.GetWindow(typeof(UAP_UpdateWindow)).Repaint();
	}

	//////////////////////////////////////////////////////////////////////////

	static public void VersionCheckComplete(bool foundNewer, string latestVersionCode, string changelog)
	{
		m_NoInternet = false;
		m_WaitingForVersionCheck = false;
		m_NewVersionAvailable = foundNewer;
		m_LatestVersion = latestVersionCode;
		m_Changelog = changelog;
		EditorWindow.GetWindow(typeof(UAP_UpdateWindow)).Repaint();
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
			EditorGUILayout.LabelField("Unity Accessibility Plugin", myTopStyle);
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


		if (m_WaitingForVersionCheck)
		{
			EditorGUILayout.Separator();
			EditorGUILayout.Separator();

			EditorGUILayout.LabelField("Checking for new version online...");
			EditorGUILayout.LabelField("                   ...please wait.");

			EditorGUILayout.Separator();
			EditorGUILayout.Separator();
		}
		else
		{
			if (m_NoInternet)
			{
				EditorGUILayout.Separator();
				EditorGUILayout.Separator();

				EditorGUILayout.LabelField("Could not check for updates,");
				EditorGUILayout.LabelField("because the online version check URL could not be reached.");

				EditorGUILayout.Separator();
				EditorGUILayout.Separator();
			}
			else
			{
				if (m_NewVersionAvailable)
				{
					EditorGUILayout.Separator();
					EditorGUILayout.Separator();

					EditorGUILayout.LabelField("Current Version: " + UAP_AccessibilityManager.PluginVersion);
					EditorGUILayout.LabelField("Latest Version: " + m_LatestVersion);

					EditorGUILayout.Separator();
					EditorGUILayout.Separator();

					EditorGUILayout.LabelField("There is a newer version of UAP available", myHeadingStyle);

					// Offer download link to the store
					EditorGUILayout.BeginHorizontal();
					if (GUILayout.Button("Update", GUILayout.Width(175)))
					{
						// TODO: Exchange asset ID for the actual ID
						Application.OpenURL("https://www.assetstore.unity3d.com/#!/content/87935");

						// Close Window
						EditorWindow.GetWindow(typeof(UAP_UpdateWindow)).Close();
					}

					EditorGUILayout.Space();

					// TODO: Give the option to be reminded again in a day

					if (GUILayout.Button("Skip This Version", GUILayout.Width(175)))
					{
						// Remember not to notify about this version during automatic checks
						PlayerPrefs.SetString("UAP_SkipVersion", m_LatestVersion);

						// Close Window
						EditorWindow.GetWindow(typeof(UAP_UpdateWindow)).Close();
					}
					EditorGUILayout.EndHorizontal();

					// CHANGELIST
					EditorGUILayout.Separator();
					EditorGUILayout.Separator();
					EditorGUILayout.LabelField("What's New", myHeadingStyle);
					if (m_Changelog.Length > 0)
					{
						GUIStyle backup = GUI.skin.box;

						scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(390), GUILayout.Height(100));
						GUI.skin.box.wordWrap = true;     
						GUI.skin.box.alignment = TextAnchor.UpperLeft;
						GUI.skin.box.normal.textColor = myLabelStyle.normal.textColor;
						GUILayout.Box(m_Changelog);       
						GUILayout.EndScrollView();

						GUI.skin.box = backup;
					}
				}
				else
				{
					// Version is up to date
					EditorGUILayout.Separator();
					EditorGUILayout.Separator();

					EditorGUILayout.LabelField("Current Version: " + UAP_AccessibilityManager.PluginVersion);
					EditorGUILayout.LabelField("Latest Version: " + m_LatestVersion);
					EditorGUILayout.Separator();
					EditorGUILayout.LabelField("Your version is up to date.");

					EditorGUILayout.Separator();
					EditorGUILayout.Separator();
				}
			}
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
			if (ass.GetType("UILabel") != null)
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