using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridSpawner))]

public class GridSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var target = (GridSpawner)this.target;
        base.OnInspectorGUI();
        
        if(GUILayout.Button("Spawn Grid"))
        {
            target.SpawnGrid();
        }

        if (GUILayout.Button("Spawn Grid 3D"))
        {
            target.SpawnGrid3D();
        }


        if (GUILayout.Button("Reset"))
        {
            GridSpawner.ResetGrid();
        }
    }
}
