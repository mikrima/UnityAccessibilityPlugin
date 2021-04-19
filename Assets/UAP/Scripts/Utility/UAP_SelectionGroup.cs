using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This helper class sends a message to the GameObject when any accessibility items beneath it receive focus.
/// </summary>
[AddComponentMenu("Accessibility/Helper/Group Focus Notification")]
public class UAP_SelectionGroup : MonoBehaviour
{
	private List<UAP_BaseElement> m_AllElements = new List<UAP_BaseElement>();
	private bool m_Selected = false;

	private GameObject m_LastFocusObject = null;

	///

	public void AddElement(UAP_BaseElement element)
	{
		if (!m_AllElements.Contains(element))
			m_AllElements.Add(element);
	}

	///

	public void RemoveElement(UAP_BaseElement element)
	{
		if (!m_AllElements.Contains(element))
			m_AllElements.Remove(element);
	}

	//

	void Update()
	{
		// Get current element from Manager and check whether it is a child of this group
		GameObject go = UAP_AccessibilityManager.GetCurrentFocusObject();

		// If the object didn't change, nothing needs to be done
		if (go == m_LastFocusObject)
			return;
		m_LastFocusObject = go;

		bool isSelected = false;

		if (UAP_AccessibilityManager.IsEnabled())
		{
			if (go != null)
			{
				UAP_BaseElement element = go.GetComponent<UAP_BaseElement>();
				if (m_AllElements.Contains(element))
				{
					isSelected = true;
				}
			}
		}

		if (isSelected != m_Selected)
		{
			m_Selected = isSelected;
			gameObject.BroadcastMessage("Accessibility_Selected", isSelected, SendMessageOptions.DontRequireReceiver);
		}
	}

	//

	void OnDisable()
	{
		bool isSelected = false;
		gameObject.BroadcastMessage("Accessibility_Selected", isSelected, SendMessageOptions.DontRequireReceiver);
	}

	//

	void OnDestroy()
	{
		bool isSelected = false;
		gameObject.BroadcastMessage("Accessibility_Selected", isSelected, SendMessageOptions.DontRequireReceiver);
	}

}
