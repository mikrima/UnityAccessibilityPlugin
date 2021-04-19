using UnityEngine;
using System.Collections;

using UnityEngine.UI;

[AddComponentMenu("Accessibility/UI/Accessible Label 3D")]
public class AccessibleLabel_3D : UAP_BaseElement_3D
{
	//////////////////////////////////////////////////////////////////////////

	AccessibleLabel_3D()
	{
		m_Type = AccessibleUIGroupRoot.EUIElement.ELabel;
	}

	//////////////////////////////////////////////////////////////////////////

	protected override string GetMainText()
	{
		if (IsNameLocalizationKey())
			return CombinePrefix(UAP_AccessibilityManager.Localize(m_Text));
		return m_Text;
	}

	//////////////////////////////////////////////////////////////////////////

}
