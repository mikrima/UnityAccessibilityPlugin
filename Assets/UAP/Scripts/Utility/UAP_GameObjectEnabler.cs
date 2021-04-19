using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Accessibility/Helper/GameObject Enabler")]
public class UAP_GameObjectEnabler : MonoBehaviour
{
	public GameObject[] m_ObjectsToEnable = null;
	public GameObject[] m_ObjectsToDisable = null;

	//////////////////////////////////////////////////////////////////////////

	void Awake()
	{
		SetActiveState();
	}

	void Start()
	{
		SetActiveState();
	}

	//////////////////////////////////////////////////////////////////////////

	void OnEnable()
	{
		UAP_AccessibilityManager.RegisterAccessibilityModeChangeCallback(Accessibility_StateChange);
		SetActiveState();
	}

	//////////////////////////////////////////////////////////////////////////

	private void SetActiveState()
	{
		if (gameObject.activeInHierarchy && this.enabled)
		{
			foreach (GameObject obj in m_ObjectsToEnable)
				if (obj != null)
					obj.SetActive(UAP_AccessibilityManager.IsEnabled());
			foreach (GameObject obj in m_ObjectsToDisable)
				if (obj != null)
					obj.SetActive(!UAP_AccessibilityManager.IsEnabled());
		}
	}

	//////////////////////////////////////////////////////////////////////////

	void OnDisable()
	{
		UAP_AccessibilityManager.UnregisterAccessibilityModeChangeCallback(Accessibility_StateChange);
	}

	//////////////////////////////////////////////////////////////////////////

	public void Accessibility_StateChange(bool newState)
	{
		SetActiveState();
	}
}
