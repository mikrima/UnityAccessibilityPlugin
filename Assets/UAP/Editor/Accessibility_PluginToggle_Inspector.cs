using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AccessiblePluginToggle)), CanEditMultipleObjects]
public class Accessibility_PluginToggle_Inspector : Accessibility_Toggle_Inspector
{
	private SerializedProperty m_HandleActivation;

	//////////////////////////////////////////////////////////////////////////

	void OnEnable()
	{
		m_HandleActivation = serializedObject.FindProperty("m_HandleActivation");
	}

	//////////////////////////////////////////////////////////////////////////

	public override void OnInspectorGUI()
	{
		SetupGUIStyles();
		serializedObject.Update();

		EditorGUILayout.Separator();
		EditorGUILayout.LabelField("Plugin Toggle", myHeadingStyle);
		EditorGUILayout.Separator();

		EditorGUILayout.HelpBox("Use this if the toggle is used to turn on/off the accessibility plugin.\n\nThis will automatically set the correct toggle state, depending on the plugin state. It will update as well when the plugin is activated or deactivated using the three finger tap gesture instead of the UI.\n\nIt can also enable and disable the plugin automatically.", MessageType.Info);
		EditorGUILayout.Separator();
		m_HandleActivation.boolValue = GUILayout.Toggle(m_HandleActivation.boolValue, new GUIContent("Handle Plugin Activation", "If true, this component will not only set the correct toggle state, but also listen to changes and activate/deactivate the plugin accordingly"));
		EditorGUILayout.Separator();



		serializedObject.ApplyModifiedProperties();
		DrawToggleInspector();
	}

}
