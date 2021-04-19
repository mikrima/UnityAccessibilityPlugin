using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
[AddComponentMenu("Accessibility/Core/UAP Audio Queue")]
public class UAP_AudioQueue : MonoBehaviour
{
	/// <summary>
	/// Read-only - for debugging purposes only. 
	/// </summary>
	public int m_CurrentQueueLength = 0;
	/// <summary>
	/// Read-only - for debugging purposes only. 
	/// </summary>
	public float m_CurrentPauseDuration = 0.0f;
	/// <summary>
	/// Read-only - for debugging purposes only. 
	/// </summary>
	public string m_CurrentElement = "none";
	/// <summary>
	/// Read-only - for debugging purposes only. 
	/// </summary>
	public bool m_IsSpeaking = false;

	/// <summary>
	/// Speech Rate in the range of 1..100, not supported on all platforms.
	/// </summary>
	private int m_SpeechRate = 65;

	private AudioSource m_AudioPlayer = null;
	private Queue<SAudioEntry> m_AudioQueue = new Queue<SAudioEntry>();
	private SAudioEntry m_ActiveEntry = null;

	private float m_PauseTimer = -1.0f;

	private float m_TTS_SpeakingTimer = -1.0f;

#if UNITY_IOS && !UNITY_EDITOR_WIN
	private bool m_LastEntryUsedVoiceOver = false;
#endif

	//////////////////////////////////////////////////////////////////////////

	public enum EAudioType
	{
		None = 0,
		Pause = 1,
		Element_Text = 2,
		Element_Type = 4,
		Element_Hint = 8,
		App = 16, // Use This One In You App
		Container_Name = 32,
		Skippable = 64,
	}

	public enum EInterrupt
	{
		None = 0,
		Elements = EAudioType.Pause | EAudioType.Element_Text | EAudioType.Element_Type | EAudioType.Element_Hint | EAudioType.Skippable,
		All = EAudioType.Pause | EAudioType.Element_Text | EAudioType.Element_Type | EAudioType.Element_Hint | EAudioType.Container_Name | EAudioType.App | EAudioType.Skippable,
	}

	public delegate void UAP_GenericCallback();

	class SAudioEntry
	{
		public string m_TTS_Text = ""; // only applicable if Audio clip is null
		public bool m_AllowVoiceOver = true; // if VoiceOver is enabled, use that to read this entry (true should be the default!)
		public AudioClip m_Audio = null; // only applicable if TTS_Text is empty
		public EAudioType m_AudioType = EAudioType.None;
		public bool m_IsInterruptible = true; // regardless of type, an audio entry can be set to be never interruptible
		public float m_PauseDuration = 0.0f; // Only applicable if type is Pause
		public UAP_GenericCallback m_CallbackOnDone = null;
	}

	//////////////////////////////////////////////////////////////////////////

	public void QueueAudio(string textForTTS, EAudioType type, bool allowVoiceOver, UAP_GenericCallback callbackOnDone = null, EInterrupt interruptsAudioTypes = EInterrupt.None, bool isInterruptible = true)
	{
		//Debug.Log("speaking " + textForTTS);
		// Build struct and call internal add function
		SAudioEntry newEntry = new SAudioEntry();
		newEntry.m_TTS_Text = textForTTS;
		newEntry.m_AllowVoiceOver = allowVoiceOver;
		newEntry.m_AudioType = type;
		newEntry.m_IsInterruptible = isInterruptible;
		newEntry.m_CallbackOnDone = callbackOnDone;
		QueueAudio(newEntry, interruptsAudioTypes);
	}

	public void QueueAudio(AudioClip audioFile, EAudioType type, UAP_GenericCallback callbackOnDone = null, EInterrupt interruptsAudioTypes = EInterrupt.None, bool isInterruptible = true)
	{
		//Debug.Log("playing " + audioFile.name);
		// Build struct and call internal add function	
		SAudioEntry newEntry = new SAudioEntry();
		newEntry.m_Audio = audioFile;
		newEntry.m_AudioType = type;
		newEntry.m_IsInterruptible = isInterruptible;
		newEntry.m_CallbackOnDone = callbackOnDone;
		QueueAudio(newEntry, interruptsAudioTypes);
	}

	public void QueuePause(float durationInSecs)
	{
		// Queue Pause
		SAudioEntry newEntry = new SAudioEntry();
		newEntry.m_AudioType = EAudioType.Pause;
		newEntry.m_PauseDuration = durationInSecs;
		QueueAudio(newEntry, EInterrupt.None);
	}

	//////////////////////////////////////////////////////////////////////////

	public void Stop()
	{
		StopAudio(true);
		m_AudioQueue.Clear();
		InvalidateActiveEntry();
	}

	//////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Stops and removes everything in the queue that is tagged as interruptible (that's almost everything)
	/// </summary>
	public void StopAllInterruptibles()
	{
		// Is there something currently playing/speaking? Stop it.
		if (m_ActiveEntry != null)
		{
			if (m_ActiveEntry.m_IsInterruptible)
			{
				StopAudio();
				InvalidateActiveEntry();
			}
		}

		// Go through the rest of the queue (if any)
		// and re-add only those that are not interruptible
		var audioQueue = m_AudioQueue.ToArray();
		m_AudioQueue.Clear();
		for (int i = 0; i < audioQueue.Length; i++)
		{
			if (!audioQueue[i].m_IsInterruptible)
				m_AudioQueue.Enqueue(audioQueue[i]);
		}

		// Regular Update will take care of starting the next item in queue (if applicable)
	}

	/// <summary>
	/// Interrupt currently playing app announcement made with ::Say().
	/// Only executes if there is an app announcement currently playing and if it is interruptible.
	/// </summary>
	public void InterruptAppAnnouncement()
	{
		// Is there a game announcement currently playing/speaking? Stop it.
		if (m_ActiveEntry != null && m_ActiveEntry.m_AudioType == EAudioType.App)
		{
			if (m_ActiveEntry.m_IsInterruptible)
			{
				StopAudio();
				InvalidateActiveEntry();
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////

	private void QueueAudio(SAudioEntry newEntry, EInterrupt interrupts)
	{
		// check for interrupts and cancel/queue accordingly
		if (interrupts != EInterrupt.None)
		{
			// Stop current element (if any)
			if (m_ActiveEntry != null && m_ActiveEntry.m_IsInterruptible)
			{
				if (((int)m_ActiveEntry.m_AudioType & (int)interrupts) > 0)
				{
					//Debug.Log("Current audio type is " + m_ActiveEntry.m_AudioType + " and it is interruptible");
					StopAudio();
					InvalidateActiveEntry();
				}
			}

			// Go through the queue and remove all entries that match the type
			int entryCount = m_AudioQueue.Count;
			Queue<SAudioEntry> tempQueue = new Queue<SAudioEntry>();
			for (int i = 0; i < entryCount; ++i)
			{
				SAudioEntry entry = m_AudioQueue.Dequeue();
				if (!entry.m_IsInterruptible)
				{
					tempQueue.Enqueue(entry);
				}
				else if (((int)entry.m_AudioType & (int)interrupts) == 0)
				{
					tempQueue.Enqueue(entry);
				}
			}
			m_AudioQueue = tempQueue;
		}

		// Sanity Check - don't queue up empty entries
		if (newEntry.m_AudioType == EAudioType.None)
			return;
		if (newEntry.m_AudioType != EAudioType.Pause && (newEntry.m_Audio == null && newEntry.m_TTS_Text.Length == 0))
			return;

		m_AudioQueue.Enqueue(newEntry);
	}

	//////////////////////////////////////////////////////////////////////////

	private void InvalidateActiveEntry()
	{
		if (m_ActiveEntry != null && m_ActiveEntry.m_CallbackOnDone != null)
			m_ActiveEntry.m_CallbackOnDone();
		m_ActiveEntry = null;
	}

	//////////////////////////////////////////////////////////////////////////

	private void InitializeWindowsTTS()
	{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
		if (WindowsTTS.instance == null)
		{
			GameObject WindowsTTSObj = new GameObject("Windows TTS");
			WindowsTTSObj.AddComponent<WindowsTTS>();
			WindowsTTSObj.transform.SetParent(this.transform, false);
		}
#endif
	}

	//////////////////////////////////////////////////////////////////////////

	public void Initialize()
	{
		m_SpeechRate = PlayerPrefs.GetInt("Accessibility_Speech_Rate", 50);

		if (m_AudioPlayer == null)
			m_AudioPlayer = GetComponent<AudioSource>();

		// Initialize which custom TTS to use, if any
		InitializeCustomTTS();

		//////////////////////////////////////////////////////////////////////////
		// For WebGL, we need a custom TTS solution
		// The following code makes sure that there is either a custom TTS already set up, or it tries to 
		// initialize the included Google Cloud TTS API. This can only be done if an API key is provided.
#if UNITY_WEBGL
		if (Application.platform == RuntimePlatform.WebGLPlayer && UAP_AccessibilityManager.UseWebGLTTS())
		{
			if (UAP_CustomTTS.IsInitialized() == UAP_CustomTTS.TTSInitializationState.NotInitialized)
			{
				// Use Google TTS if set up correctly, or internal TTS otherwise
				if (!string.IsNullOrEmpty(UAP_AccessibilityManager.GoogleTTSAPIKey))
				{
					UAP_CustomTTS.InitializeCustomTTS<Google_TTS>();
				}
				else
				{
					UAP_CustomTTS.InitializeCustomTTS<WebSpeechAPI_TTS>();
				}
			}
		}
#endif
		//////////////////////////////////////////////////////////////////////////

		if (UAP_CustomTTS.IsInitialized() == UAP_CustomTTS.TTSInitializationState.NotInitialized)
		{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
			// Initialize Text To Speech for Windows platforms - if so desired
			/*
			 * No more auto-initialization
						if (UAP_AccessibilityManager.UseWindowsTTS())
						{
							InitializeWindowsTTS();
						}
			*/
#elif (UNITY_STANDALONE_OSX || UNITY_EDITOR) && !UNITY_WEBPLAYER
		// Initialize Text To Speech for Mac platform - if so desired
		if (UAP_AccessibilityManager.UseMacOSTTS())
		{
			if (MacOSTTS.instance == null)
			{
				GameObject MacOSTTSObj = new GameObject("MacOS TTS");
				MacOSTTSObj.AddComponent<MacOSTTS>();
				MacOSTTSObj.transform.SetParent(this.transform, false);
			}
		}
#elif UNITY_ANDROID && !UNITY_EDITOR
		if (UAP_AccessibilityManager.UseAndroidTTS())
		{
			AndroidTTS.Initialize();
		}
#elif UNITY_IOS && !UNITY_EDITOR
		if (UAP_AccessibilityManager.UseiOSTTS())
		{
			iOSTTS.Init();
		}
#endif
		}
	}

	void OnDestroy()
	{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
		// NOP.
#elif UNITY_ANDROID
		if (UAP_AccessibilityManager.UseAndroidTTS())
		{
			AndroidTTS.StopSpeaking();
			AndroidTTS.Shutdown();
		}
#elif UNITY_IOS
		if (UAP_AccessibilityManager.UseiOSTTS())
		{
			iOSTTS.StopSpeaking();
			iOSTTS.Shutdown();
		}
#endif
	}

	//////////////////////////////////////////////////////////////////////////

	void TTS_Speak(string text, bool allowVoiceOver = true)
	{
		if (UAP_CustomTTS.IsInitialized() == UAP_CustomTTS.TTSInitializationState.NotInitialized)
		{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
			if (UAP_AccessibilityManager.UseWindowsTTS())
			{
				InitializeWindowsTTS();
				//Debug.Log("Speaking: " + text);
				WindowsTTS.Speak(text);
			}
#elif (UNITY_STANDALONE_OSX || UNITY_EDITOR) && !UNITY_WEBPLAYER
			if (UAP_AccessibilityManager.UseMacOSTTS())
			{
				//Debug.Log("Speaking: " + text);
				MacOSTTS.instance.Speak(text);
			}
#elif UNITY_ANDROID
			if (UAP_AccessibilityManager.UseAndroidTTS())
			{
				m_TTS_SpeakingTimer += (text.Length * 3.0f / 16.0f);
				AndroidTTS.SetSpeechRate(m_SpeechRate);
				AndroidTTS.Speak(text);
			}
#elif UNITY_IOS
			if (UAP_AccessibilityManager.UseiOSTTS())
			{
				m_TTS_SpeakingTimer += (text.Length * 3.0f / 16.0f);
				iOSTTS.StartSpeaking(text, allowVoiceOver && UAP_AccessibilityManager.IsVoiceOverAllowed(), m_SpeechRate);
			}
#endif
		}
		else
		{
			UAP_CustomTTS.Speak(text, (m_SpeechRate + 50) / 100.0f);
		}
	}

	bool TTS_IsSpeaking()
	{
		if (UAP_CustomTTS.IsInitialized() == UAP_CustomTTS.TTSInitializationState.NotInitialized)
		{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
			if (UAP_AccessibilityManager.UseWindowsTTS() && WindowsTTS.instance != null)
			{
				return WindowsTTS.IsSpeaking();
			}
			else
			{
				return false;
			}
#elif (UNITY_STANDALONE_OSX || UNITY_EDITOR) && !UNITY_WEBPLAYER
			if (UAP_AccessibilityManager.UseMacOSTTS())
				return MacOSTTS.instance.IsSpeaking();
			else
				return false;
#elif UNITY_ANDROID
			return AndroidTTS.IsSpeaking();
			//bool isTTSSpeaking = AndroidTTS.IsSpeaking();
			//if (!isTTSSpeaking)
			//  return false;
			//return m_TTS_SpeakingTimer > 0.0f;
#elif UNITY_IOS
			// VoiceOver sometimes times out without notification
			if (!m_LastEntryUsedVoiceOver)
				return iOSTTS.IsSpeaking();
			// VoiceOver is not 100% reliable unfortunately
			bool VOSpeaking = iOSTTS.IsSpeaking();
			if (!VOSpeaking)
				return false;
			return m_TTS_SpeakingTimer > 0.0f;
#else
			return false;
#endif
		}
		else
		{
			return UAP_CustomTTS.IsSpeaking();
		}
	}

	void StopAudio(bool includingAndroid = false)
	{
		if (m_AudioPlayer.isPlaying)
		{
			m_AudioPlayer.Stop();
			m_AudioPlayer.clip = null;
		}

		if (UAP_CustomTTS.IsInitialized() == UAP_CustomTTS.TTSInitializationState.NotInitialized)
		{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
			if (UAP_AccessibilityManager.UseWindowsTTS() && WindowsTTS.instance != null)
			{
				WindowsTTS.Stop();
			}
#elif (UNITY_STANDALONE_OSX || UNITY_EDITOR) && !UNITY_WEBPLAYER
			if (UAP_AccessibilityManager.UseMacOSTTS())
				MacOSTTS.instance.Stop();
#elif UNITY_ANDROID
			if (UAP_AccessibilityManager.UseAndroidTTS())
			{
				m_TTS_SpeakingTimer = 0.0f;
				if (includingAndroid)
					AndroidTTS.StopSpeaking();
			}
#elif UNITY_IOS
			if (UAP_AccessibilityManager.UseiOSTTS())
			{
				m_TTS_SpeakingTimer = 0.0f;
				iOSTTS.StopSpeaking();
			}
#endif
		}
		else
		{
			UAP_CustomTTS.Stop();
		}
	}

	//////////////////////////////////////////////////////////////////////////

	void Update()
	{
		m_CurrentQueueLength = m_AudioQueue.Count;
		m_CurrentPauseDuration = m_PauseTimer;

		// Check active entry (if any) and check whether audio is still playing (or pause still in effect)
		if (m_ActiveEntry != null)
		{
			if (m_ActiveEntry.m_AudioType == EAudioType.Pause)
			{
				m_CurrentElement = "Pause";
				m_IsSpeaking = false;

				if (!IsPlaying())
				{
					m_PauseTimer -= Time.unscaledDeltaTime;
					if (m_PauseTimer <= 0.0f)
					{
						// Pause is done.
						InvalidateActiveEntry();
					}
				}
			}
			else
			{
				m_CurrentElement = "Voice";

				// Is the voice still playing/speaking?
				bool stillPlaying = IsPlaying();
				m_IsSpeaking = stillPlaying;

				if (!stillPlaying)
					InvalidateActiveEntry();
			}
		}
		else
		{
			m_CurrentElement = "none";
			m_IsSpeaking = false;
		}


		if (m_TTS_SpeakingTimer > 0.0f)
			m_TTS_SpeakingTimer -= Time.unscaledDeltaTime;


		// Update audio queue
		// If the current entry is finished, get the next one from the queue
		if (m_ActiveEntry == null && m_AudioQueue.Count > 0)
		{
			bool dequeue = true;

			// Check whether it's a TTS entry AND we're using a custom TTS AND the TTS is not done initializing
			if (UAP_CustomTTS.IsInitialized() == UAP_CustomTTS.TTSInitializationState.InProgress)
			{
				if (m_AudioQueue.Peek().m_Audio == null)
				{
					// Next up is a TTS Entry, but custom TTS is still busy initializing. Waiting instead.
					dequeue = false;
				}
			}

			if (dequeue)
			{
				m_ActiveEntry = m_AudioQueue.Dequeue();
#if UNITY_IOS && !UNITY_EDITOR_WIN
				m_LastEntryUsedVoiceOver = false;
#endif
				if (m_ActiveEntry.m_AudioType == EAudioType.Pause)
				{
					m_PauseTimer = m_ActiveEntry.m_PauseDuration;
				}
				else
				{
					if (m_ActiveEntry.m_Audio != null)
					{
						m_AudioPlayer.clip = m_ActiveEntry.m_Audio;
						m_AudioPlayer.Play();
					}
					else if (m_ActiveEntry.m_TTS_Text.Length > 0)
					{
#if UNITY_IOS && !UNITY_EDITOR_WIN
					m_LastEntryUsedVoiceOver = m_ActiveEntry.m_AllowVoiceOver;
#endif
						TTS_Speak(m_ActiveEntry.m_TTS_Text, m_ActiveEntry.m_AllowVoiceOver);
					}
				}
			}
			// Done with dequeuing next entry
		}
	}

	//////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Checks whether audio is actively playing, TTS or audio clips, but not counting pauses.
	/// </summary>
	public bool IsPlaying()
	{
		if (m_ActiveEntry == null)
			return false;

		bool stillPlaying = false;
		if (m_ActiveEntry.m_Audio != null)
		{
			stillPlaying = m_AudioPlayer.isPlaying;
		}
		else if (m_ActiveEntry.m_TTS_Text.Length > 0)
		{
			stillPlaying = TTS_IsSpeaking();
		}
		return stillPlaying;
	}

	/// <summary>
	/// Like IsPlaying(), but will return false if the audio playing is a skippable
	/// </summary>
	/// <returns></returns>
	public bool IsPlayingExceptAppOrSkippable()
	{
		if (m_ActiveEntry == null)
			return false;

		bool stillPlaying = false;
		if (m_ActiveEntry.m_Audio != null)
		{
			stillPlaying = m_AudioPlayer.isPlaying;
		}
		else if (m_ActiveEntry.m_TTS_Text.Length > 0)
		{
			stillPlaying = TTS_IsSpeaking();
		}

		if (stillPlaying)
			if (m_ActiveEntry.m_AudioType == EAudioType.Skippable || m_ActiveEntry.m_AudioType == EAudioType.App)
				return false;

		return stillPlaying;
	}

	//////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Checks if there is nothing currently playing or waiting to be played, including pauses.
	/// </summary>
	public bool IsCompletelyEmpty()
	{
		if (m_AudioQueue.Count > 0)
			return false;

		if (IsPlaying())
			return false;

		if (m_ActiveEntry != null)
			return false;

		return true;
	}

	//////////////////////////////////////////////////////////////////////////

	public int GetSpeechRate()
	{
		return m_SpeechRate;
	}

	public int SetSpeechRate(int speechRate)
	{
		m_SpeechRate = speechRate;
		if (m_SpeechRate < 1)
			m_SpeechRate = 1;
		if (m_SpeechRate > 100)
			m_SpeechRate = 100;

		PlayerPrefs.SetInt("Accessibility_Speech_Rate", m_SpeechRate);
		PlayerPrefs.Save();

		return m_SpeechRate;
	}

	//////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// This function is used to activate third-party custom Text-to-Speech engines.
	/// </summary>
	private void InitializeCustomTTS()
	{
		// Set Up Your Custom TTS below
		//////////////////////////////////////////////////////////////////////////
		// Comment in this line and set your OWN derived class in the template 'YOUR_CLASS_NAME' field.
		// Use Custom_TTS_Template as a base for your own implementation

		//UAP_CustomTTS.InitializeCustomTTS<YOUR_CLASS_NAME>();
	}

	//////////////////////////////////////////////////////////////////////////
}

