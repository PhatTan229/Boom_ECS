using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(SpriteAnimationAuthoring))]
public class SpriteAnimationEditor : Editor
{
    private SerializedProperty data;
    private SerializedProperty row;
    private SerializedProperty col;
    private int lastDefaultIndex;

    private void OnEnable()
    {
        data = serializedObject.FindProperty("animationStates");
        row = serializedObject.FindProperty("row");
        col = serializedObject.FindProperty("col");
    }

    public override void OnInspectorGUI()
    {
        var target = (SpriteAnimationAuthoring)this.target;

        serializedObject.Update();
        EditorGUILayout.PropertyField(data, new GUIContent("Animation States"), includeChildren: true);

        EditorGUILayout.IntField("Row", row.intValue);
        EditorGUILayout.IntField("Col", col.intValue);

        if (GUILayout.Button("Read Material"))
        {
            target.GetAniamtionStates();
        }

        lastDefaultIndex = 0;
        int defaultCount = 0;

        // Loop and draw custom GUI for each element
        for (int i = 0; i < data.arraySize; i++)
        {
            var element = data.GetArrayElementAtIndex(i);
            var stateNameProp = element.FindPropertyRelative("stateName");
            var isDefaultProp = element.FindPropertyRelative("defaultState");

            EditorGUILayout.BeginVertical("box");
            if (GUILayout.Button($"Create {stateNameProp.stringValue} StateMachine"))
            {
                string stateName = stateNameProp.stringValue;
                CreateStateMachine(stateName);
            }
            EditorGUILayout.EndVertical();

            if (isDefaultProp.boolValue)
            {
                defaultCount++;
                lastDefaultIndex = i;
            }
        }

        // Ensure only one default
        if (defaultCount >= 2)
        {
            Debug.LogError("Only one default is allowed");

            for (int i = 0; i < data.arraySize; i++)
            {
                var isDefaultProp = data.GetArrayElementAtIndex(i).FindPropertyRelative("defaultState");
                isDefaultProp.boolValue = (i == lastDefaultIndex);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    public void CreateStateMachine(string stateName)
    {
        var target = (SpriteAnimationAuthoring)this.target;
        if (!target.TryGetComponent<StateMachineAuthoring>(out var stateMachineAuthoring)) target.AddComponent<StateMachineAuthoring>();

        var rootPath = System.IO.Path.Combine(Application.dataPath, "Scripts", "StateMachine");
        var stateMachineFolder = System.IO.Path.Combine(rootPath, $"{target.transform.root.name}StateMachine");
        if (!Directory.Exists(stateMachineFolder))
        {
            Directory.CreateDirectory(stateMachineFolder);
            Directory.CreateDirectory(System.IO.Path.Combine(stateMachineFolder, "Scripts"));
        }
        var scriptFolder = System.IO.Path.Combine(stateMachineFolder, "Scripts");
        var file = System.IO.Path.Combine(scriptFolder, $"{target.transform.root.name}StateMachineScript_{stateName}.cs");
        if (!File.Exists(file))
        {
            var template = AssetDatabase.LoadAssetAtPath<TextAsset>(System.IO.Path.Combine("Assets", "Editor", "StateMachineTemplate.txt"));
            File.WriteAllText(file, string.Format(template.text, target.transform.root.name, stateName));
            AssetDatabase.Refresh();
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(file, 1);
        }
        var scriptAsset = AssetDatabase.LoadAssetAtPath<MonoScript>(file.Substring(Application.dataPath.Length - "Assets".Length).Replace("\\", "/"));
        EditorGUIUtility.PingObject(scriptAsset);
    }
}
