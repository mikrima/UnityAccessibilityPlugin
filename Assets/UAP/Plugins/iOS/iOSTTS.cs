using System.Collections;
using UnityEngine;
using System.Runtime.InteropServices;


public class iOSTTS
{
#if UNITY_IOS

	[DllImport("__Internal")]
	private static extern bool IsVoiceOverRunning();
	[DllImport("__Internal")]
	private static extern bool IsVoiceSpeaking();
	[DllImport("__Internal")]
	private static extern void StopVoiceSpeaking();
	[DllImport("__Internal")]
	private static extern void StartSpeakingVoiceOver(string textToSpeak);
	[DllImport("__Internal")]
	private static extern void StartSpeakingSynthesizer(string textToSpeak, int speechRate);
	[DllImport("__Internal")]
	private static extern void InitializeVoice();
	[DllImport("__Internal")]
	private static extern void ShutdownVoice();

	//////////////////////////////////////////////////////////////////////////

	private static bool m_VoiceOverRunningAtLastCheck = false;

	//////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Check whether VoiceOver is actively running on this device
	/// </summary>
	/// <returns>true if VoiceOver was detected, false otherwise</returns>
	static public bool VoiceOverDetected()
	{
#if !UNITY_EDITOR
		m_VoiceOverRunningAtLastCheck = IsVoiceOverRunning();
#endif
		return m_VoiceOverRunningAtLastCheck;
	}

	//////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Check whether the TTS is currently speaking.
	/// This needs a custom implementation
	/// </summary>
	/// <returns></returns>
	static public bool IsSpeaking()
	{
#if !UNITY_EDITOR
		return IsVoiceSpeaking();
#else
		return false;
#endif
	}

	//////////////////////////////////////////////////////////////////////////

	static public void StopSpeaking()
	{
#if !UNITY_EDITOR
		StopVoiceSpeaking();
#endif
	}

	//////////////////////////////////////////////////////////////////////////

	static public void StartSpeaking(string textToSpeak, bool allowVoiceOver, int speechRate)
	{
#if !UNITY_EDITOR
		if (!allowVoiceOver || !m_VoiceOverRunningAtLastCheck)
			StartSpeakingSynthesizer(textToSpeak, speechRate);
		else
		  StartSpeakingVoiceOver(textToSpeak);
#endif
	}

	//////////////////////////////////////////////////////////////////////////

	static public void Init()
	{
#if !UNITY_EDITOR
		 InitializeVoice();
#endif
		 m_VoiceOverRunningAtLastCheck = VoiceOverDetected();
	}

	//////////////////////////////////////////////////////////////////////////

	static public void Shutdown()
	{
#if !UNITY_EDITOR
		ShutdownVoice();
#endif
	}

	//////////////////////////////////////////////////////////////////////////

#endif
}
