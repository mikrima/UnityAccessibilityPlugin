using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Text;

/// <summary>
/// Implementation of the Google Cloud Text-to-Speech API for use as a Custom TTS in the UAP plugin
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class Google_TTS : UAP_CustomTTS
{
	private AudioSource m_AudioPlayer = null;

	UnityWebRequest m_CurrentRequest = null;
	private bool m_IsWaitingForSynth = false;

	//////////////////////////////////////////////////////////////////////////

	protected override void Initialize()
	{
		m_AudioPlayer = GetComponent<AudioSource>();
	}

	//////////////////////////////////////////////////////////////////////////

	protected override TTSInitializationState GetInitializationStatus()
	{
		if (m_AudioPlayer == null)
			return UAP_CustomTTS.TTSInitializationState.NotInitialized;
		return UAP_CustomTTS.TTSInitializationState.Initialized;
	}

	//////////////////////////////////////////////////////////////////////////
	
	protected override void SpeakText(string textToSay, float speakRate)
	{
		if (m_AudioPlayer == null)
			return;

		m_IsWaitingForSynth = true;

		string apiKey = UAP_AccessibilityManager.GoogleTTSAPIKey;

		float speakingRate = speakRate;
		string languageCode = "en-US";
		

		string payload = "{\"input\":{\"text\":\"" + textToSay + "\"},\"voice\":{\"languageCode\":\"" + languageCode + "\",\"name\":\"en-US-Wavenet-D\",\"ssmlGender\":1},\"audioConfig\":{\"audioEncoding\":1,\"speakingRate\":" + speakingRate.ToString("0.0") + ",\"pitch\":1.0,\"volumeGainDb\":0.0,\"sampleRateHertz\":24000.0}}";
		byte[] payloadAsBytes = Encoding.UTF8.GetBytes(payload);
		//string url = "https://texttospeech.googleapis.com/v1beta1/text:synthesize?key=" + apiKey;
		string url = "https://texttospeech.googleapis.com/v1/text:synthesize?key=" + apiKey;

		if (m_CurrentRequest != null)
		{
			m_CurrentRequest.Abort();
			m_CurrentRequest.Dispose();
		}
		m_CurrentRequest = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
		m_CurrentRequest.uploadHandler = new UploadHandlerRaw(payloadAsBytes);
		m_CurrentRequest.SetRequestHeader("Content-Type", "application/json");
		m_CurrentRequest.downloadHandler = new DownloadHandlerBuffer();

#if UNITY_2017_2_OR_NEWER
		// Send() was deprecated with Unity 2017.2
		m_CurrentRequest.SendWebRequest();
#else
		m_CurrentRequest.Send();
#endif
	}

	//////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Helper Class for JSON deserialization of Google's response
	/// </summary>
	[System.Serializable]
	public class GoogleTTSSynthesizeResponse
	{
		public string audioContent;
	}
	
	void Update()
	{
		if (!m_IsWaitingForSynth)
			return;

		if (m_CurrentRequest.isDone)
		{
			//Debug.Log("Response received");
			m_IsWaitingForSynth = false;

			// Were there any errors?
			if (m_CurrentRequest.responseCode == 403)
			{
				Debug.LogWarning("[Google TTS] Received response FORBIDDEN from Google. Please check whether your API key restrictions might be blocking this call. You might have set this to only be allowed from your website. If so, you might want to create an unrestricted Editor-only API key which you're using for development.");
			}

			if (!string.IsNullOrEmpty(m_CurrentRequest.error))
			{
				Debug.LogError("[Google TTS] Error Code: " + m_CurrentRequest.responseCode + " - " + m_CurrentRequest.error);
				return;
			}
			if (m_CurrentRequest.downloadHandler.text.Contains("error"))
			{
				Debug.LogError("[Google TTS] Error Code: " + m_CurrentRequest.responseCode + " - " + m_CurrentRequest.downloadHandler.text);
				return;
			}

			GoogleTTSSynthesizeResponse synthResult = (GoogleTTSSynthesizeResponse)JsonUtility.FromJson(m_CurrentRequest.downloadHandler.text, typeof(GoogleTTSSynthesizeResponse));
			if (synthResult != null)
			{				
				WAV wav = new WAV(Convert.FromBase64String(synthResult.audioContent));
				AudioClip audioClip = AudioClip.Create("testSound", wav.SampleCount, wav.ChannelCount, wav.Frequency, false);
				audioClip.SetData(wav.LeftChannel, 0);
				m_AudioPlayer.clip = audioClip;
			}
			else
			{
				Debug.LogError("[Google TTS] Error - no audio received: " + m_CurrentRequest.downloadHandler.text);
				return;
			}

			//Debug.Log("Audio Clip Length " + m_AudioPlayer.clip.length.ToString("0.0#") + " seconds. Loading state: " + m_AudioPlayer.clip.loadState.ToString());
			m_AudioPlayer.Play();

			m_CurrentRequest.Dispose();
			m_CurrentRequest = null;
		}
	}

	//////////////////////////////////////////////////////////////////////////

	protected override void StopSpeaking()
	{
		if (m_AudioPlayer == null)
			return;

		if (m_IsWaitingForSynth)
		{
			if (m_CurrentRequest != null)
			{
				m_CurrentRequest.Abort();
				m_CurrentRequest.Dispose();
			}
			m_CurrentRequest = null;
			m_IsWaitingForSynth = false;
		}

		m_AudioPlayer.Stop();
	}

	protected override bool IsCurrentlySpeaking()
	{
		if (m_AudioPlayer == null)
			return false;

		return m_IsWaitingForSynth || m_AudioPlayer.isPlaying;
	}

	//////////////////////////////////////////////////////////////////////////
}

//////////////////////////////////////////////////////////////////////////
// Helper class that converts Google's response into a Unity Audio Clip
//////////////////////////////////////////////////////////////////////////

public class WAV
{

	// convert two bytes to one float in the range -1 to 1
	static float bytesToFloat(byte firstByte, byte secondByte)
	{
		// convert two bytes to one short (little endian)
		short s = (short)((secondByte << 8) | firstByte);
		// convert to range from -1 to (just below) 1
		return s / 32768.0F;
	}

	static int bytesToInt(byte[] bytes, int offset = 0)
	{
		int value = 0;
		for (int i = 0; i < 4; i++)
		{
			value |= ((int)bytes[offset + i]) << (i * 8);
		}
		return value;
	}

	private static byte[] GetBytes(string filename)
	{
		return File.ReadAllBytes(filename);
	}
	// properties
	public float[] LeftChannel { get; internal set; }
	public float[] RightChannel { get; internal set; }
	public int ChannelCount { get; internal set; }
	public int SampleCount { get; internal set; }
	public int Frequency { get; internal set; }

	// Returns left and right double arrays. 'right' will be null if sound is mono.
	public WAV(string filename) :
		this(GetBytes(filename)) { }

	public WAV(byte[] wav)
	{

		// Determine if mono or stereo
		ChannelCount = wav[22];     // Forget byte 23 as 99.999% of WAVs are 1 or 2 channels

		// Get the frequency
		Frequency = bytesToInt(wav, 24);

		// Get past all the other sub chunks to get to the data subchunk:
		int pos = 12;   // First Subchunk ID from 12 to 16

		// Keep iterating until we find the data chunk (i.e. 64 61 74 61 ...... (i.e. 100 97 116 97 in decimal))
		while (!(wav[pos] == 100 && wav[pos + 1] == 97 && wav[pos + 2] == 116 && wav[pos + 3] == 97))
		{
			pos += 4;
			int chunkSize = wav[pos] + wav[pos + 1] * 256 + wav[pos + 2] * 65536 + wav[pos + 3] * 16777216;
			pos += 4 + chunkSize;
		}
		pos += 8;

		// Pos is now positioned to start of actual sound data.
		SampleCount = (wav.Length - pos) / 2;     // 2 bytes per sample (16 bit sound mono)
		if (ChannelCount == 2) SampleCount /= 2;        // 4 bytes per sample (16 bit stereo)

		// Allocate memory (right will be null if only mono sound)
		LeftChannel = new float[SampleCount];
		if (ChannelCount == 2) RightChannel = new float[SampleCount];
		else RightChannel = null;

		// Write to double array/s:
		int i = 0;
		while (pos < wav.Length)
		{
			LeftChannel[i] = bytesToFloat(wav[pos], wav[pos + 1]);
			pos += 2;
			if (ChannelCount == 2)
			{
				RightChannel[i] = bytesToFloat(wav[pos], wav[pos + 1]);
				pos += 2;
			}
			i++;
		}
	}

	public override string ToString()
	{
		return string.Format("[WAV: LeftChannel={0}, RightChannel={1}, ChannelCount={2}, SampleCount={3}, Frequency={4}]", LeftChannel, RightChannel, ChannelCount, SampleCount, Frequency);
	}
}

//////////////////////////////////////////////////////////////////////////
