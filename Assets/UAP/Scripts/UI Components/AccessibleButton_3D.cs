using UnityEngine;
using System.Collections.Generic;

using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

[AddComponentMenu("Accessibility/UI/Accessible Button 3D")]
public class AccessibleButton_3D : UAP_BaseElement_3D
{

	//////////////////////////////////////////////////////////////////////////

	AccessibleButton_3D()
	{
		m_Type = AccessibleUIGroupRoot.EUIElement.EButton;
	}

	//////////////////////////////////////////////////////////////////////////

	public override bool IsElementActive()
	{
		// Return whether this button is usable, and visible
		if (!base.IsElementActive())
			return false;

		if (m_ReferenceElement != null)
			if (!m_ReferenceElement.gameObject.activeInHierarchy)
				return false;

		if (!UAP_AccessibilityManager.GetSpeakDisabledInteractables())
			if (!IsInteractable())
				return false;

		return true;
	}

	//////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// This just reports this object as interactable - the user still has to set up the appropriate callbacks
	/// </summary>
	public override bool IsInteractable()
	{
		return true;
	}

	//////////////////////////////////////////////////////////////////////////

}


