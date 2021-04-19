using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Events;
using UnityEngine.EventSystems;

//[AddComponentMenu("Accessibility/Core/UAP Base Element")]
public abstract class UAP_BaseElement_3D : UAP_BaseElement
{
	public Camera m_CameraRenderingThisObject = null;

	//////////////////////////////////////////////////////////////////////////

	public override bool AutoFillTextLabel()
	{
		if (string.IsNullOrEmpty(m_Text))
		{
			m_Text = gameObject.name;
		}
		return true;
	}

	//////////////////////////////////////////////////////////////////////////

	public override bool Is3DElement()
	{
		return true;
	}

	//////////////////////////////////////////////////////////////////////////

	public float GetPixelHeight()
	{
		return Screen.height / 8.0f;
	}


	public float GetPixelWidth()
	{
		return Screen.height / 8.0f;
	}

	//////////////////////////////////////////////////////////////////////////

	public override void HoverHighlight(bool enable, EHighlightSource selectionSource)
	{

		// #UAP 3D object might need an additional callback (OnMoseEnter etc)

		EventTrigger eventTrigger = null;
		if (m_ReferenceElement != null && m_ReferenceElement.activeInHierarchy)
			eventTrigger = m_ReferenceElement.GetComponent<EventTrigger>();
		if (eventTrigger == null && gameObject.activeInHierarchy)
			eventTrigger = gameObject.GetComponent<EventTrigger>();
		if (eventTrigger != null)
		{
			if (enable)
				eventTrigger.OnSelect(new BaseEventData(EventSystem.current) { selectedObject = eventTrigger.gameObject });
			else
				eventTrigger.OnDeselect(new BaseEventData(EventSystem.current) { selectedObject = eventTrigger.gameObject });
		}

		OnHoverHighlight(enable);

		// Callback to listeners, if any
		m_CallbackOnHighlight.Invoke(enable, selectionSource);
	}

	//////////////////////////////////////////////////////////////////////////

	protected override string GetLabelText(GameObject go)
	{
		// #UAP 3D will probably work differently

		if (go == null)
			return "";
		Text label = go.GetComponent<Text>();
		if (label != null)
			return label.text;

#if ACCESS_NGUI

		UILabel nGUIlabel = null;
		nGUIlabel = go.GetComponent<UILabel>();
		if (nGUIlabel != null)
			return nGUIlabel.text;

#endif

		return "";
	}

	//////////////////////////////////////////////////////////////////////////

}
