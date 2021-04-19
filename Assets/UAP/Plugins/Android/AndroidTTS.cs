using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AndroidTTS : MonoBehaviour
{
	//////////////////////////////////////////////////////////////////////////

	static public void Initialize()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		var plugin = new AndroidJavaClass("com.metalpopgames.androidtts.AndroidTTS");

		AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

		plugin.CallStatic("InitializeTTS", unityActivity);
#endif
	}

	//////////////////////////////////////////////////////////////////////////

	static public void Shutdown()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		var plugin = new AndroidJavaClass("com.metalpopgames.androidtts.AndroidTTS");
		plugin.CallStatic("ShutdownTTS");
#endif
	}

	//////////////////////////////////////////////////////////////////////////

	static public string GetTTSStatus()
	{
		string statusString = "Only supported on Android";

#if UNITY_ANDROID && !UNITY_EDITOR
		var plugin = new AndroidJavaClass("com.metalpopgames.androidtts.AndroidTTS");
		statusString = plugin.CallStatic<string>("GetInitializationStatus");
#endif

		return statusString;
	}

	//////////////////////////////////////////////////////////////////////////

	static public bool IsSpeaking()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		var plugin = new AndroidJavaClass("com.metalpopgames.androidtts.AndroidTTS");
		return plugin.CallStatic<bool>("IsSpeaking");
#else
		return false;
#endif
	}

	//////////////////////////////////////////////////////////////////////////

	static public void Speak(string text)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		var plugin = new AndroidJavaClass("com.metalpopgames.androidtts.AndroidTTS");
		plugin.CallStatic("Speak", text);
#endif
	}

	//////////////////////////////////////////////////////////////////////////

	static public void StopSpeaking()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		var plugin = new AndroidJavaClass("com.metalpopgames.androidtts.AndroidTTS");
		string text = "";
		plugin.CallStatic("Speak", text);
//		plugin.CallStatic<string>("StopSpeaking");
#endif
	}

	//////////////////////////////////////////////////////////////////////////

	static public void SetSpeechRate(int speechRate)
	{
		// Safety
		if (speechRate < 1)
			speechRate = 1;
		if (speechRate > 100)
			speechRate = 100;

#if UNITY_ANDROID && !UNITY_EDITOR
		float adjustedSpeechRate = (speechRate + 50) / 50.0f;
		var plugin = new AndroidJavaClass("com.metalpopgames.androidtts.AndroidTTS");
		plugin.CallStatic("SetSpeakingRate", adjustedSpeechRate);
#endif
	}

}
