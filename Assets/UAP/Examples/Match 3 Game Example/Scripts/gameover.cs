using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class gameover : MonoBehaviour
{
	public GameObject m_GameLostHeading = null;
	public GameObject m_GameWonHeading = null;
	public GameObject m_GameLostText = null;
	public GameObject m_GameWonText = null;
	public Text m_MovesLabel = null;
	public Text m_TimeLabel = null;
	public AudioClip m_GameWon = null;
	public AudioClip m_GameLost = null;
	public AudioSource m_AudioPlayer = null;

	static public int MoveCount = 0;
	static public float GameDuration = 0;
	static public bool GameWon = false;

	private int m_WaitingForSilence = 0;

	//////////////////////////////////////////////////////////////////////////

	public void OnReturnButtonPressed()
	{
		DestroyImmediate(gameObject);
		Instantiate(Resources.Load("Main Menu"));
	}

	//////////////////////////////////////////////////////////////////////////

	public void OnPlayAnotherMatchButtonPressed()
	{
		DestroyImmediate(gameObject);
		Instantiate(Resources.Load("Match3"));
	}

	//////////////////////////////////////////////////////////////////////////

	void OnEnable()
	{
		UAP_AccessibilityManager.PauseAccessibility(true);
		m_GameLostHeading.SetActive(!GameWon);
		m_GameWonHeading.SetActive(GameWon);
		m_GameLostText.SetActive(!GameWon);
		m_GameWonText.SetActive(GameWon);
		m_MovesLabel.text = MoveCount.ToString("0");
		m_MovesLabel.GetComponent<UAP_BaseElement>().m_Text = "You made " + MoveCount + " swaps.";
		m_TimeLabel.text = GameDuration.ToString("0") + " s";
		m_TimeLabel.GetComponent<UAP_BaseElement>().m_Text = "Game lasted " + GameDuration.ToString("0") + " seconds.";
		m_WaitingForSilence = 0;
	}

	//////////////////////////////////////////////////////////////////////////

	void Update()
	{
		// Wait for the accessibility manager to stop speaking
		if (m_WaitingForSilence == 0)
		{
			if (!UAP_AccessibilityManager.IsSpeaking())
				m_WaitingForSilence = 1;
			return;
		}

		if (m_WaitingForSilence == 1)
		{
			m_WaitingForSilence = 2;

			// play sound and announce game done
			if (GameWon)
				m_AudioPlayer.PlayOneShot(m_GameWon);
			else
				m_AudioPlayer.PlayOneShot(m_GameLost);

			UAP_AccessibilityManager.Say(GameWon ? "Game Won!" : "Game Over!", false, true, UAP_AudioQueue.EInterrupt.All);

			// unblock the input
			UAP_AccessibilityManager.BlockInput(false);
			UAP_AccessibilityManager.PauseAccessibility(false);
		}
	}
}
