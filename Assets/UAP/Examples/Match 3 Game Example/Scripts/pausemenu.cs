using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class pausemenu : MonoBehaviour
{
	public Image m_SoundToggle = null;
	public Sprite m_SoundOn = null;
	public Sprite m_SoundOff = null;
	public UAP_BaseElement m_SoundToggleButton_Access = null;

	//////////////////////////////////////////////////////////////////////////

	void OnEnable()
	{
		EnableMusic(PlayerPrefs.GetInt("Music_Enabled", 1) == 1);
		UAP_AccessibilityManager.RegisterOnPauseToggledCallback(OnUserPause);
	}

	void OnDisable()
	{
		UAP_AccessibilityManager.UnregisterOnPauseToggledCallback(OnUserPause);
	}

	//////////////////////////////////////////////////////////////////////////

	public void OnUserPause()
	{
		OnResumeButtonPressed();
	}

	//////////////////////////////////////////////////////////////////////////

	public void OnResumeButtonPressed()
	{
		gameplay.Instance.ResumeGame();
		DestroyImmediate(gameObject);
	}

	//////////////////////////////////////////////////////////////////////////

	public void OnAbortGameButtonPressed()
	{
		DestroyImmediate(gameObject);
		gameplay.Instance.AbortGame();
	}

	//////////////////////////////////////////////////////////////////////////

	public void OnSoundToggle()
	{
		EnableMusic(!(PlayerPrefs.GetInt("Music_Enabled", 1) == 1));
	}

	void EnableMusic(bool enable)
	{
		m_SoundToggleButton_Access.m_Text = enable ? "Turn Music Off" : "Turn Music On";
		m_SoundToggle.sprite = enable ? m_SoundOn : m_SoundOff;
		PlayerPrefs.SetInt("Music_Enabled", enable ? 1 : 0);
		PlayerPrefs.Save();
	}

	//////////////////////////////////////////////////////////////////////////

}
