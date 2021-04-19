using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AccessibleTextEdit)), CanEditMultipleObjects]
public class Accessibility_TextEdit_Inspector : Accessibility_InspectorShared
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
		EditorGUILayout.LabelField("Text Edit", myHeadingStyle);
		EditorGUILayout.Separator();

		// Name
		DrawNameSection();

		// Reference / Target 
		DrawTargetSection("Text Field");

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
