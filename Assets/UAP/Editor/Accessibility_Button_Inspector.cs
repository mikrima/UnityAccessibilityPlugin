using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AccessibleButton)), CanEditMultipleObjects]
public class Accessibility_Button_Inspector : Accessibility_InspectorShared
{
	//////////////////////////////////////////////////////////////////////////

	void OnEnable()
	{
	}

	//////////////////////////////////////////////////////////////////////////

	public override void OnInspectorGUI()
	{
		SetupGUIStyles();
		serializedObject.Update();

		EditorGUILayout.Separator();
		EditorGUILayout.LabelField("Button", myHeadingStyle);
		EditorGUILayout.Separator();

		// Name
		DrawNameSection();

		// Reference / Target 
		DrawTargetSection("Button");

		// Positioning / Traversal
		DrawPositionOrderSection();

		// Speech Output
		DrawSpeechOutputSection();

		// Callbacks
		DrawCallbackSection(true);


		serializedObject.ApplyModifiedProperties();
		DrawDefaultInspectorSection();
	}

}
