using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Map))]
public class MapEditor : Editor
{
    private Map map;
    private ThemeData themeData;

    private void OnEnable()
    {
        map = (Map)target;
        themeData = (ThemeData)serializedObject.FindProperty("theme").objectReferenceValue;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Add Sprite Render Authoring")) AddSpriteRender();
    }

    private void AddSpriteRender()
    {
        var render = target.GetComponentsInChildren<SpriteRenderer>();
        foreach (var r in render) 
        {
            if (!r.TryGetComponent<SpriteRenderAuthoring>(out _)) r.AddComponent<SpriteRenderAuthoring>();
            var authoring = r.GetComponent<SpriteRenderAuthoring>();
            authoring.material = themeData.tileMaterial;
            authoring.mesh = themeData.mesh;
            authoring.transform.eulerAngles = new Vector3(90, 0, 0);
            if (!r.TryGetComponent<TileAuthoring>(out _)) r.AddComponent<TileAuthoring>();
        }
    }
}
