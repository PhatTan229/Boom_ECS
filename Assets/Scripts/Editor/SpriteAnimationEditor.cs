using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEditor.Rendering.FilterWindow;


[CustomEditor(typeof(SpriteAnimationAuthoring))]
public class SpriteAnimationEditor : Editor
{
    private SerializedProperty data;
    private int lastDefaultIndex;

    private void OnEnable()
    {
        data = serializedObject.FindProperty("animationStates");
    }

    public override void OnInspectorGUI()
    {
        var target = (SpriteAnimationAuthoring)this.target;
        base.OnInspectorGUI();

        if (GUILayout.Button("Read Material"))
        {
            target.GetAniamtionStates();
        }
        serializedObject.Update();

        lastDefaultIndex = 0; 
        for (int i = 0; i < data.arraySize; i++)
        {
            var element = data.GetArrayElementAtIndex(i);
            var isDefault = element.FindPropertyRelative("defaultState").boolValue;
            if (isDefault)
            {
                lastDefaultIndex = i;
                break;
            }
        }

        var defaultCount = 0;
        for (int i = 0; i < data.arraySize; i++)
        {
            var element = data.GetArrayElementAtIndex(i);
            var isDefault = element.FindPropertyRelative("defaultState").boolValue;
            if (isDefault) defaultCount++;
        }

        if (defaultCount >= 2)
        {
            Debug.LogError("Only one default is allow");
            for (int i = 0; i < data.arraySize; i++)
            {
                if (i == lastDefaultIndex)
                {
                    data.GetArrayElementAtIndex(lastDefaultIndex).FindPropertyRelative("defaultState").boolValue = true;
                    continue;
                }
                var element = data.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("defaultState").boolValue = false;
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
