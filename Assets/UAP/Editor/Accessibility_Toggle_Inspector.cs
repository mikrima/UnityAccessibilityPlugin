using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AccessibleToggle)), CanEditMultipleObjects]
public class Accessibility_Toggle_Inspector : Accessibility_InspectorShared
{
	private SerializedProperty m_UseCustomOnOff;
	private SerializedProperty m_CustomOn;
	private SerializedProperty m_CustomOff;
	private SerializedProperty m_CustomHintsAreLocalizationKeys;

	//////////////////////////////////////////////////////////////////////////

	void OnEnable()
	{
		m_UseCustomOnOff = serializedObject.FindProperty("m_UseCustomOnOff");
		m_CustomOn = serializedObject.FindProperty("m_CustomOn");
		m_CustomOff = serializedObject.FindProperty("m_CustomOff");
		m_CustomHintsAreLocalizationKeys = serializedObject.FindProperty("m_CustomHintsAreLocalizationKeys");
	}

	//////////////////////////////////////////////////////////////////////////

	public override void OnInspectorGUI()
	{
		SetupGUIStyles();
		serializedObject.Update();

		EditorGUILayout.Separator();
		EditorGUILayout.LabelField("Toggle", myHeadingStyle);
		EditorGUILayout.Separator();

		DrawToggleInspector();
	}

	//////////////////////////////////////////////////////////////////////////
	
	protected void DrawToggleInspector()
	{
		// Name
		DrawNameSection();

		// Reference / Target 
		DrawTargetSection("Toggle");

		// Positioning / Traversal
		DrawPositionOrderSection();

		// Speech Output
		if (DrawSpeechOutputSection())
		{
			EditorGUILayout.PropertyField(m_UseCustomOnOff, new GUIContent("Custom Status Text", "By default the status will be read as 'Checked' or 'Not Checked'. You can provide custom text here."));
			if (m_UseCustomOnOff.boolValue)
			{
				++EditorGUI.indentLevel;
				EditorGUILayout.PropertyField(m_CustomOn, new GUIContent("Custom Checked Text", "Default is 'Checked'"));
				EditorGUILayout.PropertyField(m_CustomOff, new GUIContent("Custom Unchecked Text", "Default is 'Not Checked'"));
				EditorGUILayout.PropertyField(m_CustomHintsAreLocalizationKeys, new GUIContent("Localization Keys", "If checked, provided texts will be used to look up translations via the localization."));
				--EditorGUI.indentLevel;
			}
		}

		// Callbacks
		DrawCallbackSection(true);


		serializedObject.ApplyModifiedProperties();
		DrawDefaultInspectorSection();
	}
}
