using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
#if UNITY_2018_3_OR_NEWER
using UnityEngine.Networking;
#endif

[InitializeOnLoad]
public class Accessibility_EditorFunctions
{
	static public string PluginFolder = "Assets/UAP";

	static Texture2D AccessibilityIcon = null;
	static string versionURL = "http://www.metalpopgames.com/assetstore/accessibility/UAP_Version.txt";

	static bool VersionCheckRunning = false;
#if UNITY_2018_3_OR_NEWER
	static UnityWebRequest wwwVersionCheck = null;
#else
	static WWW wwwVersionCheck = null;
#endif
	static string ignoreVersion = "";
	static bool automaticCheck = false;

	//////////////////////////////////////////////////////////////////////////

	static Accessibility_EditorFunctions()
	{
		EditorApplication.update += WaitForLoadingDone;
	}

	//////////////////////////////////////////////////////////////////////////

	static void WaitForLoadingDone()
	{
		if (EditorApplication.isUpdating)
			return;

		EditorApplication.update -= WaitForLoadingDone;
		PerformStartUpRoutine();
	}

	//////////////////////////////////////////////////////////////////////////

	private static void PerformStartUpRoutine()
	{
		EditorApplication.hierarchyWindowItemOnGUI += DrawAccessibilityIcon;

		// Check whether we should show the Welcome Window
		if (EditorPrefs.GetInt("UAP_WelcomeWindowShown_" + PlayerSettings.productName, 0) == 0)
		{
			EditorPrefs.SetInt("UAP_WelcomeWindowShown_" + PlayerSettings.productName, 1);
			EditorPrefs.SetFloat("UAP_Version_" + PlayerSettings.productName, UAP_AccessibilityManager.PluginVersionAsFloat);
			OpenAboutWindow();

			// Don't open any more windows
			return;
		}

		// Was the plugin updated?
		if (EditorPrefs.GetFloat("UAP_Version_" + PlayerSettings.productName, 0.9f) < UAP_AccessibilityManager.PluginVersionAsFloat)
		{
			// Save the current version
			EditorPrefs.SetFloat("UAP_Version_" + PlayerSettings.productName, UAP_AccessibilityManager.PluginVersionAsFloat);

			// Open About window to show "What's New In This Version"
			OpenAboutWindow();

			// Don't open any more windows
			return;
		}

		// If this is a first time install, only perform the update check in 3 dys
		if (!PlayerPrefs.HasKey("UAP_UpdateCheck_NextCheck"))
			PlayerPrefs.SetString("UAP_UpdateCheck_NextCheck", DateTime.Now.AddDays(3).ToBinary().ToString());
		// Is it time for an automated version check?
		string nextCheckTimestamp = PlayerPrefs.GetString("UAP_UpdateCheck_NextCheck", DateTime.Now.ToBinary().ToString());
		long temp = Convert.ToInt64(nextCheckTimestamp);
		DateTime date = DateTime.FromBinary(temp);
		if (date <= DateTime.Now)
		{
			// Re-Check in 1 week
			PlayerPrefs.SetString("UAP_UpdateCheck_NextCheck", DateTime.Now.AddDays(7).ToBinary().ToString());

			// Time for an automatic update check
			ignoreVersion = PlayerPrefs.GetString("UAP_SkipVersion", "0.9.0");
			automaticCheck = true;
			CheckForUpdate();
		}
	}
	//////////////////////////////////////////////////////////////////////////

	[MenuItem("Tools/UAP Accessibility/About")]
	static public void OpenAboutWindow()
	{
		UAP_WelcomeWindow.Init();
	}

	//////////////////////////////////////////////////////////////////////////

	[MenuItem("Tools/UAP Accessibility/Add Accessibility Manager to Scene")]
	static public void AddPrefabToScene()
	{
		if (GameObject.FindObjectOfType<UAP_AccessibilityManager>() != null)
		{
			EditorUtility.DisplayDialog("UAP already present", "There is already an Accessibility Manager in the scene", "OK");
			return;
		}

		PrefabUtility.InstantiatePrefab(Resources.Load("Accessibility Manager"));
	}

	//////////////////////////////////////////////////////////////////////////

	[MenuItem("Tools/UAP Accessibility/Documentation")]
	static public void OpenDocumentation()
	{
		Application.OpenURL("http://www.metalpopgames.com/assetstore/accessibility/doc");
	}

	//////////////////////////////////////////////////////////////////////////

	[MenuItem("Tools/UAP Accessibility/Check For Updates")]
	static public void CheckForUpdate()
	{
		if (VersionCheckRunning)
			return;
		VersionCheckRunning = true;

#if UNITY_2018_3_OR_NEWER
		wwwVersionCheck = new UnityWebRequest(versionURL);
		wwwVersionCheck.downloadHandler = new DownloadHandlerBuffer();
		wwwVersionCheck.SendWebRequest();
#else
		wwwVersionCheck = new WWW(versionURL);
#endif

		UAP_UpdateWindow.StartVersionCheck();
		if (!automaticCheck)
			UAP_UpdateWindow.Init();
		EditorApplication.update += UpdateCheckVersion;
	}

	//////////////////////////////////////////////////////////////////////////

#if ACCESS_NGUI
	//[MenuItem("Tools/UAP Accessibility/Disable NGUI support")]
	static public void DisableNGUISupport()
	{
		UAP_WelcomeWindow.DisableNGUISupport();
	}
#else
	//[MenuItem("Tools/UAP Accessibility/Enable NGUI support")]
	static public void EnableNGUISupport()
	{
		if (EditorUtility.DisplayDialog("Really enable NGUI support?", "Make sure you have NGUI added to your project before you enable UAP NGUI support.\n\nOtherwise the plugin will not compile.", "Go Ahead", "Cancel"))
			UAP_WelcomeWindow.EnableNGUISupport();
	}
#endif

	//////////////////////////////////////////////////////////////////////////

	static public void UpdateCheckVersion()
	{
		if (wwwVersionCheck.isDone)
		{
			EditorApplication.update -= UpdateCheckVersion;

			// Call Found Latest Version
#if UNITY_2018_3_OR_NEWER
			string resultText = wwwVersionCheck.downloadHandler.text;
#else
			string resultText = wwwVersionCheck.text;
#endif
			if (!string.IsNullOrEmpty(resultText) && resultText.StartsWith("VERSION"))
			{
				int index = resultText.IndexOf("\n", 0);
				string latestVersionString = resultText.Substring(8, index - 8);
				string latestVersionRaw = latestVersionString;

				// Remove any dots except the first one
				int firstDot = latestVersionString.IndexOf('.');
				int lastDot = latestVersionString.LastIndexOf('.');
				while (lastDot > firstDot)
				{
					latestVersionString = latestVersionString.Remove(lastDot, 1);
					lastDot = latestVersionString.LastIndexOf('.');
				}

				//Debug.Log("[Accessibility] Version check returned: Latest Version: " + latestVersionRaw + "   Your Version: " + UAP_AccessibilityManager.PluginVersion);
				float latestVersion = UAP_AccessibilityManager.PluginVersionAsFloat;
				if (float.TryParse(latestVersionString, out latestVersion))
				{
					// Check whether this version is supposed to be skipped
					if (!automaticCheck || ignoreVersion.CompareTo(latestVersionRaw) != 0)
					{
						// The window isn't open yet
						if (automaticCheck)
							UAP_UpdateWindow.Init();
						FoundLatestVersion(latestVersion, latestVersionRaw, resultText);
					}
				}
			}
			else
			{
				if (!automaticCheck)
					UAP_UpdateWindow.NoInternet();
				//Debug.LogError("[Accessibility] Could not check for updates, because the online version check URL could not be reached.");
			}

			ignoreVersion = "";
			automaticCheck = false;
			VersionCheckRunning = false;
			wwwVersionCheck = null;
		}
	}

	static public void FoundLatestVersion(float latestVersion, string latestVersionAsString, string fullChangeLog)
	{
		// Display a window with result
		UAP_UpdateWindow.VersionCheckComplete((latestVersion > UAP_AccessibilityManager.PluginVersionAsFloat), latestVersionAsString, fullChangeLog);

		/*
				if (latestVersion > UAP_AccessibilityManager.PluginVersionAsFloat)
				{
					// TODO: Offer download link to the store
					// TODO: Give the option to be reminded again in a day, or skip this version
					Debug.Log("You might want to update your plugin");
				}
				else
					Debug.Log("UAP is up to date");
		*/
	}

	//////////////////////////////////////////////////////////////////////////

	static public void DrawAccessibilityIcon(int instanceID, Rect selectionRect)
	{
		GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
		if (gameObject == null)
			return;

		if (AccessibilityIcon == null)
		{
			string iconPath = PluginFolder + "/Editor/img/UAP_Icon.png";
			AccessibilityIcon = AssetDatabase.LoadAssetAtPath(iconPath, typeof(Texture2D)) as Texture2D;
			if (AccessibilityIcon == null)
			{
				Debug.LogWarning("[Accessibility] Could not load accessibility icon at '" + iconPath + "'");
				return;
			}
		}

		// Check here for Accessibility components on the GameObject
		if (gameObject.GetComponent<UAP_BaseElement>() != null)
		{
			DrawUAPIcon(selectionRect);
			return;
		}
		if (gameObject.GetComponent<AccessibleUIGroupRoot>() != null)
		{
			DrawUAPIcon(selectionRect);
			return;
		}
		if (gameObject.GetComponent<UAP_AccessibilityManager>() != null)
		{
			DrawUAPIcon(selectionRect);
			return;
		}
	}

	private static void DrawUAPIcon(Rect selectionRect)
	{
		{
			// place the icon to the right of the list:
			Rect r = new Rect(selectionRect);
			r.x = r.x + r.width - 20;
			r.width = 18;

			// Draw the texture if it's a light (e.g.)
			GUI.Label(r, AccessibilityIcon);
		}
	}

	//////////////////////////////////////////////////////////////////////////

}
