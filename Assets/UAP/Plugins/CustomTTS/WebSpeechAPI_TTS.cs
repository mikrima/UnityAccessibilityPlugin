using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

/// <summary>
/// Duplicate and rename this class, and fill in the function bodies with your own implementation.
/// To activate, modify the function InitializeCustomTTS() in UAP_AudioQueue.cs to point to this class.
/// </summary>
public class WebSpeechAPI_TTS : UAP_CustomTTS
{

#if UNITY_WEBGL
	// this is supplied by WebSpeechBridge.jslib in the Plugins/WebGL folder
	[DllImport("__Internal")]
	private static extern void SpeakWeb(string str, string langCode/*, float speakRate*/);
	[DllImport("__Internal")]
	private static extern void CancelSpeakWeb();
	[DllImport("__Internal")]
	private static extern bool IsSpeakingWeb();
#endif

	//////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Initialize anything that needs to be done only once at the start
	/// </summary>
	protected override void Initialize()
	{
		// Nothing to do here
	}

	//////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Return whether or not the initialization is finished, or pending.
	/// If using a server based TTS method, the app might need to wait for a callback
	/// before initialization is complete. Returning 'InProgress' will tell
	/// the plugin to queue and wait any text that needs to be spoken.
	/// </summary>
	/// <returns>
	/// UAP_CustomTTS.TTSInitializationState.Initialized - ready to speak text
	/// UAP_CustomTTS.TTSInitializationState.InProgress - Waiting, should be completed soon
	/// UAP_CustomTTS.TTSInitializationState.NotInitalized - this TTS system is not available
	/// </returns>
	protected override TTSInitializationState GetInitializationStatus()
	{
		// Web Speech API needs no initialization
		return UAP_CustomTTS.TTSInitializationState.Initialized;
	}

	//////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Synthesize and speak the provided text.
	/// </summary>
	/// <param name="textToSay">Text string that is to be spoken.</param>
	/// <param name="speakRate">Speed of speech, between 0..2, with 1.0 being normal speed</param>
	protected override void SpeakText(string textToSay, float speakRate)
	{
#if UNITY_WEBGL
		// The jslib only works while in the browser
		if (Application.platform != RuntimePlatform.WebGLPlayer)
			return;

		// #WebSpeechAPI Support speak rate
		//float speakingRate = speakRate;

		// #WebSpeechAPI Support different languages
		string languageCode = "en-US";

		SpeakWeb(textToSay, languageCode/*, speakingRate*/);
#endif
	}

	//////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Stop speaking (if anything is currently playing)
	/// </summary>
	protected override void StopSpeaking()
	{
		// the jslib only works while in the browser
		if (Application.platform != RuntimePlatform.WebGLPlayer)
			return;

#if UNITY_WEBGL
		CancelSpeakWeb();
#endif
	}

	//////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Return true if currently speaking (or about to speak)
	/// </summary>
	protected override bool IsCurrentlySpeaking()
	{
		// the jslib only works while in the browser
		if (Application.platform != RuntimePlatform.WebGLPlayer)
			return false;

#if UNITY_WEBGL
		return IsSpeakingWeb();
#else
		return false;
#endif
	}
}
