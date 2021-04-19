using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlatformTextSwitcher : MonoBehaviour
{
	public Text m_Label = null;
	public string m_WindowsText = "";
	public string m_iOSText = "";
	public string m_AndroidText = "";

	//////////////////////////////////////////////////////////////////////////

	void OnEnable()
	{
		if (m_Label == null)
			return;

#if !UNITY_EDITOR
		m_Label.text = m_WindowsText;
		return;
#else
		if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WebGLPlayer)
		{
			m_Label.text = m_WindowsText;
			return;
		}

		if (Application.platform == RuntimePlatform.Android)
			m_Label.text = m_AndroidText;


		if (Application.platform == RuntimePlatform.IPhonePlayer)
			m_Label.text = m_iOSText;
#endif
	}
}
