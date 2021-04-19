using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class mainmenu : MonoBehaviour
{
	public Dropdown m_DifficultyDropdown = null;

	//////////////////////////////////////////////////////////////////////////

	void Start()
	{
		gameplay.DifficultyLevel = PlayerPrefs.GetInt("Difficulty", 0);
		m_DifficultyDropdown.value = gameplay.DifficultyLevel;
	}

	//////////////////////////////////////////////////////////////////////////

	public void OnInstructionsButtonPressed()
	{
		if (UAP_AccessibilityManager.IsEnabled())
			Instantiate(Resources.Load("Instructions"));
		else
			Instantiate(Resources.Load("Instructions Sighted"));

		DestroyImmediate(gameObject);
	}

	//////////////////////////////////////////////////////////////////////////

	public void OnQuitButtonPressed()
	{
		UAP_AccessibilityManager.Say("Goodbye");
		Application.Quit();
	}

	//////////////////////////////////////////////////////////////////////////

	public void OnPlayButtonPressed()
	{
		gameplay.DifficultyLevel = m_DifficultyDropdown.value;
		PlayerPrefs.SetInt("Difficulty", gameplay.DifficultyLevel);
		PlayerPrefs.Save();
		Instantiate(Resources.Load("Match3"));
		DestroyImmediate(gameObject);
	}

	//////////////////////////////////////////////////////////////////////////

	public void OnAccessibilityButtonPressed()
	{
		Instantiate(Resources.Load("Accessibility Settings"));
	}

	//////////////////////////////////////////////////////////////////////////

}
