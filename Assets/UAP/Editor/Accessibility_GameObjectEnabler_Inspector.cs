using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(UAP_GameObjectEnabler)), CanEditMultipleObjects]
public class Accessibility_GameObjectEnabler_Inspector : Accessibility_InspectorShared
{
	private SerializedProperty m_ObjectsToEnable;
	private SerializedProperty m_ObjectsToDisable;

	//////////////////////////////////////////////////////////////////////////

	void OnEnable()
	{
		m_ObjectsToEnable = serializedObject.FindProperty("m_ObjectsToEnable");
		m_ObjectsToDisable = serializedObject.FindProperty("m_ObjectsToDisable");
	}

	//////////////////////////////////////////////////////////////////////////

	public override void OnInspectorGUI()
	{
		SetupGUIStyles();
		serializedObject.Update();

		EditorGUILayout.Separator();
		EditorGUILayout.LabelField("GameObject Enabler", myHeadingStyle);
		EditorGUILayout.Separator();

		DrawSectionHeader("Auto-Enable/Disable", true);

		EditorGUILayout.LabelField("If the accessibility plugin is active...");
		++EditorGUI.indentLevel;
		EditorGUILayout.PropertyField(m_ObjectsToEnable, new GUIContent("Enable Objects", "These GameObjects will be enabled if the accessibility plugin is active"), true);
		EditorGUILayout.PropertyField(m_ObjectsToDisable, new GUIContent("Disable Objects", "These GameObjects will be disabled if the accessibility plugin is active"), true);
		--EditorGUI.indentLevel;
		EditorGUILayout.Separator();

		EditorGUILayout.HelpBox("Add this component to automatically enable/disable certain other GameObjects based on the plugin state.\n\nYou can for example show different UI elements to sighted or blind players.", MessageType.Info);
		EditorGUILayout.Separator();


		serializedObject.ApplyModifiedProperties();
		DrawDefaultInspectorSection();
	}

}
