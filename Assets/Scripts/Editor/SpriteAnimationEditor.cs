using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(SpriteAnimationAuthoring))]
public class SpriteAnimationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var target = (SpriteAnimationAuthoring)this.target;
        base.OnInspectorGUI();

        if (GUILayout.Button("Read Material"))
        {
            target.GetAniamtionStates();
        }
    }
}
