using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AccessibleButton_3D)), CanEditMultipleObjects]
public class Accessibility_Button_3D_Inspector : Accessibility_InspectorShared
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
		DrawNameSection(false);

		// Reference / Target - not applicable for 3D objects
		//DrawTargetSection("Button");

		// Positioning / Traversal
		DrawPositionOrderSection(false);

		// Speech Output
		DrawSpeechOutputSection();

		// Callbacks
		DrawCallbackSection(true);

		serializedObject.ApplyModifiedProperties();
		DrawDefaultInspectorSection();
	}

}
