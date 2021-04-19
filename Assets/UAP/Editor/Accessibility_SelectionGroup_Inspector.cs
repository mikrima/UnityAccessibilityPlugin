using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(UAP_SelectionGroup)), CanEditMultipleObjects]
public class Accessibility_SelectionGroup_Inspector : Accessibility_InspectorShared
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
		EditorGUILayout.LabelField("Group Focus Notification", myHeadingStyle);
		EditorGUILayout.Separator();

		EditorGUILayout.HelpBox("Add this component to any GameObject in your UI hierarchy if you want to be notified when any UI element on or beneath it receives focus. This allows you to pay extra sound effects etc as additional feedback for blind users.\n\nThe component will broadcast a message 'Accessibility_Selected' to all components on this GameObject when the focus state changes.", MessageType.Info);
		EditorGUILayout.Separator();

		/*
		// Name
		DrawNameSection();

		// Positioning / Traversal
		DrawPositionOrderSection();

		// Speech Output
		DrawSpeechOutputSection();


		serializedObject.ApplyModifiedProperties();
		DrawDefaultInspectorSection();
		*/ 
	}

}
