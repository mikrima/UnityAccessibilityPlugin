using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.UI;

public class iOSGestures : MonoBehaviour
{
	public Text m_DebugOutputLabel = null;

#if UNITY_IOS

	private UAP_AccessibilityManager m_UAP = null;

#if !UNITY_EDITOR
	[DllImport("__Internal")]
	private static extern void InitGestureRecognition();

	[DllImport("__Internal")]
	private static extern void RemoveGestureRecognition();
#endif

	//////////////////////////////////////////////////////////////////////////

	public void StartRecognition()
	{
#if !UNITY_EDITOR
		InitGestureRecognition();
#endif
	}

	public void StopRecognition()
	{
#if !UNITY_EDITOR
		RemoveGestureRecognition();
#endif
	}

	//////////////////////////////////////////////////////////////////////////

	public void SetUAP(UAP_AccessibilityManager UAP_Instance)
	{
		m_UAP = UAP_Instance;
	}

	//////////////////////////////////////////////////////////////////////////

	void OnSwipeLeftCallback(string fingerCount)
	{
		if (m_DebugOutputLabel != null)
		{
			m_DebugOutputLabel.text += "Swipe Left detected - " + fingerCount + "\n";
		}

		int fingers = 1;
		int.TryParse(fingerCount, out fingers);

		if (m_UAP != null && UAP_AccessibilityManager.IsEnabled())
			m_UAP.OnSwipe(UAP_AccessibilityManager.ESDirection.ELeft, fingers);
	}

	void OnSwipeRightCallback(string fingerCount)
	{
		if (m_DebugOutputLabel != null)
		{
			m_DebugOutputLabel.text += "Swipe Right detected - " + fingerCount + "\n";
		}

		int fingers = 1;
		int.TryParse(fingerCount, out fingers);

		if (m_UAP != null && UAP_AccessibilityManager.IsEnabled())
			m_UAP.OnSwipe(UAP_AccessibilityManager.ESDirection.ERight, fingers);
	}

	void OnSwipeUpCallback(string fingerCount)
	{
		if (m_DebugOutputLabel != null)
		{
			m_DebugOutputLabel.text += "Swipe Up detected - " + fingerCount + "\n";
		}

		int fingers = 1;
		int.TryParse(fingerCount, out fingers);

		if (m_UAP != null && UAP_AccessibilityManager.IsEnabled())
			m_UAP.OnSwipe(UAP_AccessibilityManager.ESDirection.EUp, fingers);
	}

	void OnSwipeDownCallback(string fingerCount)
	{
		if (m_DebugOutputLabel != null)
		{
			m_DebugOutputLabel.text += "Swipe Down detected - " + fingerCount + "\n";
		}

		int fingers = 1;
		int.TryParse(fingerCount, out fingers);

		if (m_UAP != null && UAP_AccessibilityManager.IsEnabled())
			m_UAP.OnSwipe(UAP_AccessibilityManager.ESDirection.EDown, fingers);
	}

	/*
	void OnDoubleTap(string msg)
	{
		if (m_DebugOutputLabel != null)
		{
			m_DebugOutputLabel.text += "Double Tap detected\n";
		}
	}
	*/
	//////////////////////////////////////////////////////////////////////////
	/*
	void OnEscapeGesture(string escape)
	{
		if (m_DebugOutputLabel != null)
		{
			m_DebugOutputLabel.text += "Escape detected\n";
		}
	}
	*/
#endif
}
