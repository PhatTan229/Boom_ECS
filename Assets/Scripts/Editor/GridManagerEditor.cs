using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridManager))]
public class GridManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var gridManager = (GridManager)target;

        if(GUILayout.Button("Spawn Grid"))
        {
            gridManager.SpawnGrid();
        }
        if(GUILayout.Button("Clear Grid"))
        {
            gridManager.ClearGrid();
        }
    }
}
