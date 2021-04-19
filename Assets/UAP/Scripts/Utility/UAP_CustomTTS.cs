using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for custom TTS implementations. DO NOT MODIFY THIS FILE!
/// Please use or make a copy of Custom_TTS_Template.cs instead!
/// </summary>
public class UAP_CustomTTS : MonoBehaviour
{
	/// <summary>
	/// Custom Text-to-Speech Initialization state.
	/// NotInitialized - this TTS implementation cannot be used<br>
	/// InProgress - initialization is in progress, but not ready to synthesize speech (no calls to the SpeakText() function will be made)<br>
	/// Initialized - ready to synthesize speech<br>
	/// </summary>
	public enum TTSInitializationState
	{
		NotInitialized,
		InProgress,
		Initialized,
	}

	protected static UAP_CustomTTS Instance = null;

	//////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Initialize anything that needs to be done only once at the start
	/// </summary>
	protected virtual void Initialize()
	{
	}

	/// <summary>
	/// Return whether or not the initialization is finished, or pending.
	/// If using a server based TTS method, the app might need to wait for a callback
	/// before initialization is complete. Returning 'InProgress' will tell
	/// the plugin to queue and wait any text that needs to be spoken.
	/// </summary>
	/// <returns>
	/// TTSInitializationState.Initialized - ready to speak text<br>
	/// TTSInitializationState.InProgress - Waiting, should be completed soon<br>
	/// TTSInitializationState.NotInitalized - this TTS system is not available<br>
	/// </returns>
	protected virtual TTSInitializationState GetInitializationStatus()
	{
		return TTSInitializationState.NotInitialized;
	}

	/// <summary>
	/// Synthesize and speak the provided text.
	/// </summary>
	/// <param name="textToSay">Text string that is to be spoken.</param>
	/// <param name="speakRate">Speed of speech, between 0..2, with 1.0 being normal speed</param>
	protected virtual void SpeakText(string textToSay, float speakRate)
	{
	}

	/// <summary>
	/// Stop speaking (if anything is currently playing)
	/// </summary>
	protected virtual void StopSpeaking()
	{
	}

	/// <summary>
	/// Return true if currently speaking (or about to speak)
	/// </summary>
	protected virtual bool IsCurrentlySpeaking()
	{
		return false;
	}

	//////////////////////////////////////////////////////////////////////////

	public static void InitializeCustomTTS<T>()
	{
		if (Instance != null)
			return;

		GameObject newTTSGO = new GameObject("Custom TTS");
		newTTSGO.AddComponent(typeof(T));
		Instance = newTTSGO.GetComponent<UAP_CustomTTS>();

		if (Instance == null)
		{
			Debug.LogError("[TTS] Error creating custom TTS system. " + typeof(T).ToString() + " is not derived from UAP_CustomTTS");
			return;
		}

		Debug.Log("[TTS] Initializing Custom TTS");

		Instance.Initialize();
		DontDestroyOnLoad(newTTSGO);
	}

	//////////////////////////////////////////////////////////////////////////

	static public void Speak(string textToSay, float speakRate)
	{
		if (Instance != null)
			Instance.SpeakText(textToSay, speakRate);
	}

	static public void Stop()
	{
		if (Instance != null)
			Instance.StopSpeaking();
	}

	static public bool IsSpeaking()
	{
		if (Instance != null)
			return Instance.IsCurrentlySpeaking();

		return false;
	}

	//////////////////////////////////////////////////////////////////////////

	public static TTSInitializationState IsInitialized()
	{
		if (Instance == null)
			return TTSInitializationState.NotInitialized;

		return Instance.GetInitializationStatus();
	}

	//////////////////////////////////////////////////////////////////////////
	
	void OnDestroy()
	{
		if (Instance == this)
			Instance = null;
	}

	//////////////////////////////////////////////////////////////////////////

}
