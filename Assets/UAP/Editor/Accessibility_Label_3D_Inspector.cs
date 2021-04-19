using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(AccessibleLabel_3D)), CanEditMultipleObjects]
public class Accessibility_Label_3D_Inspector : Accessibility_InspectorShared
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
		EditorGUILayout.LabelField("Label", myHeadingStyle);
		EditorGUILayout.Separator();
		
		// Name
		DrawNameSection(false);

		// Positioning / Traversal
		DrawPositionOrderSection(false);

		// Speech Output
		DrawSpeechOutputSection();

		// Callbacks
		DrawCallbackSection(false);


		serializedObject.ApplyModifiedProperties();
		DrawDefaultInspectorSection();
	}

}
