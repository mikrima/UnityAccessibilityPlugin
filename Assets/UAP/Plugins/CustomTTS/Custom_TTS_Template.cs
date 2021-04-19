using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Duplicate and rename this class, and fill in the function bodies with your own implementation.
/// To activate, modify the function InitializeCustomTTS() in UAP_AudioQueue.cs to point to this class.
/// </summary>
public class Custom_TTS_Template : UAP_CustomTTS
{
	//////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Initialize anything that needs to be done only once at the start
	/// </summary>
	protected override void Initialize()
	{
		throw new System.Exception("The method or operation is not implemented.");
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
		throw new System.Exception("The method or operation is not implemented.");
	}

	//////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Stop speaking (if anything is currently playing)
	/// </summary>
	protected override void StopSpeaking()
	{
		throw new System.Exception("The method or operation is not implemented.");
	}

	//////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Return true if currently speaking (or about to speak)
	/// </summary>
	protected override bool IsCurrentlySpeaking()
	{
		throw new System.Exception("The method or operation is not implemented.");
	}
}
